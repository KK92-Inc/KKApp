<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import * as Project from '$lib/remotes/projects.remote';
	import {
		Plus,
		X,
		Zap,
		Unlock,
		Lock,
		Search,
		Trash,
		CircleAlert,
		GitBranch,
		Archive,
		Trophy
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	// import Thumbnail from '$lib/components/thumbnail.svelte';

	import * as Components from './index.svelte';
	import type { PageProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	let { params }: PageProps = $props();
	const context = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.projectId
		)
	);
</script>

{#await context.hydrate()}
	<Layout classR="pt-4 grid gap-2" classL="pt-4 flex flex-col gap-2">
		{#snippet left()}
			<Skeleton class="h-40" />
			<Skeleton class="h-20" />
			<Skeleton class="h-20" />
			<Skeleton class="h-40" />
			<Skeleton class="h-10" />
		{/snippet}

		{#snippet right()}
			<div class="flex h-10 gap-3">
				<Skeleton class="w-50" />
				<Skeleton class="w-80" />
			</div>
			<Skeleton class="h-100 w-full" />
		{/snippet}
	</Layout>
{:then _blank}
	<Layout classR="pt-4 grid gap-2" classL="pt-4 flex flex-col gap-2">
		{#snippet left()}
			<Components.Info />
			<Components.Members />
			{#if context.userProject}
				<Components.Reviews />
			{/if}
			<Components.Actions />
		{/snippet}

		{#snippet right()}
			<Components.Menu />
			{#if !context.initialized}
				<Components.Init />
			{:else}
				File Explorer TODO
			{/if}
		{/snippet}
	</Layout>
{/await}
