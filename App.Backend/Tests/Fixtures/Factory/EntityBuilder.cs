// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Bogus;
using App.Backend.Database;

// ============================================================================

namespace App.Backend.Tests.Fixtures.Factory;

/// <summary>
/// A wrapper around Bogus Faker that provides fluent database persistence.
/// </summary>
/// <typeparam name="T">The entity type being generated.</typeparam>
public class EntityBuilder<T>(Faker<T> faker) where T : class
{
    private readonly Faker<T> _faker = faker;
    private DatabaseContext? _context;

    /// <summary>
    /// Sets the database context for automatic persistence.
    /// </summary>
    public EntityBuilder<T> WithContext(DatabaseContext context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// Applies additional rules to the faker.
    /// </summary>
    public EntityBuilder<T> With<TProperty>(
        System.Linq.Expressions.Expression<Func<T, TProperty>> property,
        TProperty value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    /// <summary>
    /// Applies additional rules to the faker using a generator function.
    /// </summary>
    public EntityBuilder<T> With<TProperty>(
        System.Linq.Expressions.Expression<Func<T, TProperty>> property,
        Func<Faker, TProperty> generator)
    {
        _faker.RuleFor(property, generator);
        return this;
    }

    /// <summary>
    /// Generates a single entity. If a context was provided, the entity is saved to the database.
    /// </summary>
    public T Generate()
    {
        var entity = _faker.Generate();

        if (_context is not null)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        return entity;
    }

    /// <summary>
    /// Generates a single entity asynchronously. If a context was provided, the entity is saved to the database.
    /// </summary>
    public async Task<T> GenerateAsync(CancellationToken token = default)
    {
        var entity = _faker.Generate();

        if (_context is not null)
        {
            await _context.AddAsync(entity, token);
            await _context.SaveChangesAsync(token);
        }

        return entity;
    }

    /// <summary>
    /// Generates multiple entities. If a context was provided, entities are saved to the database.
    /// </summary>
    public List<T> Generate(int count)
    {
        var entities = _faker.Generate(count);

        if (_context is not null)
        {
            _context.AddRange(entities);
            _context.SaveChanges();
        }

        return entities;
    }

    /// <summary>
    /// Generates multiple entities asynchronously. If a context was provided, entities are saved to the database.
    /// </summary>
    public async Task<List<T>> GenerateAsync(int count, CancellationToken token = default)
    {
        var entities = _faker.Generate(count);

        if (_context is not null)
        {
            await _context.AddRangeAsync(entities, token);
            await _context.SaveChangesAsync(token);
        }

        return entities;
    }

    /// <summary>
    /// Gets the underlying Faker for advanced customization.
    /// </summary>
    public Faker<T> AsFaker() => _faker;
}

/// <summary>
/// Extension methods to convert Faker to EntityBuilder.
/// </summary>
public static class FakerExtensions
{
    /// <summary>
    /// Converts a Faker to an EntityBuilder for fluent database persistence.
    /// </summary>
    public static EntityBuilder<T> ToBuilder<T>(this Faker<T> faker) where T : class
        => new(faker);

    /// <summary>
    /// Shorthand to create an EntityBuilder with context directly from a Faker.
    /// </summary>
    public static EntityBuilder<T> WithContext<T>(this Faker<T> faker, DatabaseContext context) where T : class
        => new EntityBuilder<T>(faker).WithContext(context);
}
