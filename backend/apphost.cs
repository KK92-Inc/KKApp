// ============================================================================
// Copyright (c) 2025 - W2Inc.
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
#:project Backend.API/Backend.API.Root/Backend.API.Root.csproj

// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);
var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);
var postgres = builder.AddPostgres("postgres", username, password).WithDataVolume();
var postgresdb = postgres.AddDatabase("postgresdb");

// ============================================================================

builder.AddProject<Projects.Backend_API_Root>("api")
    .WithReference(postgresdb);

// builder.AddNpgsqlDataSource(connectionName: "postgresdb");
// TODO: Cookies are fucked...
builder.AddBunApp("frontend", "../frontend", "dev")
    .WithBunPackageInstallation()
    .WithHttpEndpoint(env: "PORT", port: 5173)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ORIGIN", "http://localhost:5173")
    .WithEnvironment("KC_CALLBACK", "http://localhost:5173/auth/callback");

builder.Build().Run();
