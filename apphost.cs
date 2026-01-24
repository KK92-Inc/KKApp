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

using Aspire.Hosting.JavaScript;
using Scalar.Aspire;

// Setup
// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env").WithDashboard(false);
var isRun = builder.ExecutionContext.IsRunMode;

// Paramaters
// ============================================================================


// Configure origin domain for frontend
var kcOrigin = builder.AddParameter("kc-origin", isRun ? "http://localhost:8080" : "https://auth.kk92.net", true);
// Configure origin domain for keycloak
var feOrigin = builder.AddParameter("fe-origin", isRun ? "http://localhost:46783" : "https://intra.kk92.net", true);

// S3 Storage Key
var s3Key = builder.AddParameter("s3-access-key-id", true);
// S3 Storage Password
var s3Password = builder.AddParameter("s3-secret-access-key", true);

// Keycloak primary application id
var kcId = builder.AddParameter("kc-id", "intra");
// Keycloak primary application realm
var kcRealm = builder.AddParameter("kc-realm", "student");
// Keycloak Client Secret generated on the keycloak dashboard
var kcSecret = builder.AddParameter("kc-secret", secret: true);
// Keycloak cookie used for the frontend
var kcCookie = builder.AddParameter("kc-cookie", "kc.session");

// Storage
// ============================================================================

var postgres = builder.AddPostgres("database")
    .WithDataVolume(name: "database-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddValkey("valkey")
    .WithDataVolume(name: "cache-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("db");

// Migration
// ============================================================================

var migration = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

// ============================================================================

var api = builder.AddDockerfile("git-api", "./App.Repository", "Dockerfile.api")
    .WithVolume("git-repos", "/home/git/repos")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WithLifetime(ContainerLifetime.Persistent);

var ssh = builder.AddDockerfile("git-ssh", "./App.Repository", "Dockerfile.ssh")
    .WithVolume("git-repos", "/home/git/repos")
    .WithEndpoint(port: 2222, targetPort: 22, scheme: "tcp", name: "ssh")
    .WithReference(database)
    .WaitFor(database)
    .WaitFor(api)
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================================================

var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./config/student-realm.json")
    .WithImport("./config/admin-realm.json")
    // .WithEnvironment("KC_HTTP_ENABLED", "true")
    // .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    // .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    // .WithEnvironment("KC_HOSTNAME", kcOrigin)
    .WithExternalHttpEndpoints();

var realm = keycloak.AddRealm("student");

var backend = builder.AddProject<Projects.App_Backend_API>("backend")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(keycloak)
    .WaitFor(migration)
    .WaitFor(postgres)
    .WaitFor(cache)
    .WaitFor(keycloak)
    .WaitFor(api);

var frontend = builder.AddViteApp("frontend", "./App.Frontend")
    .WaitFor(cache)
    .WithReference(cache)
    .WaitFor(backend)
    .WithReference(backend)
    .WaitFor(keycloak)
    .WithReference(realm)
    // Reverse proxy headers
    .WithEnvironment("KC_ID", kcId)
    .WithEnvironment("KC_REALM", kcRealm)
    .WithEnvironment("KC_ORIGIN", kcOrigin)
    .WithEnvironment("KC_SECRET", kcSecret)
    .WithEnvironment("KC_COOKIE", kcCookie)
    .WithEnvironment("S3_ACCESS_KEY_ID", s3Key)
    .WithEnvironment("S3_SECRET_ACCESS_KEY", s3Password)
    .WithEnvironment("ORIGIN", feOrigin)
    .WithEnvironment("XFF_DEPTH", "1")
    .WithEnvironment("PROTOCOL_HEADER", "x-forwarded-proto")
    .WithEnvironment("HOST_HEADER", "x-forwarded-host")
    .WithEnvironment("PORT_HEADER", "x-forwarded-port")
    .WithEnvironment("ADDRESS_HEADER", "True-Client-IP")
    //TODO: Remove on Aspire 13.2: https://github.com/dotnet/aspire/issues/13686
    .WithAnnotation(new JavaScriptPackageManagerAnnotation("bun", runScriptCommand: "run", cacheMount: "/root/.bun")
    {
        PackageFilesPatterns =
        {
            new CopyFilePattern("package.json", "./"),
            new CopyFilePattern("bun.lock", "./")
        }
    })
    .WithAnnotation(new JavaScriptInstallCommandAnnotation(["install"]));

// ============================================================================

var scalar = builder.AddScalarApiReference("scalar", o => o.WithTheme(ScalarTheme.Kepler))
    .WithReference(keycloak)
    .WithReference(backend)
    .WithApiReference(backend, o =>
    {
        o.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        o.AddPreferredSecuritySchemes("OAuth2");
        o.AddImplicitFlow("OAuth2", flow => flow.WithClientId("intra"));
    });

// ============================================================================


builder.Build().Run();
