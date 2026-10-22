<script lang="ts">
	import type { LayoutProps } from './$types';
	import * as Tabs from '$lib/components/tabs';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
	import { goto } from '$app/navigation';

	import { Clock, Earth, FolderCode, Globe, UserCheck, UserPlus, UserSearch } from '@lucide/svelte';
	import { page } from '$app/state';

	const { children }: LayoutProps = $props();
	let tab = $derived(page.url.pathname);

	const TABS = [
		{
			label: 'Searching',
			icon: UserSearch,
			href: '/reviews'
		},
		{
			label: 'Other Evaluations',
			icon: Earth,
			href: '/reviews/global'
		}
	] as const;

	const PERSONAL = [
		{
			label: 'Recent Evaluations',
			icon: Clock,
			href: '/reviews/recent'
		},
		{
			label: 'My projects',
			icon: FolderCode,
			href: '/reviews/projects'
		}
	] as const;
</script>

<div class="mx-auto px-4 py-6 md:container">
	<h1 class="text-2xl font-bold tracking-tight">Peer Reviews & Evaluations</h1>
	<p class="text-sm font-normal text-muted-foreground">
		Claim pending evaluations, track session rounds, and view finished reviews.
	</p>

	<Separator class="my-2" />

	<Tabs.Root bind:value={tab} class="mb-2 gap-1 flex">
		<Tabs.List variant="line" class="p-0 pb-1">
			{#each TABS as tab (tab.href)}
				<Tabs.Trigger value={tab.href}>
					{#snippet child({ props })}
						<a {...props} href={tab.href}>
							<tab.icon />
							{tab.label}
						</a>
					{/snippet}
				</Tabs.Trigger>
			{/each}
			<Separator orientation="vertical" class="h-6!" />
			{#each PERSONAL as tab (tab.href)}
				<Tabs.Trigger value={tab.href}>
					{#snippet child({ props })}
						<a {...props} href={tab.href}>
							<tab.icon />
							{tab.label}
						</a>
					{/snippet}
				</Tabs.Trigger>
			{/each}
		</Tabs.List>
	</Tabs.Root>

	{@render children()}
</div>

<!-- <Tabs.Trigger onclick={() => goto('/reviews/global')} value="global">
				<Globe />
				All Evaluations
			</Tabs.Trigger>
			<Tabs.Trigger onclick={() => goto('/reviews/evaluate')} value="evaluate">
				<UserCheck />
				Active Evaluations
			</Tabs.Trigger>
			<Separator orientation="vertical" class="h-6!"/>
			<Tabs.Trigger onclick={() => goto('/reviews/recent')} value="recent">
				<Clock />
				My Recent Evaluations
			</Tabs.Trigger>
			<Tabs.Trigger onclick={() => goto('/reviews/projects')} value="projects">
				<FolderCode />
				My Projects
			</Tabs.Trigger> -->
