<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import { Separator } from '$lib/components/separator';
	import { Badge } from '$lib/components/badge';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';
	import { MessageCircleHeart } from '@lucide/svelte';
	import Members from './page-members.svelte';
	import Reviews from './page-reviews.svelte';
	import type { components } from '$lib/api/api';

	interface Props {
		project: components['schemas']['ProjectDO'];
		userProject?: components['schemas']['UserProjectDO'];
		userId: string;
	}

	const { project, userProject, userId }: Props = $props();

	const isSubscribed = $derived(userProject && userProject.state !== 'Inactive');
	const stateVariant = $derived.by(() => {
		if (!userProject) return undefined;
		switch (userProject.state) {
			case 'Active': return 'default' as const;
			case 'Completed': return 'secondary' as const;
			case 'Inactive': return 'outline' as const;
			default: return 'outline' as const;
		}
	});
</script>

<Card.Root class="mt-4 flex h-fit flex-col shadow-none">
	<Card.Header class="items-center pb-2">
		<Thumbnail readonly src="/placeholder.svg" class="mx-auto" />
	</Card.Header>

	<Card.Content class="space-y-4">
		<!-- Project name & state -->
		<div class="space-y-1">
			<h1 class="text-xl font-bold leading-tight">{project.name}</h1>
			{#if userProject}
				<Badge variant={stateVariant} class="text-[10px]">
					{userProject.state}
				</Badge>
			{/if}
		</div>

		<!-- Members -->
		{#if userProject && isSubscribed}
			<div class="space-y-2">
				<h3 class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
					Members
				</h3>
				<Members userProjectId={userProject.id} />
			</div>

			<Separator />
		{/if}

		<!-- Subscribe / Unsubscribe -->
		{#if !isSubscribed}
			<form {...subscribeProject}>
				<input hidden {...subscribeProject.fields.userId.as('text')} value={userId} />
				<input hidden {...subscribeProject.fields.projectId.as('text')} value={project.id} />
				<Button loading={subscribeProject.pending > 0} type="submit" class="w-full">
					Subscribe
				</Button>
			</form>
		{:else}
			<form {...unsubscribeProject}>
				<input hidden {...unsubscribeProject.fields.userId.as('text')} value={userId} />
				<input hidden {...unsubscribeProject.fields.projectId.as('text')} value={project.id} />
				<Button
					loading={unsubscribeProject.pending > 0}
					type="submit"
					variant="outline"
					class="w-full"
				>
					Unsubscribe
				</Button>
			</form>
		{/if}

		<!-- Reviews -->
		{#if userProject && isSubscribed}
			<Separator />
			<div class="space-y-2">
				<h3 class="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
					<MessageCircleHeart size={14} />
					Reviews
				</h3>
				<Reviews userProjectId={userProject.id} />
			</div>
		{/if}
	</Card.Content>
</Card.Root>
