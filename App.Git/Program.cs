// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Git.Data;
using App.Git.Endpoints;
using App.Git.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Services Configuration
// ============================================================================

// Database
builder.Services.AddDbContext<GitDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Git service configuration
builder.Services.Configure<GitServiceOptions>(
    builder.Configuration.GetSection(GitServiceOptions.Section));

// Register services
builder.Services.AddScoped<GitService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SshKeyService>();
builder.Services.AddScoped<RepositoryService>();
builder.Services.AddScoped<CollaboratorService>();

// Authentication - JWT Bearer (works with Keycloak, Auth0, etc.)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        // Handle user provisioning on authentication
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();

                var sub = context.Principal?.FindFirst("sub")?.Value;
                var preferredUsername = context.Principal?.FindFirst("preferred_username")?.Value
                    ?? context.Principal?.FindFirst("name")?.Value;
                var email = context.Principal?.FindFirst("email")?.Value;

                if (!string.IsNullOrEmpty(sub) && !string.IsNullOrEmpty(preferredUsername))
                {
                    var user = await userService.GetOrCreateFromExternalAsync(sub, preferredUsername, email);

                    // Add internal user ID as claim for easier access
                    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    identity?.AddClaim(new System.Security.Claims.Claim(
                        System.Security.Claims.ClaimTypes.NameIdentifier,
                        user.Id.ToString()));
                }
            }
        };
    });

builder.Services.AddAuthorization();

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// ============================================================================
// Application Configuration
// ============================================================================

var app = builder.Build();

// Development features
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Auto-migrate database in development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GitDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

// ============================================================================
// API Endpoints
// ============================================================================

// Health check
app.MapGet("/", () => new { status = "ok", service = "git-server", version = "1.0.0" })
    .WithName("HealthCheck")
    .WithTags("Health");

// Repository endpoints: /repos
app.MapGroup("/repos")
    .MapRepositoryEndpoints()
    .WithTags("Repositories");

// Collaborator endpoints (part of repos)
app.MapGroup("/repos")
    .MapCollaboratorEndpoints()
    .WithTags("Collaborators")
    .RequireAuthorization();

// User endpoints: /user (current user) and /users/{username}
app.MapGroup("/user")
    .MapUserEndpoints()
    .WithTags("Users")
    .RequireAuthorization();

// SSH Key endpoints: /user/keys
app.MapGroup("/user/keys")
    .MapSshKeyEndpoints()
    .WithTags("SSH Keys")
    .RequireAuthorization();

// Users lookup (public)
app.MapGet("/users/{username}", async (string username, UserService userService) =>
{
    var user = await userService.GetByUsernameAsync(username);
    if (user == null) return Results.NotFound();
    return Results.Ok(new UserResponse(user.Id, user.Username, user.Email, user.IsAdmin, user.CreatedAt));
})
.WithName("GetUser")
.WithTags("Users");

app.Run();
