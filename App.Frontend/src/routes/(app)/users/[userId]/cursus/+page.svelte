<script lang="ts">
	import Paginate from '$lib/components/paginate.svelte';
	import { getCursi } from '$lib/remotes/cursus.remote';
	import { getContext } from './context.svelte';

	const { url } = getContext();
	const page = url.query('page');
	const search = url.query('search');
	const promise = $derived(
		getCursi({
			name: search.value,
			page: page.value
		})
	);
</script>

<div class="container mx-auto p-6">
	<h1 class="text-2xl font-bold text-foreground mb-6">Browse Cursi</h1>

	<svelte:boundary>
		{@const data = await promise}
		{#snippet pending()}
			<div class="flex items-center justify-center py-12">
				<div class="text-muted-foreground">Loading cursi...</div>
			</div>
		{/snippet}

<div class="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
	{#each data.data as cursus (cursus.id)}
		{@const owner = cursus.workspace?.owner}
		<a
			href={owner ? `/users/${owner.id}/cursus/${cursus.slug}` : undefined}
			class="group relative flex flex-col justify-between overflow-hidden rounded-xl border border-border bg-card shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-lg hover:shadow-primary/5"
		>
			<div
				class="pointer-events-none absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-primary to-primary/50 opacity-0 transition-opacity group-hover:opacity-100"
			></div>

			<div class="flex h-full flex-col p-6">
				<div class="mb-4 flex items-start justify-between">
					<h2
						class="line-clamp-1 text-lg font-bold tracking-tight text-card-foreground transition-colors group-hover:text-primary"
					>
						{cursus.name}
					</h2>
				</div>

				{#if cursus.description}
					<p class="mb-6 line-clamp-3 text-sm text-muted-foreground">
						{cursus.description}
					</p>
				{:else}
					<p class="mb-6 text-sm italic text-muted-foreground opacity-50">
						No description provided.
					</p>
				{/if}

				<div class="mt-auto flex items-center justify-between border-t border-border/50 pt-4">
					<div class="flex items-center gap-2.5">
						{#if owner?.avatarUrl}
							<img
								src={owner.avatarUrl}
								alt={owner.displayName ?? 'User'}
								class="h-6 w-6 rounded-full object-cover ring-2 ring-background transition-transform group-hover:scale-105"
							/>
						{:else}
							<div
								class="flex h-6 w-6 items-center justify-center rounded-full bg-secondary text-[10px] font-bold text-secondary-foreground ring-2 ring-background"
							>
								{(owner?.displayName?.[0] ?? '?').toUpperCase()}
							</div>
						{/if}

						<div class="flex flex-col">
							<span class="text-xs font-medium text-foreground">
								{owner?.displayName ?? 'Unknown'}
							</span>
							<span class="text-[10px] capitalize text-muted-foreground">
								{cursus.workspace?.ownership?.toLowerCase() ?? 'owner'}
							</span>
						</div>
					</div>

					<div class="text-[10px] text-muted-foreground tabular-nums">
						{new Date(cursus.createdAt).toLocaleDateString(undefined, {
							month: 'short',
							day: 'numeric',
							year: 'numeric'
						})}
					</div>
				</div>
			</div>
		</a>
	{/each}
</div>

		{#if data.items.length === 0}
			<div class="flex flex-col items-center justify-center py-12 text-center">
				<p class="text-muted-foreground">No cursi found</p>
			</div>
		{/if}
	</svelte:boundary>
</div>
