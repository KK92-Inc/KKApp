// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.4.6
#:package Scalar.Aspire@0.8.45
#:package Aspire.Npgsql@13.4.6
#:package Aspire.Hosting.Docker@13.4.6
#:package Aspire.Hosting.Valkey@13.4.6
#:package Aspire.Hosting.PostgreSQL@13.4.6
#:package Aspire.Hosting.JavaScript@13.4.6
#:project App.Migrations/Migrations.csproj
#:project App.Git/App.Git.Http/App.Git.Http.csproj
#:project App.Backend/API/App.Backend.API.csproj

using Scalar.Aspire;

// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env").WithDashboard(false);

// The main domain this app is running under.
// Propregates to services which resolve to their own hardcoded subdomains.
var domain = builder.AddParameter("domain", "localhost");
// Object Storage Key.
var s3key = builder.AddParameter("s3-access-key-id", true);
// Object Storage Password.
var s3pwd = builder.AddParameter("s3-secret-access-key", true);
// Secret for the main application intra client on the admin realm.
var adminIntraSecret = builder.AddParameter("kc-admin-intra-secret", true);
// Secret for the login broker that lets staff login to the student realm.
var adminBrokerSecret = builder.AddParameter("kc-admin-broker-secret", true);
// Secret for the main application intra client on the student realm.
var studentIntraSecret = builder.AddParameter("kc-student-intra-secret", true);
// Secret for using resend (i.e: SendGrid) for inbound email transport.
var resendToken = builder.AddParameter("be-resend-token", true);
// Object Storage Key.
var kcLogin = builder.AddParameter("kc-master-login", "admin", true);
// Object Storage Password.
var kcPassword = builder.AddParameter("kc-master-password", "admin", true);
// ============================================================================

var feHostname = builder.ExecutionContext.IsPublishMode
    ? ReferenceExpression.Create($"https://intra.{domain}")
    : ReferenceExpression.Create($"http://frontend-w2inc.dev.{domain}:5173");

var feRedirect = builder.ExecutionContext.IsPublishMode
    ? ReferenceExpression.Create($"https://intra.{domain}/auth/callback")
    : ReferenceExpression.Create($"http://frontend-w2inc.dev.{domain}:5173/auth/callback");

var kcHostname = builder.ExecutionContext.IsPublishMode
    ? ReferenceExpression.Create($"https://auth.{domain}")
    : ReferenceExpression.Create($"http://keycloak-w2inc.dev.{domain}:8080");

// ============================================================================
// Database + Cache
// - Here we configure the database along with the redis compatible cache
// ============================================================================

var postgres = builder.AddPostgres("database", port: 5432)
    .WithDockerfile("./Configurations/Postgres/", "../../Docker/Files/Dockerfile.pg")
    .WithDataVolume(name: "pg-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var cache = builder.AddValkey("valkey", port: 6379)
    .WithDataVolume(name: "cache-volume")
    .WithLifetime(ContainerLifetime.Persistent);

var backendDb = postgres.AddDatabase("db");
var keycloakDb = postgres.AddDatabase("keycloak-db");

// ============================================================================
// S3 Object Storage
// - We self host our own S3 Object storage to avoid costs
// ============================================================================

var storage = builder.AddContainer("rustfs", "rustfs/rustfs", "latest")
    .WithArgs("/data")
    .WithVolume("rustfs-volume", "/data")
    .WithEnvironment("RUSTFS_CONSOLE_ENABLE", "true")
    .WithEnvironment("RUSTFS_ACCESS_KEY", s3key)
    .WithEnvironment("RUSTFS_SECRET_KEY", s3pwd)
    .WithHttpEndpoint(targetPort: 9000, name: "s3")
    .WithHttpEndpoint(targetPort: 9001, name: "console")
    .WithHttpHealthCheck("/health", endpointName: "s3")
    .WithLifetime(ContainerLifetime.Persistent);

// Pin to consistent host ports for `mc`/S3 clients
if (!builder.ExecutionContext.IsPublishMode)
{
    storage.WithEndpoint("s3", e => e.Port = 9000);
    storage.WithEndpoint("console", e => e.Port = 9001);
}

// ============================================================================
// Migrations
// - Handles backend migrations
// ============================================================================

var migration = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(backendDb)
    .WaitFor(postgres)
    .PublishAsDockerComposeService((resource, service) =>
    {
        // Avoid crash-looping, this service only runs once per deploy
        service.Restart = "no";
    });

// ============================================================================
// Identity Provider / Authentication (Keycloak)
// - Keycloak starts FIRST (no WaitFor on FE/BE needed)
// ============================================================================

var auth = builder.AddDockerfile("keycloak", "./Configurations/Keycloak", "../../Docker/Files/Dockerfile.auth")
    .WithVolume("keycloak-volume", "/opt/keycloak/data")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_ADMIN_INTRA_SECRET", adminIntraSecret)
    .WithEnvironment("KC_ADMIN_BROKER_SECRET", adminBrokerSecret)
    .WithEnvironment("KC_STUDENT_INTRA_SECRET", studentIntraSecret)
    .WithEnvironment("FE_URL", feHostname)
    .WithEnvironment("FE_REDIRECT_URL", feRedirect)
    .WithEnvironment("KC_HOSTNAME", kcHostname)
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://database:5432/keycloak-db")
    .WithEnvironment("KC_DB_USERNAME", "postgres")
    .WithEnvironment("KC_DB_PASSWORD", postgres.Resource.PasswordParameter!)
    .WithArgs(builder.ExecutionContext.IsPublishMode
        ? ["start", "--verbose", "--import-realm"]
        : ["start-dev", "--verbose", "--import-realm"]
    );

if (builder.ExecutionContext.IsPublishMode)
    auth // All of it will sit behind a reverse proxy on prod
        .WithEnvironment("KC_HTTP_ENABLED", "true")
        .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
        .WithEnvironment("KC_HOSTNAME_STRICT", "false")
        .WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "false")
        // Allows internal docker-to-docker requests
        .WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true");

