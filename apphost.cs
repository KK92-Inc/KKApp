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
builder.AddDockerComposeEnvironment("env");

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

var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./config/student-realm.json")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithExternalHttpEndpoints();

var realm = keycloak.AddRealm("student");

var backend = builder.AddProject<Projects.App_Backend_API>("backend")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WithReference(cache)
    .WithReference(keycloak)
    .WaitFor(migration)
    .WaitFor(postgres)
    .WaitFor(cache)
    .WaitFor(keycloak);

var frontend = builder.AddViteApp("frontend", "./App.Frontend")
    .WaitFor(cache)
    .WithReference(cache)
    .WaitFor(backend)
    .WithReference(backend)
    .WaitFor(keycloak)
    .WithReference(realm)
    // Reverse proxy headers
    .WithEnvironment("S3_ACCESS_KEY_ID", builder.AddParameter("s3-id", secret: true))
    .WithEnvironment("S3_SECRET_ACCESS_KEY", builder.AddParameter("s3-password", secret: true))
    .WithEnvironment("KC_SECRET", builder.AddParameter("keycloak-client-secret", secret: true))
    .WithEnvironment("HOST_HEADER", "x-forwarded-host")
    .WithEnvironment("PROTOCOL_HEADER", "x-forwarded-proto")
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

var scalar = builder.AddScalarApiReference(o => o.WithTheme(ScalarTheme.Kepler))
    .WithReference(keycloak)
    .WithExternalHttpEndpoints()
    .WithApiReference(backend, o =>
    {
        o.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        o.AddPreferredSecuritySchemes("OAuth2");
        o.AddImplicitFlow("OAuth2", flow => flow.WithClientId("intra"));
    });

// ============================================================================


builder.Build().Run();
