// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.1.1
#:package Scalar.Aspire@0.8.45
#:package Aspire.Npgsql@13.1.1
#:package Aspire.Hosting.JavaScript@13.1.1
#:package Aspire.Hosting.Docker@13.1.1-preview.1.26105.8
#:package Aspire.Hosting.Valkey@13.1.1
#:package Aspire.Hosting.PostgreSQL@13.1.1
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
var isPublish = builder.ExecutionContext.IsPublishMode;

// Parameters
// ============================================================================

// S3 Storage credentials
var s3Key = builder.AddParameter("s3-access-key-id", true);
var s3Password = builder.AddParameter("s3-secret-access-key", true);

// Keycloak settings
var kcId = builder.AddParameter("kc-id", "intra");
var kcRealm = builder.AddParameter("kc-realm", "student");
var kcSecret = builder.AddParameter("kc-secret", true);
var kcCookie = builder.AddParameter("kc-cookie", "kc.session");

// Resend email token
var resendToken = builder.AddParameter("be-resend-token", true);

// Production-only: external origin overrides
var kcOrigin = isPublish ? builder.AddParameter("kc-origin", true) : null;
var feOrigin = isPublish ? builder.AddParameter("fe-origin", true) : null;

// Storage
// ============================================================================

var postgres = builder.AddPostgres("database")
    .WithDataVolume(name: "database-volume")
    .WithLifetime(ContainerLifetime.Persistent);

if (!isPublish)
{
    // Pin to a consistent host port for tools to avoid reconfiguration
    postgres.WithEndpoint("tcp", e => e.Port = 5432);
}

var cache = builder.AddValkey("valkey")
    .WithDataVolume(name: "cache-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("db");

// Migration
// ============================================================================

var migration = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

// Git Services
// ============================================================================

var api = builder.AddDockerfile("git-api", "./App.Repository", "Dockerfile.api")
    .WithVolume("git-repos", "/home/git/repos")
    .WithHttpEndpoint(targetPort: 3000, name: "http")
    .WithLifetime(ContainerLifetime.Persistent);

var ssh = builder.AddDockerfile("git-ssh", "./App.Repository", "Dockerfile.ssh")
    .WithVolume("git-repos", "/home/git/repos")
    .WithEndpoint(targetPort: 22, scheme: "tcp", name: "ssh")
    .WithReference(database)
    .WaitFor(database)
    .WaitFor(api)
    .WithLifetime(ContainerLifetime.Persistent);

if (!isPublish)
{
    // Pin to a consistent host port so we don't fill our authorized_hosts
    // with garbage
    ssh.WithEndpoint("ssh", e => e.Port = 2222);
}

// Keycloak
// ============================================================================

var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithImport("./Configurations/student-realm.json")
    .WithImport("./Configurations/admin-realm.json")
    .WithExternalHttpEndpoints();

if (isPublish)
{
    // Production: keycloak sits behind a reverse proxy
    keycloak
        .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
        .WithEnvironment("KC_HOSTNAME_STRICT", "false")
        .WithEnvironment("KC_HOSTNAME", kcOrigin!);
}

var realm = keycloak.AddRealm("student");

// Backend
// ============================================================================

var backend = builder.AddProject<Projects.App_Backend_API>("backend")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(keycloak)
    .WithEnvironment("RESEND_APITOKEN", resendToken)
    .WithEnvironment("Git__BaseUrl", api.GetEndpoint("http"))
    .WaitFor(migration)
    .WaitFor(postgres)
    .WaitFor(cache)
    .WaitFor(keycloak)
    .WaitFor(api);

// Frontend
// ============================================================================

var frontendBuilder = builder.AddViteApp("frontend", "./App.Frontend")
    .WaitFor(cache)
    .WithReference(cache)
    .WaitFor(backend)
    .WithReference(backend)
    .WaitFor(keycloak)
    .WithReference(realm)
    .WithEnvironment("KC_ID", kcId)
    .WithEnvironment("KC_REALM", kcRealm)
    .WithEnvironment("KC_SECRET", kcSecret)
    .WithEnvironment("KC_COOKIE", kcCookie)
    .WithEnvironment("S3_ACCESS_KEY_ID", s3Key)
    .WithEnvironment("S3_SECRET_ACCESS_KEY", s3Password)
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

if (isPublish)
{
    // Production: behind a reverse proxy, set explicit origins and proxy headers
    frontendBuilder
        .WithEnvironment("ORIGIN", feOrigin!)
        .WithEnvironment("KC_ORIGIN", kcOrigin!)
        .WithEnvironment("XFF_DEPTH", "1")
        .WithEnvironment("PROTOCOL_HEADER", "x-forwarded-proto")
        .WithEnvironment("HOST_HEADER", "x-forwarded-host")
        .WithEnvironment("PORT_HEADER", "x-forwarded-port")
        .WithEnvironment("ADDRESS_HEADER", "True-Client-IP");
}
else
{
    // Local dev: KC_ORIGIN is resolved via service discovery from the keycloak reference
    // ORIGIN is not needed locally — SvelteKit infers it from the listening address
}

// Scalar API Reference
// ============================================================================

builder.AddScalarApiReference("scalar", o => o.WithTheme(ScalarTheme.Saturn))
    .WithReference(keycloak)
    .WithReference(backend)
    .WithApiReference(backend, o =>
    {
        o.WithDefaultHttpClient(ScalarTarget.C, ScalarClient.HttpClient);
        o.AddPreferredSecuritySchemes("OAuth2");
        o.AddImplicitFlow("OAuth2", flow => flow.WithClientId("intra"));
    });

// ============================================================================

builder.Build().Run();
