// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Npgsql@*
#:package Scalar.Aspire@0.7.4
#:package Aspire.Hosting.PostgreSQL@*
#:package CommunityToolkit.Aspire.Hosting.Bun@*
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0
#:project App.Migrations/Migrations.csproj
#:project App.Backend/API/App.Backend.API.csproj

using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// ============================================================================
// 1. Database
// ============================================================================
var dbPassword = builder.AddParameter("db-password", secret: true);
var dbUsername = builder.AddParameter("db-username", secret: true);

var postgres = builder.AddPostgres("postgres-server", dbUsername, dbPassword)
    .WithHostPort(52842)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("peeru-db");

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
var backendApi = builder.AddProject<Projects.App_Backend_API>("backend-api")
    .WithReference(database)
    .WaitFor(migrationService)
    .WithReference(keycloak)
    .WithReference(realm);
    // ------------------------------------------------------------
    // CONFIGURATION FOR CUSTOM DOMAIN & FIXED PORT
    // ------------------------------------------------------------
    // This sets the port to 7001 (HTTPS).
    // With '127.0.0.1 app.test' in your /etc/hosts, you can now
    // set your Keycloak callback URL to:
    // https://app.test:7001/signin-oidc
    // ------------------------------------------------------------
    // .WithHttpsEndpoint(port: 7001, name: "https");

// ============================================================================
// 5. API Documentation (Scalar)
// ============================================================================
var scalar = builder.AddScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.DeepSpace);
    })
    .WithReference(keycloak)
    .WithExternalHttpEndpoints();

scalar.WithApiReference(backendApi, options =>
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
builder.AddBunApp("frontend-app", "./App.Frontend", "dev")
    .WithReference(backendApi)
    .WithHttpEndpoint(env: "PORT", port: 51842)
    .WithExternalHttpEndpoints();

// ============================================================================
// Run Application
// ============================================================================
builder.Build().Run();