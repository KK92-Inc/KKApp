// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.1.0
#:package Scalar.Aspire@0.7.4
#:package Aspire.Npgsql@13.1.0
#:package Aspire.Hosting.JavaScript@13.1.0
#:package Aspire.Hosting.Docker@13.1.0-preview.1.25616.3
#:package Aspire.Hosting.Valkey@13.1.0
#:package Aspire.Hosting.PostgreSQL@13.1.0
#:package CommunityToolkit.Aspire.Hosting.Bun@*
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0
// Project references
#:project App.Migrations/Migrations.csproj
#:project App.Backend/API/App.Backend.API.csproj

using Scalar.Aspire;

// ============================================================================
// Parameters
// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);
var username = builder.AddParameter("postgres-usr", secret: true);
var password = builder.AddParameter("postgres-pwd", secret: true);
var keycloakClientSecret = builder.AddParameter("kc-client-secret", secret: true);

builder.AddDockerComposeEnvironment("env");

// ============================================================================
// PostgreSQL Database
// ============================================================================

var postgres = builder.AddPostgres("postgres-server", username, password)
    .WithDataVolume()
    .WithHostPort(52843)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("peeru-db");

// ============================================================================
// Valkey Cache
// ============================================================================

var cache = builder.AddValkey("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================================================
// Git Server (Soft Serve) TODO: Migrate to custom solution
// ============================================================================
// var git = builder.AddContainer("git", "charmcli/soft-serve", "v0.11.1");

var gitApi = builder.AddDockerfile("git-api", "./App.Repository", "Dockerfile.api")
    .WithVolume("git-repos", "/home/git/repos")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WithLifetime(ContainerLifetime.Persistent);

var gitSsh = builder.AddDockerfile("git-ssh", "./App.Repository", "Dockerfile.ssh")
    .WithVolume("git-repos", "/home/git/repos")
    .WithEndpoint(port: 2222, targetPort: 22, scheme: "tcp", name: "ssh")
    .WithReference(database)
    .WaitFor(database)
    .WaitFor(gitApi)
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================================================
// Keycloak Identity Provider
// ===========================================================================

var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./config/student-realm.json")
    .WithExternalHttpEndpoints()
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin");

var realm = keycloak.AddRealm("student");

// ============================================================================
// Migrations and API Service
// ============================================================================

var migration = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

var api = builder.AddProject<Projects.App_Backend_API>("backend")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(keycloak)
    .WaitFor(migration)
    .WaitFor(postgres)
    .WaitFor(cache)
    .WaitFor(keycloak);

// ============================================================================
// Frontend
// ============================================================================

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
    resource.WaitFor(api)
        .WaitFor(keycloak)
        .WithReference(cache)
        .WithEnvironment("API", api.GetEndpoint("http"))
        .WithEnvironment("KC_ID", "intra")
        .WithEnvironment("KC_REALM", "student")
        .WithEnvironment("KC_ORIGIN", keycloak.GetEndpoint("http"))
        .WithEnvironment("KC_SECRET", keycloakClientSecret);
}

// ============================================================================
// Scalar Hosting
// ============================================================================

var scalar = builder.AddScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.DeepSpace);
    })
    .WithReference(keycloak)
    .WithExternalHttpEndpoints();

scalar.WithApiReference(api, options =>
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

builder.Build().Run();