// ============================================================================
// Repository
// - For repository projects such as the Git API and SSH Shell
// ============================================================================

var api = builder.AddProject<Projects.App_Git_Http>("git-api")
    .WithHttpHealthCheck("/health")
    .WithReference(backendDb)
    .WaitFor(backendDb);

var ssh = builder.AddDockerfile("git-ssh", ".", "Docker/Files/Dockerfile.ssh")
    .WithReference(backendDb)
    .WithReference(cache)
    .WaitFor(backendDb)
    .WaitFor(cache)
    .WaitFor(api)
    // TODO: Use WithRef instead maybe ?
    .WithEnvironment("KC_ORIGIN", auth.GetEndpoint("http"))
    .WithEnvironment("KC_SECRET", adminIntraSecret)
    .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsPublishMode)
{
    api.WithEnvironment("REPOSITORY_DIRECTORY", "/home/git/repos")
       .PublishAsDockerComposeService((resource, service) =>
       {
           service.Volumes.Add(new()
           {
               Name = "git-repos",
               Target = "/home/git/repos",
               Type = "volume"
           });
       });

    ssh.WithEnvironment("REPOSITORY_DIRECTORY", "/home/git/repos")
        .WithEndpoint(port: 22, targetPort: 22, scheme: "tcp", name: "ssh", isExternal: true)
        .WithVolume("git-repos", "/home/git/repos")
        .WithVolume("git-ssh-keys", "/etc/ssh/keys");
}
else
{
    var dir = Path.Combine(builder.AppHostDirectory, "tmp", "repos");
    api.WithEnvironment("REPOSITORY_DIRECTORY", dir);
    ssh.WithEnvironment("REPOSITORY_DIRECTORY", "/home/git/repos") // container-side path, not host path
        .WithEndpoint(port: 2222, targetPort: 22, scheme: "tcp", name: "ssh", isExternal: true)
        .WithBindMount("./tmp/repos", "/home/git/repos")
        .WithBindMount("./Configurations/Shell/Keys", "/etc/ssh/keys");
}


// ============================================================================
// Backend
// - Backend waits for DB, Migrations, and Keycloak
// ============================================================================

var backend = builder.AddProject<Projects.App_Backend_API>("backend")
    .WithHttpHealthCheck("/health")
    .WithReference(backendDb)
    .WithReference(cache)
    .WithEnvironment("KeycloakAdmin__credentials__secret", adminIntraSecret)
    .WithEnvironment("KeycloakAdmin__auth-server-url", auth.GetEndpoint("http"))
    .WithEnvironment("KeycloakStudent__credentials__secret", studentIntraSecret)
    .WithEnvironment("KeycloakStudent__auth-server-url", auth.GetEndpoint("http"))
    .WithEnvironment("Resend__Secret", resendToken)
    .WithEnvironment("Git__BaseUrl", api.GetEndpoint("http"))
    .WaitFor(migration)
    .WaitFor(postgres)
    .WaitFor(cache)
    .WaitFor(auth)
    .WaitFor(api);

// ============================================================================
// Frontend
// - Frontend waits for Backend & Keycloak
// ============================================================================

var frontend = builder.AddViteApp("frontend", "./App.Frontend")
    .WithArgs("--host")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("KC_SECRET", studentIntraSecret)
    .WithEnvironment("KC_ORIGIN", kcHostname)
    .WithEnvironment("S3_ACCESS_KEY_ID", s3key)
    .WithEnvironment("S3_SECRET_ACCESS_KEY", s3pwd)
    .WithEnvironment("PUBLIC_GIT_URL", $"git@{domain}")
    .WithEnvironment("PUBLIC_API_URL", backend.GetEndpoint("http"))
    .WithEnvironment("PUBLIC_S3_ENDPOINT", storage.GetEndpoint("s3"))
    .WithEnvironment("ORIGIN", feHostname)
    .WithReference(backend)
    .WithReference(cache)
    .WaitFor(backend)
    .WaitFor(auth)
    .WaitFor(cache)
    .WithBun();

if (!builder.ExecutionContext.IsPublishMode)
    // Pin so frontend on 5173 is real, not guessed
    frontend.WithEndpoint("http", e => e.Port = 5173);
else
{
    frontend // On prod will sit behind a reverse proxy
        .WithEnvironment("XFF_DEPTH", "1")
        .WithEnvironment("PROTOCOL_HEADER", "x-forwarded-proto")
        .WithEnvironment("HOST_HEADER", "x-forwarded-host")
        .WithEnvironment("PORT_HEADER", "x-forwarded-port")
        .WithEnvironment("ADDRESS_HEADER", "True-Client-IP");
}

// ============================================================================
// Scalar API Reference
// ============================================================================

builder.AddScalarApiReference("scalar", o => o.WithTheme(ScalarTheme.Saturn))
    .WithReference(backend)
    .WithApiReference(backend, o =>
    {
        o.WithDefaultHttpClient(ScalarTarget.C, ScalarClient.HttpClient);
        o.AddPreferredSecuritySchemes("OAuth2");
        o.AddImplicitFlow("OAuth2", flow => flow.WithClientId("intra"));
    });

// ============================================================================

builder.Build().Run();