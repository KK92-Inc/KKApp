// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using App.Backend.Database;
using App.Backend.Database.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using App.Backend.Core.Services.Interface;
using Serilog;

// ============================================================================

namespace App.Backend.Tests.Integration;

public class WebAppTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    public HttpClient CreateClient(Guid? userId = null, string[]? roles = null)
    {
        var client = base.CreateClient();
        if (userId is not null)
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        if (roles is not null)
            client.DefaultRequestHeaders.Add("X-Test-Roles", string.Join(",", roles));
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                // Ignore transaction warnings since InMemory doesn't support them
                options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                // Re-apply production interceptors and proxies to maintain fidelity
                options.UseLazyLoadingProxies();
                options.AddInterceptors(new SshKeyInterceptor());
                options.AddInterceptors(new SavingChangesInterceptor(TimeProvider.System));
                options.EnableSensitiveDataLogging();
            });

            // Bypass Keycloak auth/authorization entirely
            services.RemoveAll<IPolicyEvaluator>();
            services.AddSingleton<IPolicyEvaluator, TestPolicyEvaluator>();

            // Remove as it requires external service.
            // Instead run git commands locally in a tmp directory.
            services.RemoveAll<IGitService>();
            services.AddSingleton<IGitService, LocalGitService>();
        });
    }

    public DatabaseContext CreateDbContext() =>
        Services.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
}