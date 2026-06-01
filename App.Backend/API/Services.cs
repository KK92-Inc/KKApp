// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using System.Security.Claims;
using System.Threading.RateLimiting;

using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;

using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;
using Keycloak.AuthServices.Sdk;

using Quartz;
using Resend;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

using Wolverine;
using Wolverine.Postgresql;

using App.Backend.API.Filters;
using App.Backend.API.Notifications.Registers.Interface;
using App.Backend.API.Notifications.Registers.Implementation;
using App.Backend.API.Schemas.Operation;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Database.Interceptors;
using App.Backend.API.Jobs.Extensions;
using App.Backend.Core.Engines.Evaluations;
using App.Backend.Core.Engines.Evaluations.Rules;
using App.Backend.API.Schemas.Schema;
using App.Backend.API.Schemas.Document;
using Duende.AccessTokenManagement;
using Microsoft.AspNetCore.Authorization;

// ============================================================================

namespace App.Backend.API;

/// <summary>
/// Static service registration class. All DI wiring happens here.
/// </summary>
public static class Services
{
    public static WebApplicationBuilder Register(WebApplicationBuilder builder)
    {
        RegisterCore(builder);
        RegisterAuthentication(builder);
        RegisterOpenApi(builder);
        RegisterCaching(builder);
        RegisterDatabase(builder);
        RegisterMessageBus(builder);
        RegisterDomainServices(builder);
        RegisterScheduling(builder);
        RegisterRateLimiting(builder);
        RegisterLogging(builder);

        return builder;
    }

    // Core
    // ============================================================================

