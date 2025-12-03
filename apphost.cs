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
#:package Scalar.Aspire@0.7.4
#:package Aspire.Hosting.PostgreSQL@*
#:package CommunityToolkit.Aspire.Hosting.Bun@*
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0
#:project App.Migrations/Migrations.csproj
#:project App.Backend/API/App.Backend.API.csproj

using Scalar.Aspire;

// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);

// Database
// ============
var dbPassword = builder.AddParameter("db-password", secret: true);
var dbUsername = builder.AddParameter("db-username", secret: true);
var postgres = builder.AddPostgres("postgres-server", dbUsername, dbPassword)
    .WithHostPort(52842)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("peeru-db");

// Keycloak
// ============
var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./config/dev-realm.json");

var realm = keycloak.AddRealm("student");

// Migration
// ============
var migrationService = builder.AddProject<Projects.Migrations>("migration-job")
    .WithReference(database)
    .WaitFor(postgres);

// Backend
// ============
var backendApi = builder.AddProject<Projects.App_Backend_API>("backend-api")
    .WithReference(database)
    .WaitFor(migrationService);
    // .WithReference(keycloak)
    // .WaitFor(keycloak)
    // .WithReference(realm);

var scalar = builder.AddScalarApiReference(options =>
{
    options
    .WithTheme(ScalarTheme.DeepSpace)
    .AddPreferredSecuritySchemes("OAuth2", "ApiKey")
    .AddAuthorizationCodeFlow("OAuth2", flow =>
    {
        flow
            .WithClientId("intra")
            .WithAuthorizationUrl("https://auth.example.com/oauth2/authorize")
            .WithTokenUrl("https://auth.example.com/oauth2/token");
    });
});

scalar.WithApiReference(backendApi);

// Frontend
// ============

builder.AddBunApp("frontend-app", "./App.Frontend", "dev")
    .WithReference(backendApi)
    .WithHttpEndpoint(env: "PORT", port: 51842)
    .WithExternalHttpEndpoints();

builder.Build().Run();
