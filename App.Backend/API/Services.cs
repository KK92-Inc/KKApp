// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Diagnostics;
using System.Security.Claims;
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
using Npgsql;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Implementation;
using System.Threading.RateLimiting;

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

        // Misc Services
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();
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

        // Keycloak Auth + Authz
        builder.Services.AddKeycloakAuthorization(builder.Configuration);
        builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("IsStaff", b => b.RequireClaim(ClaimTypes.Role, "staff"))
            .AddPolicy("IsDeveloper", b => b.RequireClaim(ClaimTypes.Role, "developer"));

        // Controller Config
        builder.Services.AddControllers(o =>
        {
            o.AddProtectedResources();
            o.Filters.Add<ServiceExceptionFilter>();
        }).AddJsonOptions(o =>
        {
            // Let's us configure the casing for out JSON DTOs for example.
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        // Database
        builder.AddNpgsqlDbContext<DatabaseContext>("data", null, options =>
        {
            options.UseLazyLoadingProxies();
            options.AddInterceptors(new SavingChangesInterceptor(TimeProvider.System));
            options.UseNpgsql(builder.Configuration.GetConnectionString("data"));
            if (builder.Environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        });

        // Register Transient, Scoped, Singletons, ...
        // builder.Services.AddScoped<ICursusService, CursusService>();
        builder.Services.AddScoped<IUserService, UserService>();
        // builder.Services.AddScoped<IUserCursusService, UserCursusService>();
        // builder.Services.AddScoped<IUserGoalService, UserGoalService>();
        // builder.Services.AddScoped<IUserProjectService, UserProjectService>();
        // builder.Services.AddScoped<IFeatureService, FeatureService>();
        // builder.Services.AddScoped<IGoalService, GoalService>();
        // builder.Services.AddScoped<IFeedbackService, FeedbackService>();
        // builder.Services.AddScoped<ICommentService, CommentService>();
        // builder.Services.AddScoped<IProjectService, ProjectService>();
        // builder.Services.AddScoped<IRubricService, RubricService>();
        // builder.Services.AddScoped<IReviewService, ReviewService>();
        // builder.Services.AddScoped<IResourceOwnerService, ResourceOwnerService>();
        // builder.Services.AddScoped<ISpotlightEventService, SpotlightEventService>();
        // builder.Services.AddScoped<IGitService, GitService2>();
        // builder.Services.AddScoped<INotificationService, NotificationService>();
        // builder.Services.AddScoped<ISpotlightEventActionService, SpotlightEventActionService>();
        // builder.Services.AddTransient<IResend, ResendClient>();
        // builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
        builder.Services.AddSingleton(TimeProvider.System);

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
