<script lang="ts">
	import { HierarchyEditor, type LazyHierarchyAdapter } from '$lib/components/hierarchy';
	import type { LayoutProps } from './$types';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Drawer from '$lib/components/drawer';
	import * as Tabs from '$lib/components/tabs';
	import * as Dialog from '$lib/components/dialog';
	import Layout from '$lib/components/layout.svelte';
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import {
		HeartHandshake,
		File,
		Folder,
		Ban,
		BadgeQuestionMark,
		Ruler,
		Archive,
		MessageSquareCode,
		Files,
		Play,
		X,
		CircleArrowLeft
	} from '@lucide/svelte';
	import { page } from '$app/state';
	import Separator from '$lib/components/separator/separator.svelte';
	import ViewRubric from './view-rubric.svelte';
	import ViewProject from './view-project.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import { startTour } from './context.svelte';
	import Conclusion from './conclusion.svelte';

	const { params, children }: LayoutProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));

	const data = $derived(await Remote.load(params.id));

	interface FileNode {
		path: string;
		isDirectory: boolean;
	}

	const adapter: LazyHierarchyAdapter<FileNode> = {
		type: 'lazy',
		getId: (node) => node.path,
		canHaveChildren: (node) => node.isDirectory,
		getChildren: async (node) => {
			const content = await Git.getTreePath({
				id: data.review.userProject.gitInfoId,
				ref: data.review.sha!,
				path: node.path
			});

			return content.map((f) => ({
				isDirectory: f.directory,
				path: f.path
			}));
		},
		isDraggable: () => false,
		canDrop: () => false
	};
</script>

