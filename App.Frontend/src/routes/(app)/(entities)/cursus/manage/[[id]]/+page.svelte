<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import { Separator } from '$lib/components/separator';
	import { Skeleton } from '$lib/components/skeleton';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Button } from '$lib/components/button';
	import { Textarea } from '$lib/components/textarea';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import * as Tabs from '$lib/components/tabs';
	import * as Alert from '$lib/components/alert';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Item from '$lib/components/item';
	import * as Page from './context.svelte';
	import Access from '../../../shared/access.svelte';
	import type { PageProps } from './$types';
	import { Input } from '$lib/components/input';
	import { page } from '$app/state';
	import {
		Blend,
		CircleDot,
		CirclePlay,
		Database,
		Download,
		FileText,
		FlaskConical,
		GitBranch,
		Heart,
		HeartCrack,
		Info,
		Link,
		ListTree,
		LocateFixed,
		Network,
		TrendingUpDown,
		Unlink,
		Upload,
		VectorSquare
	} from '@lucide/svelte';
	import CurusRender from './curus-render.svelte';
	import CursusSchematic from './cursus-schematic.svelte';
	import { useDialog } from '$lib/components/dialog';

	const dialog = useDialog();
	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	let fileInput = $state<HTMLInputElement | null>(null);

	$effect(() => {
		context.hydrate();
	});

	function exportTrack() {
		const json = JSON.stringify(context.track);
		const blob = new Blob([json], { type: 'application/json' });
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		const fileName = context.fields.name
			? `${context.fields.name.toLowerCase().replace(/\s+/g, '-')}-track.json`
			: 'cursus-track.json';

		a.href = url;
		a.download = fileName;
		a.click();
		URL.revokeObjectURL(url);
	}

	function importTrack(event: Event) {
		const target = event.target as HTMLInputElement;
		const file = target.files?.[0];
		if (!file) return;

		const reader = new FileReader();
		reader.onload = async (e) => {
			try {
				const data = JSON.parse(e.target?.result as string);
				if (Array.isArray(data)) {
					context.track = data;
				} else {
					await dialog.alert('Invalid file format', 'The file might be invalid / corrupted.');
				}
			} catch (_) {
				await dialog.alert('Failed to parse file', 'The file might be invalid / corrupted.');
			} finally {
				target.value = '';
			}
		};
		reader.readAsText(file);
	}
</script>

