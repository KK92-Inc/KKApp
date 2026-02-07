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
using App.Backend.API.Schemas.Document;
using App.Backend.API.Schemas.Operation;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;
using App.Backend.Database;
using App.Backend.Database.Interceptors;

// ============================================================================

namespace App.Backend.API;

/// <summary>
/// Static service initilization class.
/// </summary>
public static class Services
{
    public static WebApplicationBuilder Register(WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddRazorTemplating();
        builder.Services.AddProblemDetails();
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
            builder.Configuration.GetSection(GitServiceOptions.SectionName)
        );

        builder.Services.AddHttpClient<ResendClient>();
        builder.Services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = Environment.GetEnvironmentVariable("RESEND_APITOKEN")!;
        });

        // builder.Services
        //     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddKeycloakWebApi(builder.Configuration);

        builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration, options =>
        {
            options.Audience = "intra";
        });

        builder.Services
            .AddAuthorization()
            .AddAuthorizationBuilder()
            .AddPolicy("IsStaff", b => b.RequireClaim(ClaimTypes.Role, "staff"))
            .AddPolicy("IsDeveloper", b => b.RequireClaim(ClaimTypes.Role, "developer"));

        builder.Services
            .AddKeycloakAuthorization()
            .AddAuthorizationServer(builder.Configuration);

        builder.Services
            .AddKeycloakProtectionHttpClient(
                builder.Configuration,
                keycloakClientSectionName: KeycloakProtectionClientOptions.Section
            );

        // Misc Services
        builder.Services.AddOpenApi(o =>
        {
            o.AddDocumentTransformer<InfoSchemeTransformer>();
            o.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            o.AddOperationTransformer<BasicResponsesOperationTransformer>();
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
                                AuthorizationUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect/auth"),
                                TokenUrl = new Uri($"{options.KeycloakUrlRealm}protocol/openid-connect/token"),
                            }
                        }
                    });
                }

                return Task.CompletedTask;
            });
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("cache");
            options.InstanceName = "KKBackend";
        });
        builder.Services.AddResponseCompression();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(b => b.Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy("NoCache", builder => builder.NoCache());
            options.AddPolicy("1m", b => b.Expire(TimeSpan.FromMinutes(1)));
            options.AddPolicy("5m", b => b.Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy("30m", b => b.Expire(TimeSpan.FromMinutes(30)));
            options.AddPolicy("1h", b => b.Expire(TimeSpan.FromHours(1)));
        });

        // Database
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

        // Messaging Bus (confusingly named use?)
        builder.Host.UseWolverine(opts =>
        {
            opts.PersistMessagesWithPostgresql(cs!).EnableMessageTransport(o =>
            {
                o.AutoProvision();
            });

            // Outgoing async messages go here
            opts.PublishAllMessages().ToPostgresqlQueue("outbound");
            // Background workers listen here
            opts.ListenToPostgresqlQueue("outbound").MaximumMessagesToReceive(50);
        });

        // Register Transient, Scoped, Singletons, ...
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRuleService, RuleServiceN>();
        builder.Services.AddScoped<IReviewService, ReviewService>();
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        builder.Services.AddScoped<IGoalService, GoalService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<ICursusService, CursusService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IGitService, GitService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddTransient<IResend, ResendClient>();

        // // builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IBroadcastRegistry, MemoryBroadcastRegistry>();

        // Quartz
        builder.Services.AddQuartz(quartz =>
        {
            quartz.SchedulerName = "NXT";
            quartz.SchedulerId = "Queue";
            quartz.UseDefaultThreadPool(x => x.MaxConcurrency = 5);
            // quartz.Register<SampleJob>();
        });

        builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

        // Rate limit
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("AuthenticatedRateLimit", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));
        });

        // Logging
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                theme: TemplateTheme.Code
            )));

        return builder;
    }
}
