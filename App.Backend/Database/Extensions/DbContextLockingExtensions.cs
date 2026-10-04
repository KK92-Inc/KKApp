// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Database.Extensions;

public static partial class DbContextLockingExtensions
{
    extension(DbContext context)
    {
        /// <summary>
        /// Runs <paramref name="work"/> in a transaction that holds a named, database wide lock.
        ///
        /// Anything that does "check, then change" (a capacity, a uniqueness rule, a counter) can be
        /// passed by two requests at the same time, and both would pass the check. Running that code
        /// under the same lock makes those requests take turns instead. The second one waits until the
        /// first has committed or rolled back, and then sees its result.
        /// </summary>
        /// <param name="context">The context to run on.</param>
        /// <param name="key">
        /// What to lock. Prefix it with a scope, e.g. <c>kickoff:{id}</c>, so unrelated features can't end up
        /// sharing a lock by accident. Keys are hashed, a collision just means two keys briefly share a lock.
        /// </param>
        /// <param name="work">The work to run while holding the lock.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Whatever <paramref name="work"/> returns.</returns>
        public Task<T> WithAdvisoryLockAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> work,
            CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(work);

            // An outer transaction means an outer execution strategy owns the retries,
            // starting another one here would throw.
            if (context.Database.CurrentTransaction is not null)
            {
                return RunAsync(token);

                async Task<T> RunAsync(CancellationToken ct)
                {
                    await AcquireAsync(context, key, ct);
                    return await work(ct);
                }
            }

            var strategy = context.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async ct =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(ct);
                await AcquireAsync(context, key, ct);

                var result = await work(ct);
                await transaction.CommitAsync(ct);
                return result;
            }, token);
        }

        public async Task WithAdvisoryLockAsync(
            string key,
            Func<CancellationToken, Task> work,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(work);

            await context.WithAdvisoryLockAsync(key, async ct =>
            {
                await work(ct);
                return true;
            }, token);
        }

        private Task<int> AcquireAsync(string key, CancellationToken token)
        {
            return context.Database
                .ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", token);
        }
    }


}