{#snippet count(length: number)}
	{#if length > 0}
		<Badge class="absolute -top-1.5 -right-2 size-4 border p-1 text-xs">
			{length}
		</Badge>
	{/if}
{/snippet}

<!-- Does this concern you... if not is it finished yet ? -->
{#if data.review.state !== 'Finished' && data.review.reviewer?.id !== page.data.session.userId}
	<Dialog.Root open>
		<Dialog.Content escapeKeydownBehavior="ignore" interactOutsideBehavior="ignore" showCloseButton={false}>
			<Dialog.Header>
				<Dialog.Title>Evaluation is not yet completed</Dialog.Title>
				<Dialog.Description>
					Since this is not your review and it is not completed there is nothing to show yet.
					Come back once the evaluation is completed.
				</Dialog.Description>
			</Dialog.Header>
			<Dialog.Footer class="sm:justify-start">
				<Button href="/reviews/round/{data.review.roundId}">
					<CircleArrowLeft />
					Go Back
				</Button>
				{#if data.review.reviewer && page.data.session.roles.includes('staff')}
					<Button variant="destructive">
						Cancel Evaluation
						<X />
					</Button>
				{/if}
			</Dialog.Footer>
		</Dialog.Content>
	</Dialog.Root>
{:else}
	<Layout cover class="gap-0" classR="px-0!">
		{#snippet left()}
			<div
				id="file-explorer-sidebar"
				class="flex h-full flex-col border-r bg-sidebar text-sidebar-foreground"
			>
				<span
					class="border-b bg-muted/20 px-4 py-3 text-xs font-semibold tracking-wider text-muted-foreground uppercase"
				>
					Evaluation
				</span>

				<div class="flex items-center justify-between border-b bg-muted/40 px-4 py-2">
					<div class="flex min-w-0 items-center gap-2">
						<Archive class="size-4 shrink-0" />
						<span class="truncate text-xs font-medium text-foreground">
							{data.review.userProject.name}
						</span>
					</div>
					<Badge variant="outline" class="rounded font-mono">
						Submitted Files
						<Files />
					</Badge>
				</div>

				<svelte:boundary>
					{@const root = await Git.getTree({ id: data.review.userProject.gitInfoId, ref: data.review.sha! })}
					{@const items = root.map((f) => ({
						isDirectory: f.directory,
						path: f.path
					}))}

					<HierarchyEditor {items} {adapter} onMove={() => {}} class="space-y-1 p-2">
						{#snippet item({ item })}
							{@const name = item.path.slice(item.path.lastIndexOf('/') + 1)}
							{@const href = `/reviews/${data.review.id}/blob/${data.review.sha}/${item.path}`}
							<Button
								data-sveltekit-preload-data="off"
								href={item.isDirectory ? null : href}
								class="h-6! w-full justify-start bg-transparent! p-1! px-0 shadow-none"
								inert={item.isDirectory}
							>
								{#if item.isDirectory}
									<Folder class="shrink-0 text-muted-foreground group-hover:text-foreground" />
								{:else}
									{@const annotationCount = context.annotations.get(item.path)?.length ?? 0}
									<div class="relative shrink-0">
										<File size={16} class="text-muted-foreground group-hover:text-foreground" />
										{@render count(annotationCount)}
									</div>
								{/if}

								<span class="ml-1.5 truncate text-foreground">{name}</span>
							</Button>
						{/snippet}
					</HierarchyEditor>
				</svelte:boundary>
			</div>
		{/snippet}

		{#snippet right()}
			<div class="flex items-center gap-2 border-b bg-muted/40 px-4 py-3">
				<ButtonGroup.Root class="flex-wrap items-center">
					<ButtonGroup.Root>
						<Button variant="outline" href="/reviews/{data.review.id}">
							Start Page
							<HeartHandshake />
						</Button>
						<Button id="help-button" variant="outline" onclick={startTour}>
							Help
							<BadgeQuestionMark />
						</Button>
					</ButtonGroup.Root>

					<Separator orientation="vertical" class="h-4!" />

					<ButtonGroup.Root>
						<Drawer.Root modal={false} nested direction="right">
							<Drawer.Trigger
								id="view-rubric-btn"
								class={buttonVariants({
									variant: 'outline'
								})}
							>
								View Rubric
								<Ruler size={16} />
							</Drawer.Trigger>
							<Drawer.Content class="data-[vaul-drawer-direction=right]:sm:max-w-xl">
								<Drawer.Header class="border-b">
									<Drawer.Title class="flex items-center gap-2">
										<Ruler />
										Rubric
									</Drawer.Title>
								</Drawer.Header>
								<div class="flex-1 overflow-auto p-4">
									<Tabs.Root value="account">
										<Tabs.List class="w-full">
											<Tabs.Trigger value="account">Subject <Archive /></Tabs.Trigger>
											<Tabs.Trigger value="password">Rubric <MessageSquareCode /></Tabs.Trigger>
										</Tabs.List>
										<Tabs.Content value="account">
											<ViewProject />
										</Tabs.Content>
										<Tabs.Content value="password">
											<ViewRubric />
										</Tabs.Content>
									</Tabs.Root>
								</div>
								<Drawer.Footer>
									<Drawer.Close class={buttonVariants()}>Close</Drawer.Close>
								</Drawer.Footer>
							</Drawer.Content>
						</Drawer.Root>
					</ButtonGroup.Root>
				</ButtonGroup.Root>

				<Separator class="flex-1" />
				<ButtonGroup.Root>
					<Button
						id="action-cancel"
						variant="destructive"
						class="border"
						disabled={data.review.state === 'Finished' || data.review.state === 'Cancelled'}
						onclick={async () => await context.cancel(data.review)}
					>
						Cancel <Ban />
					</Button>
					{#if data.review.state === 'Pending' && data.review.reviewer?.id === page.data.session.userId}
						<Button
							id="action-start"
							variant="outline"
							class="rounded-l-none border"
							onclick={async () => await context.start(data.review)}
						>
							Start <Play />
						</Button>
					{:else}
						<Conclusion />
					{/if}
				</ButtonGroup.Root>
			</div>
			{@render children()}
		{/snippet}
	</Layout>
{/if}
