// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.1.0
#:package Aspire.Hosting.Docker@13.1.0-preview.1.25616.3
#:package Aspire.Npgsql@13.1.0
#:package Aspire.Hosting.Valkey@13.1.0
#:package Aspire.Hosting.PostgreSQL@13.1.0
#:package CommunityToolkit.Aspire.Hosting.Bun@*
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0
#:project App.Migrations/Migrations.csproj
#:project App.Backend/API/App.Backend.API.csproj
#:package Aspire.Hosting.JavaScript@13.1.0
#:package Scalar.Aspire@0.7.4

using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env");

// ============================================================================
// 1. Database
// ============================================================================
var dbPassword = builder.AddParameter("db-password", secret: true).ExcludeFromManifest();
var dbUsername = builder.AddParameter("db-username", secret: true).ExcludeFromManifest();

var postgres = builder.AddPostgres("postgres-server", dbUsername, dbPassword)
    .WithHostPort(52843)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("peeru-db");

var cache = builder.AddValkey("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================================================
// 2. Keycloak
// ============================================================================
var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./config/student-realm.json")
    .WithExternalHttpEndpoints()
    .WithArgs("--verbose")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin");

var realm = keycloak.AddRealm("student");

// ============================================================================
// 3. Migrations
// ============================================================================
var migrationService = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

// ============================================================================
// 4. Backend API
// ============================================================================
var backend = builder.AddProject<Projects.App_Backend_API>("backend-api")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(migrationService)
    .WithReference(keycloak)
    // This overrides Keycloak:auth-server-url in appsettings.json
    .WithEnvironment("Keycloak__auth-server-url", keycloak.GetEndpoint("http"));

// ============================================================================
// 5. API Documentation (Scalar)
// ============================================================================
var scalar = builder.AddScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.DeepSpace);
    })
    .WithReference(keycloak)
    .WithExternalHttpEndpoints();

scalar.WithApiReference(backend, options =>
{
    options.WithTheme(ScalarTheme.Saturn);
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("OAuth2");
    options.AddImplicitFlow("OAuth2", flow =>
    {
        flow.WithClientId("intra");
        // flow.WithRefreshUrl($"{keycloak()}/realms/student/protocol/openid-connect/token");
    });
});

// ============================================================================
// 6. Frontend
// ============================================================================

var clientSecret = builder.AddParameter("client-secret", secret: true);
if (builder.ExecutionContext.IsPublishMode)
{
    var frontend = builder.AddDockerfile("frontend-app", "./App.Frontend")
        .WithHttpEndpoint(env: "PORT", port: 51842)
        .WithExternalHttpEndpoints();
    ConfigureFrontend(frontend);
    frontend.WithEnvironment("ORIGIN", frontend.GetEndpoint("http"));
}
else
{
    var frontend = builder.AddBunApp("frontend-app", "./App.Frontend", "dev")
        .WithHttpEndpoint(env: "PORT", port: 51842)
        .WithExternalHttpEndpoints();
    ConfigureFrontend(frontend);
    // TODO: ehhh ?
    frontend.WithEnvironment("ORIGIN", "http://frontend-app-peeru.dev.localhost:51842");
}

void ConfigureFrontend<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment, IResourceWithEndpoints, IResourceWithWaitSupport
{
    resource.WaitFor(backend)
        .WaitFor(keycloak)
        .WithReference(cache)
        .WithEnvironment("API", backend.GetEndpoint("http"))
        .WithEnvironment("KC_ID", "intra")
        .WithEnvironment("KC_REALM", "student")
        .WithEnvironment("KC_ORIGIN", keycloak.GetEndpoint("http"))
        .WithEnvironment("KC_SECRET", clientSecret);
}

// ============================================================================
// Run Application
// ============================================================================
builder.Build().Run();