    private static void RegisterCore(WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddRazorTemplating();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResponseCompression();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddControllers(o =>
        {
            o.AddProtectedResources();
            o.Filters.Add<UserResourceFilter>();
            o.Filters.Add<ServiceExceptionFilter>();
        }).AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddHttpClient<GitService>();
        builder.Services.Configure<GitServiceOptions>(
            builder.Configuration.GetSection(GitServiceOptions.SectionName));
        builder.Services.Configure<SubscriptionOptions>(
            builder.Configuration.GetSection(SubscriptionOptions.SectionName));

        builder.Services.AddHttpClient<ResendClient>();
        builder.Services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = builder.Configuration["Resend:Secret"]
                ?? throw new InvalidOperationException("Resend API token is not configured.");
        });
    }

    // Authentication & Authorization
    // ============================================================================

    private static void RegisterAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
        builder.Services
            .AddAuthorization()
            .AddAuthorizationBuilder()
            .AddPolicy("IsStaff", b => b.RequireClaim(ClaimTypes.Role, "staff"))
            .AddPolicy("IsDeveloper", b => b.RequireClaim(ClaimTypes.Role, "developer"));

        builder.Services
            .AddKeycloakAuthorization()
            .AddAuthorizationServer(builder.Configuration);

        // Authenticated admin client, used to manage clients, secrets, etc.
        AddKeycloakAdminHttpClient(builder);
        // UMA protection client, used for resource-level authorization checks.
        AddKeycloakProtectionHttpClient(builder);
    }

    // OpenAPI / Scalar
    // ============================================================================

    private static void RegisterOpenApi(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(o =>
        {
            o.AddDocumentTransformer<InfoDocumentTransformer>();
            o.AddDocumentTransformer<BearerDocumentTransformer>();
            o.AddSchemaTransformer<RequiredDiscriminatorTransformer>();
            o.AddSchemaTransformer<BreakRuleCircularRefTransformer>();
            o.AddOperationTransformer<BasicResponsesOperationTransformer>();

            // Register the Keycloak OAuth2 security scheme.
            // Uses Authorization Code + PKCE instead of Implicit so that Scalar
            // can automatically refresh expired tokens without re-authenticating.
            o.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                var options = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>();
                if (options?.KeycloakUrlRealm is not null)
                {
                    document.Components.SecuritySchemes!.TryAdd("OAuth2", new OpenApiSecurityScheme
                    {
                        Name = "Keycloak Server",
                        OpenIdConnectUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect"),
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                Scopes = new Dictionary<string, string>
                                {
                                    { "openid", "Authenticate using Keycloak" },
                                    { "profile", "Access user profile information" },
                                    { "email", "Access user email address" },
                                    { "roles", "Access user roles" }
                                },
                                AuthorizationUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect/auth"),
                                TokenUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect/token"),
                                RefreshUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect/token"),
                            }
                        }
                    });
                }

                return Task.CompletedTask;
            });
        });
    }

    // Caching
    // ============================================================================
    private static void RegisterCaching(WebApplicationBuilder builder)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = "KKBackend";
            options.Configuration = builder.Configuration.GetConnectionString("cache");
        });

        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(b => b.Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy("NoCache", b => b.NoCache());
            options.AddPolicy("1m", b => b.Expire(TimeSpan.FromMinutes(1)));
            options.AddPolicy("5m", b => b.Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy("30m", b => b.Expire(TimeSpan.FromMinutes(30)));
            options.AddPolicy("1h", b => b.Expire(TimeSpan.FromHours(1)));
        });
    }

    // Database
    // ============================================================================

    private static void RegisterDatabase(WebApplicationBuilder builder)
    {
        var cs = builder.Configuration.GetConnectionString("db");

        builder.AddNpgsqlDbContext<DatabaseContext>("db", null, options =>
        {
            options.UseNpgsql(cs);
            options.UseLazyLoadingProxies();
            options.AddInterceptors(new SshKeyInterceptor());
            options.AddInterceptors(new SavingChangesInterceptor(TimeProvider.System));

            if (builder.Environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        });
    }

    // Message Bus
    // ============================================================================

    private static void RegisterMessageBus(WebApplicationBuilder builder)
    {
        var cs = builder.Configuration.GetConnectionString("db");

        builder.Host.UseWolverine(opts =>
        {
            opts.PersistMessagesWithPostgresql(cs!).EnableMessageTransport(o => o.AutoProvision());
            opts.PublishAllMessages().ToPostgresqlQueue("outbound");
            opts.ListenToPostgresqlQueue("outbound").MaximumMessagesToReceive(50);
        });
    }

    // Domain Services
    // ============================================================================

    private static void RegisterDomainServices(WebApplicationBuilder builder)
    {
        // Infrastructure
        builder.Services.AddScoped<IGitService, GitService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<ISpotlightService, SpotlightService>();
        builder.Services.AddTransient<IResend, ResendClient>();
        builder.Services.AddSingleton<IBroadcastRegistry, MemoryBroadcastRegistry>();

        // User
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IMemberService, MemberService>();
        builder.Services.AddScoped<IUserCursusService, UserCursusService>();
        builder.Services.AddScoped<IUserGoalService, UserGoalService>();
        builder.Services.AddScoped<IUserProjectService, UserProjectService>();

        // Academic
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        builder.Services.AddScoped<IGoalService, GoalService>();
        builder.Services.AddScoped<ICursusService, CursusService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IRubricService, RubricService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();

        // Rules
        builder.Services.AddScoped<IRuleService, RuleService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<RuleEngine>();
        builder.Services.AddScoped<IRuleEvaluator, HasCursusEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, CompletedProjectEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, IsMemberEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, MinDaysRegisteredEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, MinProjectsCompletedEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, MinReviewsCompletedEvaluator>();
        builder.Services.AddScoped<IRuleEvaluator, SameTimezoneEvaluator>();
    }

    // Scheduling
    // ============================================================================

    private static void RegisterScheduling(WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(quartz =>
        {
            quartz.SchedulerId = "Queue";
            quartz.SchedulerName = "KKScheduler";
            quartz.UseDefaultThreadPool(x => x.MaxConcurrency = 5);
        });

        builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
    }

    // Rate Limiting
    // ============================================================================

    private static void RegisterRateLimiting(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("AuthenticatedRateLimit", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.User.Identity?.Name
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));
        });
    }

    // Logging
    // ============================================================================

    private static void RegisterLogging(WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code)
            ));
    }

    // Keycloak HTTP Clients
    // ============================================================================

    /// <summary>
    /// Shared helper: populate a Duende CC client from keycloak appsettings options.
    /// </summary>
    private static void BindKeycloak(ClientCredentialsClient client, KeycloakInstallationOptions options)
    {
        client.ClientId = ClientId.Parse(options.Resource);
        client.ClientSecret = ClientSecret.Parse(options.Credentials.Secret);
        client.TokenEndpoint = new Uri(options.KeycloakTokenEndpoint);
    }

    /// <summary>
    /// Registers an authenticated <see cref="HttpClient"/> named <c>kc_admin</c>
    /// targeting <c>/admin/realms/{realm}/</c>. Token is managed automatically
    /// via client credentials and attached to every outgoing request.
    /// </summary>
    private static void AddKeycloakAdminHttpClient(WebApplicationBuilder builder)
    {
        var name = ClientCredentialsClientName.Parse("kc_admin");
        var options = builder.Configuration.GetKeycloakOptions<KeycloakAdminClientOptions>()!;

        builder.Services
            .AddClientCredentialsTokenManagement()
            .AddClient(name, client => BindKeycloak(client, options));

        builder.Services // NOTE(W2): We avoid using the package because it lacks methods.
            .AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri($"{options.AuthServerUrl}admin/realms/{options.Realm}/");
            })
            .AddClientCredentialsTokenHandler(name);
    }

    /// <summary>
    /// Registers the Keycloak UMA protection client used for resource-level
    /// authorization checks (RPT token introspection, permission queries, etc).
    /// </summary>
    private static void AddKeycloakProtectionHttpClient(WebApplicationBuilder builder)
    {
        var name = ClientCredentialsClientName.Parse("kc_protection");
        var options = builder.Configuration.GetKeycloakOptions<KeycloakProtectionClientOptions>()!;

        builder.Services
            .AddClientCredentialsTokenManagement()
            .AddClient(name, client => BindKeycloak(client, options));

        builder.Services
            .AddKeycloakProtectionHttpClient(options)
            .AddClientCredentialsTokenHandler(name);
    }
}