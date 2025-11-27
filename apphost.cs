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
#:package Scalar.Aspire@0.7.4
#:project App.Migrations/Migrations.csproj
#:project App.Backend/API/App.Backend.API.csproj

// ============================================================================

using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// 1. Define specific names for resources
var dbPassword = builder.AddParameter("db-password", secret: true);
var dbUsername = builder.AddParameter("db-username", secret: true);

// "postgres-server" is the container name
var postgres = builder.AddPostgres("postgres-server", dbUsername, dbPassword)
    .WithHostPort(52842)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// "peeru-db" is the logical database name used in connection strings
var database = postgres.AddDatabase("peeru-db");

// 2. Migrations run as a task
var migrationService = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

// 3. Backend API
var backendApi = builder.AddProject<Projects.App_Backend_API>("backend-api")
    .WithReference(database)
    .WaitFor(migrationService);

var scalar = builder.AddScalarApiReference(options =>
{
    options
    .WithTheme(ScalarTheme.Kepler)
    .AddPreferredSecuritySchemes("OAuth2", "ApiKey")
    .AddAuthorizationCodeFlow("OAuth2", flow =>
    {
        flow
            .WithClientId("")
            .WithAuthorizationUrl("https://auth.example.com/oauth2/authorize")
            .WithTokenUrl("https://auth.example.com/oauth2/token");
    });
});

scalar.WithApiReference(backendApi);

// 4. Frontend (Uncommented and wired up)
// builder.AddBunApp("frontend-app", "./Frontend", "dev")
//     .WithReference(backendApi) // Allows frontend to resolve backend URL
//     .WithHttpEndpoint(env: "PORT", port: 5173)
//     .WithExternalHttpEndpoints();

builder.Build().Run();