<svelte:boundary>
	{#snippet pending()}
		<Layout class="px-4" classL="space-y-4" classR="px-0!">
			{#snippet left()}
				<Skeleton class="mt-4 h-100" />
				<Skeleton class="h-50" />
				<Skeleton class="h-10" />
			{/snippet}

			{#snippet right()}
				<Skeleton class="mt-4 h-25" />
				<Separator class="my-2" />
				<Skeleton class="h-196" />
			{/snippet}
		</Layout>
	{/snippet}

	<input type="file" accept=".json" bind:this={fileInput} onchange={importTrack} class="hidden" />

	<Layout class="px-4" classL="space-y-4" classR="px-0!">
		{#snippet left()}
			<Card.Root class="mt-4 gap-1 overflow-hidden p-0">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail
						size={192}
						bind:value={context.fields.thumbnail}
						name="thumbnail"
						class="rounded-lg border"
						alt="Event thumbnail"
					/>
				</div>

				<Card.Content class="p-4">
					<Field.Set class="gap-1.5">
						<!-- Name -->
						<Field.Field>
							<Field.Label for="name">Name</Field.Label>
							<Input
								id="name"
								maxlength={255}
								bind:value={context.fields.name}
								disabled={context.fields.deprecated}
								placeholder="Entry into..."
							/>
							<Field.Description>The name of the cursus.</Field.Description>
							<Field.Error errors={context.errors.name} />
						</Field.Field>

						<Field.Field>
							<Field.Label for="description">
								Description
								<span class="ml-auto text-xs font-normal">
									{context.fields.description.length}/255
								</span>
							</Field.Label>
							<Textarea
								id="description"
								rows={3}
								disabled={context.fields.deprecated}
								class="max-h-52 resize-y"
								placeholder="This cursus will teach you about..."
								maxlength={255}
								bind:value={context.fields.description}
							/>
							<Field.Description>Short and readable description about the cursus.</Field.Description>
							<Field.Error errors={context.errors.description} />
						</Field.Field>

						<!-- Workspace -->
						<!-- NOTE(W2): For now only really staff can actually put this somewhere else... -->
						{#if !params.id && page.data.session.roles.includes('staff')}
							<Field.Field>
								<Field.Label for="workspace">Workspace</Field.Label>
								<Tabs.Root id="workspace" bind:value={context.workspace}>
									<Tabs.List class="w-auto">
										<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
										<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
									</Tabs.List>
								</Tabs.Root>
								<Field.Description>Which workspace this cursus belongs to.</Field.Description>
								<Field.Error errors={context.errors.workspace} />
							</Field.Field>
						{/if}

						<Separator class="my-2" />

						<Field.Field>
							<Field.Label for="mode">Progression Mode</Field.Label>
							<Tabs.Root id="mode" bind:value={context.fields.mode}>
								<Tabs.List class="w-auto">
									<Tabs.Trigger value="FreeStyle">Freestyle <Network /></Tabs.Trigger>
									<Tabs.Trigger value="Ring">Ring <CircleDot /></Tabs.Trigger>
								</Tabs.List>
							</Tabs.Root>
							<Field.Description>
								{#if context.fields.mode === 'Ring'}
									In Ring mode progression goes in depth. Meaning that in order to progress to the next
									goals you need to complete all goals in the same depth.
								{:else}
									In Freestyle mode progression is hierarchical. Meaning you need to complete the previous
									goal to progress to the next goal.
								{/if}
							</Field.Description>
							<Field.Error errors={context.errors.mode} />
						</Field.Field>

						{#if !params.id}
							<Separator class="my-2" />
							<Field.Field>
								<Field.Label for="variant">Cursus Variant</Field.Label>
								<Tabs.Root id="variant" bind:value={context.fields.variant}>
									<Tabs.List class="w-auto">
										<Tabs.Trigger disabled value="Dynamic">Dynamic <TrendingUpDown /></Tabs.Trigger>
										<Tabs.Trigger disabled value="Hybrid">Hybrid <Blend /></Tabs.Trigger>
										<Tabs.Trigger value="Static">Static <LocateFixed /></Tabs.Trigger>
									</Tabs.List>
								</Tabs.Root>
								<Field.Description>
									{#if context.fields.variant === 'Static'}
										A static cursus defines the goals in it's entirety with no choice of selection for the
										user.
									{:else if context.fields.variant === 'Dynamic'}
										A Dynamic cursus defines gives the user full agency over defining the goals they want to
										complete.
									{:else if context.fields.variant === 'Hybrid'}
										A hybdrid cursus defines a set of given goals and leaves the rest up for the user to
										decide.
									{:else}
										Unknown
									{/if}
								</Field.Description>
								<Field.Error errors={[]} />
							</Field.Field>
						{/if}
					</Field.Set>
				</Card.Content>
			</Card.Root>

			<Access
				bind:visible={context.fields.public}
				bind:enabled={context.fields.enabled}
				disabled={context.fields.deprecated}
			/>

			<div class="flex items-center justify-around gap-4">
				<Separator class="flex-1" />
				<ButtonGroup.Root>
					{#if params.id && context.fields.deprecated}
						<Button variant="outline" onclick={() => context.undeprecate()}>
							Undeprecate <Heart />
						</Button>
					{:else if params.id}
						<Button variant="outline" onclick={() => context.deprecate()}>
							Deprecate <HeartCrack />
						</Button>
					{/if}

					<Button onclick={() => context.submit()} disabled={context.fields.deprecated}>
						{params.id ? 'Save Changes' : 'Create Cursus'}
						<CirclePlay />
					</Button>
				</ButtonGroup.Root>
			</div>
		{/snippet}

		{#snippet right()}
			<div class="mt-4 space-y-2">
				{#if !params.id}
					<Item.Group class="mt-4 flex flex-row gap-3">
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><ListTree class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Define the Schematic</Item.Title>
								<Item.Description class="line-clamp-none text-xs">
									The schematic outlines the progression path of the cursus. You can drag and drop to
									re-arrange the order.
								</Item.Description>
							</Item.Content>
						</Item.Root>
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><Link class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Choose a Mode & Variant</Item.Title>
								<Item.Description class="line-clamp-none text-xs">
									Determine which mode and variant you want the cursus to be. Be aware that the Variant cannot
									be changed at a later time.
								</Item.Description>
							</Item.Content>
						</Item.Root>
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><VectorSquare class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Double check the graph</Item.Title>
								<Item.Description class="line-clamp-none text-xs">
									Once everything is ready you can check out the graph to see what the cursus will look like.
								</Item.Description>
							</Item.Content>
						</Item.Root>
					</Item.Group>
				{/if}

				<Tabs.Root value="schema">
					<div class="flex w-full items-center gap-4">
						<Tabs.List>
							<Tabs.Trigger value="schema">
								View Schematic
								<ListTree />
							</Tabs.Trigger>
							<Tabs.Trigger value="render">
								View Render
								<VectorSquare />
							</Tabs.Trigger>
						</Tabs.List>
						<Separator class="flex-1" />
						<ButtonGroup.Root>
							<Button variant="outline" onclick={() => fileInput?.click()}>
								Import <Upload />
							</Button>
							<Button variant="outline" onclick={exportTrack}>
								Export <Download />
							</Button>
						</ButtonGroup.Root>
					</div>
					<Tabs.Content value="schema">
						<CursusSchematic />
					</Tabs.Content>
					<Tabs.Content value="render">
						<CurusRender />
					</Tabs.Content>
				</Tabs.Root>
			</div>
		{/snippet}
	</Layout>
</svelte:boundary>
