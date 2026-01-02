// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Diagnostics;
using System.Text.Json;
using App.Backend.Core;
using App.Backend.Database;
using App.Backend.Database.Interceptors;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Wolverine;
using Microsoft.EntityFrameworkCore;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Implementation;
using System.Threading.RateLimiting;
using Quartz;
using App.Backend.API.Jobs;
using App.Backend.API.Jobs.Extensions;
using Keycloak.AuthServices.Sdk;
using System.Security.Claims;
using App.Backend.API.Schemas.Document;
using App.Backend.API.Schemas.Operation;
using Microsoft.OpenApi;
using Keycloak.AuthServices.Common;

using App.Backend.API.Filters;
using Renci.SshNet;

namespace App.Backend.API;

// ============================================================================

/// <summary>
/// Static service initilization class.
/// </summary>
public static class Services
{
    public static WebApplicationBuilder Register(WebApplicationBuilder builder)
    {
        // Messaging Bus (confusingly named use?)
        builder.Host.UseWolverine();
        builder.Services.AddProblemDetails();
        builder.Services.AddScoped<KeycloakUser>();
        builder.Services.AddControllers(o =>
        {
            o.AddProtectedResources();
            o.Filters.Add<ServiceExceptionFilter>();
            o.Filters.AddService<KeycloakUser>();
        }).AddJsonOptions(o =>
        {
            // Let's us configure the casing for out JSON DTOs for example.
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        // builder.Services
        //     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddKeycloakWebApi(builder.Configuration);

        builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration, options =>
        {
            options.Audience = "intra";
            options.RequireHttpsMetadata = false;
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
                // TODO: Get from config
                if (builder.Environment.IsDevelopment())
                    document.Servers = [new() { Url = "http://localhost:3001" }];
                document.Components ??= new OpenApiComponents();

                var options = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;
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

                return Task.CompletedTask;
            });
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("cache");
            options.InstanceName = "PeerU_";
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
        builder.AddNpgsqlDbContext<DatabaseContext>("peeru-db", null, options =>
        {
            options.UseLazyLoadingProxies();
            options.AddInterceptors(new SavingChangesInterceptor(TimeProvider.System));
            options.UseNpgsql(builder.Configuration.GetConnectionString("peeru-db"));
            if (builder.Environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        });

        // // Register Transient, Scoped, Singletons, ...
        // // builder.Services.AddScoped<ICursusService, CursusService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRuleService, EligibilityService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();
        // // builder.Services.AddScoped<IUserCursusService, UserCursusService>();
        // // builder.Services.AddScoped<IUserGoalService, UserGoalService>();
        // // builder.Services.AddScoped<IUserProjectService, UserProjectService>();
        // // builder.Services.AddScoped<IFeatureService, FeatureService>();
        // // builder.Services.AddScoped<IGoalService, GoalService>();
        // // builder.Services.AddScoped<IFeedbackService, FeedbackService>();
        // // builder.Services.AddScoped<ICommentService, CommentService>();
        // // builder.Services.AddScoped<IProjectService, ProjectService>();
        // // builder.Services.AddScoped<IRubricService, RubricService>();
        // // builder.Services.AddScoped<IReviewService, ReviewService>();
        // // builder.Services.AddScoped<IResourceOwnerService, ResourceOwnerService>();
        // // builder.Services.AddScoped<ISpotlightEventService, SpotlightEventService>();
        // // builder.Services.AddScoped<IGitService, GitService2>();
        // // builder.Services.AddScoped<INotificationService, NotificationService>();
        // // builder.Services.AddScoped<ISpotlightEventActionService, SpotlightEventActionService>();
        // // builder.Services.AddTransient<IResend, ResendClient>();
        // // builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
        builder.Services.AddSingleton(TimeProvider.System);
        // builder.Services.AddSingleton(sp =>
        // {
        //     var sshClient = new SshClient(
        //         builder.Configuration.GetValue<string>("GitService:Host"),
        //         builder.Configuration.GetValue<int>("GitService:Port"),
        //         builder.Configuration.GetValue<string>("GitService:User"),
        //         new PrivateKeyFile(builder.Configuration.GetValue<string>("GitService:PrivateKeyPath"))
        //     );
        //     sshClient.Connect();
        //     return sshClient;
        // });

        // Git Service
        // builder.Services.Configure<GitServiceOptions>(
        //     builder.Configuration.GetSection(GitServiceOptions.SectionName));
        // builder.Services.AddSingleton<IGitService, GitService>();

        // Quartz
        builder.Services.AddQuartz(quartz =>
        {
            quartz.SchedulerName = "NXT";
            quartz.SchedulerId = "Queue";
            quartz.UseDefaultThreadPool(x => x.MaxConcurrency = 5);
            quartz.Register<SampleJob>();
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

// ============================================================================

/// <summary>
/// Exception filter for deep service related exceptions.
/// For example, a service way down in the call stack might need to propegate
/// some form of HTTP Error to the client because further processing isn't
/// possible. Or for example we tried to find something and it isn't there.
///
/// Rather than 'handle' it, we just throw an exception and respond back.
/// </summary>
public class ServiceExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ServiceException serviceException)
            return;

        context.Result = serviceException.StatusCode switch
        {
            StatusCodes.Status403Forbidden => new ForbidResult(),
            StatusCodes.Status404NotFound => new NotFoundResult(),
            _ => new ObjectResult(new ProblemDetails
            {
                Type = $"https://http.cat/{serviceException.StatusCode}",
                Title = serviceException.Message,
                Detail = serviceException.Detail,
                Status = serviceException.StatusCode,
                Instance = context.HttpContext.Request.Path,
                Extensions = new Dictionary<string, object?>
                {
                    ["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
                }
            })
            {
                ContentTypes = { "application/problem+json" }
            }
        };

        context.ExceptionHandled = true;
    }
}
