// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using App.Backend.Core;
using App.Backend.Core.Services.Implementation;
using App.Backend.Core.Services.Interface;
using App.Backend.Database;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Tests.Services;

public class SubscriptionServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly Mock<IUserProjectService> _userProjectServiceMock;
    private readonly SubscriptionService _sut; // System Under Test

    public SubscriptionServiceTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DatabaseContext(options);
        _context.Database.EnsureCreated();

        _userProjectServiceMock = new Mock<IUserProjectService>();
        _sut = new SubscriptionService(_context, _userProjectServiceMock.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    #region SubscribeToCursus Tests

    [Fact]
    public async Task SubscribeToCursusAsync_WhenNotSubscribed_ShouldCreateNewSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursus = new Cursus
        {
            Id = Guid.NewGuid(),
            Name = "Test Cursus",
            Description = "Test Description",
            Slug = "test-cursus",
            Track = JsonDocument.Parse("{}")
        };
        await _context.Cursi.AddAsync(cursus);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.SubscribeToCursusAsync(userId, cursus.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(cursus.Id, result.CursusId);
        Assert.Equal(EntityObjectState.Active, result.State);
    }

    #endregion
}
