// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Options;
using App.Backend.Domain.Enums;
using App.Backend.Tests.Fixtures;
using App.Backend.Tests.Fixtures.Factory;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Tests.Services;

public class SubscriptionTest : ServiceTestBase
{
    private SubscriptionService CreateService(ProgressionMode mode = ProgressionMode.Restricted)
    {
        var options = Options.Create(new SubscriptionOptions { Mode = mode });
        return new SubscriptionService(Context, options);
    }

    [Fact]
    public async Task Subscription_SubscribingToProject_FreeMode()
    {
        var service = CreateService(ProgressionMode.Free);

        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await service.SubscribeToProjectAsync(user.Id, project.Id);
    }

    [Fact]
    public async Task Subscription_SubscribingToProject_RestrictedMode()
    {
        var service = CreateService(ProgressionMode.Restricted);

        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await Assert.ThrowsAsync<ServiceException>(async () => await service.SubscribeToProjectAsync(user.Id, project.Id));
    }

    [Fact]
    public async Task Subscription_UnsubscribeFromProject_FreeMode()
    {
        var service = CreateService(ProgressionMode.Free);

        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await service.SubscribeToProjectAsync(user.Id, project.Id);
        await service.UnsubscribeFromProjectAsync(user.Id, project.Id);
    }

    [Fact]
    public async Task Subscription_UnsubscribeFromProject_RestrictedMode()
    {
        var service = CreateService(ProgressionMode.Restricted);

        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await Assert.ThrowsAsync<ServiceException>(async () => await service.UnsubscribeFromProjectAsync(user.Id, project.Id));
    }

    [Fact]
    public async Task Subscription_Cooldown()
    {
        var service = CreateService(ProgressionMode.Free);

        var user = await UserFactory.Create().WithContext(Context).GenerateAsync();
        var project = await ProjectFactory.Create().WithContext(Context).GenerateAsync();

        await service.SubscribeToProjectAsync(user.Id, project.Id);
        await service.UnsubscribeFromProjectAsync(user.Id, project.Id);

        // Attempt to subscribe again immediately should fail due to cooldown
        await Assert.ThrowsAsync<ServiceException>(async () => await service.SubscribeToProjectAsync(user.Id, project.Id));
    }
}