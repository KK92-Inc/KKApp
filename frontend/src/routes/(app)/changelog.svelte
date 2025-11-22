<script lang="ts">
	import Badge from '$lib/components/badge/badge.svelte';
	import { Button } from '$lib/components/button';
	import * as Card from '$lib/components/card';
	import { Skeleton } from '$lib/components/skeleton';
	import { getChangelog } from '$lib/remotes/changelog.remote';
	import { RotateCcw } from '@lucide/svelte';
	import type { HttpError } from '@sveltejs/kit';
</script>

<svelte:boundary>
	{#snippet pending()}
		<div class="flex flex-col gap-4">
			<Skeleton class="h-24 w-full" />
			<Skeleton class="h-24 w-full" />
			<Skeleton class="h-24 w-full" />
		</div>
	{/snippet}

	{#snippet failed(error, reset)}
		{@const e = error as HttpError}
		<div class="flex flex-col items-center justify-center gap-2 py-4 text-center">
			<p class="text-sm text-destructive">{e.body.message}</p>
			<Button variant="outline" size="sm" onclick={reset}>
				<RotateCcw class="mr-2 h-4 w-4" />
				Try Again
			</Button>
		</div>
	{/snippet}

	<Card.Root class="h-min w-80 pb-2">
		{#await getChangelog() then changelog}
			<Card.Header class="py-0 border-b pb-2!">
				<Card.Title class="flex justify-between">
					Changelog
					<Badge>Version: {changelog.at(0)?.version ?? 'N/A'}</Badge>
				</Card.Title>
			</Card.Header>
			<Card.Content class="flex flex-col gap-4 px-2">
				{#each changelog.slice(0, 3) as item (item.version)}
					<a
						href="/changelog#{item.version}"
						class="group flex flex-col rounded-lg border p-3 transition-colors hover:bg-muted/50"
					>
						<div class="flex items-center justify-between pb-2">
							<Badge variant="secondary" class="font-mono text-xs">v{item.version}</Badge>
							<span class="text-xs text-muted-foreground">
								{new Date(item.on).toLocaleDateString()}
							</span>
						</div>
						<div class="space-y-1">
							<h4 class="leading-none font-medium tracking-tight">{item.title}</h4>
							<p class="line-clamp-2 text-sm text-muted-foreground">
								{item.description}
							</p>
						</div>
					</a>
				{/each}
			</Card.Content>
		{/await}
	</Card.Root>
</svelte:boundary>
