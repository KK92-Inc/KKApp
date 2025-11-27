// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Aspire’s AppHost is the code-first place where you declare your application’s
// services and their relationships.
//
// Instead of managing scattered configuration files,
// you describe the architecture in code.
//
// Aspire then handles local orchestration so you can focus on
// building features.
// @see: https://aspire.dev/get-started/app-host/
// ============================================================================

#:sdk Aspire.AppHost.Sdk@13.0.0
#:package Aspire.Npgsql@*
#:package Aspire.Hosting.PostgreSQL@*
#:package CommunityToolkit.Aspire.Hosting.Bun@*
#:project Backend/Backend.API.Root/Backend.API.Root.csproj

// ============================================================================

var dbname = "data";
var builder = DistributedApplication.CreateBuilder(args);
var username = builder.AddParameter("db-username", secret: true);
var password = builder.AddParameter("db-password", secret: true);
var postgres = builder.AddPostgres("database", username, password)
    .WithHostPort(52842)
    .WithEnvironment("POSTGRES_DB", dbname)
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume()
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase(dbname);
builder.AddProject<Projects.Backend_API_Root>("api")
    // .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();

// var postgres = builder.AddPostgres("postgres", username, password)
//     .WithHostPort(52842)
//     .WithDataVolume();

// // ============================================================================

// var postgresdb = postgres.AddDatabase("postgresdb");


// // builder.AddNpgsqlDataSource(connectionName: "postgresdb");
// // TODO: Cookies are fucked...
// builder.AddBunApp("frontend", "./Frontend", "dev")
//     .WithBunPackageInstallation()
//     .WithHttpEndpoint(env: "PORT", port: 5173)
//     .WithExternalHttpEndpoints()
//     .WithEnvironment("ORIGIN", "http://localhost:5173")
//     .WithEnvironment("KC_CALLBACK", "http://localhost:5173/auth/callback");
