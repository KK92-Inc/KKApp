<script lang="ts">
	import type { LayoutProps } from './$types';
	import * as Tabs from '$lib/components/tabs';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
	import { goto } from '$app/navigation';

	import { Clock, FolderCode, UserCheck, UserPlus } from '@lucide/svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { page } from '$app/state';

	const { children }: LayoutProps = $props();
	let tab = $derived(page.url.pathname.slice(page.url.pathname.lastIndexOf('/') + 1));
</script>

<div class="mx-auto px-4 py-6 md:container">
	<h1 class="text-2xl font-bold tracking-tight">Peer Reviews & Evaluations</h1>
	<p class="text-sm font-normal text-muted-foreground">
		Claim pending evaluations, track session rounds, and view finished reviews.
	</p>

	<Separator class="my-2" />

	<Tabs.Root bind:value={tab} class="gap-1 mb-2">
		<Tabs.List variant="line" class="p-1 [*]:font-normal gap-0">
			<Tabs.Trigger onclick={() => goto('/reviews')} value="reviews">
				<UserPlus />
				Seeking Evaluations
			</Tabs.Trigger>
			<Tabs.Trigger onclick={() => goto('/reviews/evaluate')} value="evaluate">
				<UserCheck />
				Active Evaluations
			</Tabs.Trigger>
			<Tabs.Trigger onclick={() => goto('/reviews/recent')} value="recent">
				<Clock />
				Recent Evaluations
			</Tabs.Trigger>
			<Tabs.Trigger onclick={() => goto('/reviews/projects')} value="projects">
				<FolderCode />
				My Projects
			</Tabs.Trigger>
		</Tabs.List>

		<Tabs.Content value="reviews">
			<Alert.Root class="mt-2">
				<Alert.Title>Looking for evaluations</Alert.Title>
				<Alert.Description>
					Browse and claim available evaluation sessions that match your skills and interests.
				</Alert.Description>
			</Alert.Root>
		</Tabs.Content>

		<Tabs.Content value="evaluate">
			<Alert.Root class="mt-2">
				<Alert.Title>Conducting evaluations</Alert.Title>
				<Alert.Description>
					Review the evaluations you have claimed and complete the active session rounds.
				</Alert.Description>
			</Alert.Root>
		</Tabs.Content>

		<Tabs.Content value="recent">
			<Alert.Root class="mt-2">
				<Alert.Title>Recent evaluations</Alert.Title>
				<Alert.Description>
					View evaluations you have recently completed, including their status and feedback.
				</Alert.Description>
			</Alert.Root>
		</Tabs.Content>

		<Tabs.Content value="projects">
			<Alert.Root class="mt-2">
				<Alert.Title>My projects</Alert.Title>
				<Alert.Description>
					Manage your projects and review the evaluations associated with each one.
				</Alert.Description>
			</Alert.Root>
		</Tabs.Content>
	</Tabs.Root>

	{@render children()}
</div>
