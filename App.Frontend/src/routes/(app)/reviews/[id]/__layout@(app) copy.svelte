<script lang="ts">
	import { HierarchyEditor, type LazyHierarchyAdapter } from '$lib/components/hierarchy';
	import type { LayoutProps } from './$types';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import * as Review from '$lib/remotes/review.remote';
	import * as Git from '$lib/remotes/git.remote';
	import * as Alert from '$lib/components/alert';
	import * as Dialog from '$lib/components/dialog';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Resizable from '$lib/components/resizable';
	import * as Drawer from '$lib/components/drawer';

	import * as Avatar from '$lib/components/avatar';
	import * as Empty from '$lib/components/empty';
	import Layout from '$lib/components/layout.svelte';
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import { HeartHandshake, File, Folder, Ban, Check, BadgeQuestionMark, HeartCrack } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import ViewRubric from './view-rubric.svelte';
	import ViewProject from './view-project.svelte';

	const { params, children }: LayoutProps = $props();
	Page.setContext(new Page.Context(() => params.id));

	const data = $derived(await Remote.getData(params.id));

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

			return content.map((f) => {
				return {
					isDirectory: f.directory,
					path: f.path
				};
			});
		},
		isDraggable: () => false,
		canDrop: () => false
	};
</script>

{#if data.review.state !== 'Finished'}
	<Layout cover classL="p-4" classR="py-4 pr-4">
		{#snippet left()}
			<Skeleton class="max-h-[calc(100svh-(--header-height))] flex-1 animate-none" />
		{/snippet}
		{#snippet right()}
			{#if data.review.state === 'Cancelled'}
				<Empty.Root>
					<Empty.Header>
						<Empty.Media variant="default">
							<HeartCrack />
						</Empty.Media>
						<Empty.Title>This review has been cancelled.</Empty.Title>
						<Empty.Description>
							The evaluator or evaluatee have cancelled this review and thus there is nothing to show.
						</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{:else}
				<Empty.Root class="border">
					<Empty.Header>
						<Empty.Media variant="default">
							{#if data.review.reviewer}
								<Avatar.Root class="size-12">
									<Avatar.Image src={data.review.reviewer.avatarUrl} />
									<Avatar.Fallback>LR</Avatar.Fallback>
								</Avatar.Root>
							{:else}
								<HeartHandshake />
							{/if}
						</Empty.Media>
						<Empty.Title>
							{#if data.review.reviewer}
								{data.review.reviewer.displayName ?? data.review.reviewer.login}
								{#if data.review.state === 'Pending'}
									is going to evaluate this project.
								{:else if data.review.state === 'InProgress'}
									is currently evaluating this project.
								{/if}
							{:else if data.review.kind !== 'Auto'}
								There is no one evaluating this review.
							{:else}
								This is a automatic evaluation.
							{/if}
						</Empty.Title>
						<!-- <Empty.Description>
						There is currently no user reviweing this project
					</Empty.Description> -->
					</Empty.Header>
					<Empty.Content>
						<Button size="sm">Leave Message</Button>
					</Empty.Content>
				</Empty.Root>
			{/if}
		{/snippet}
	</Layout>
{:else}
	<Layout cover class="gap-1" classL="p-4" classR="py-4 pr-4 h-[calc(100svh-var(--header-height)-2rem)]">
		{#snippet left()}
			<svelte:boundary>
				{@const root = await Git.getTree({ id: data.review.userProject.gitInfoId, ref: data.review.sha! })}
				{@const items = root.map((f) => {
					return {
						isDirectory: f.directory,
						path: f.path
					};
				})}

				<div
					class="flex h-full max-h-[calc(100svh-(--header-height))] flex-col overflow-hidden rounded border bg-card text-card-foreground shadow-sm"
				>
					<div class="flex items-center gap-2 border-b bg-muted/40 px-4 py-3">
						<Button variant="secondary" size="sm" href="/reviews/{data.review.id}">
							Home
							<HeartHandshake size={16} class="text-muted-foreground" />
						</Button>
						<Separator class="ml-2 flex-1" />
						<ButtonGroup.Root>
							<ButtonGroup.Root>
								<Button size="icon-sm" variant="outline" class="size-6">
									<BadgeQuestionMark />
								</Button>
							</ButtonGroup.Root>

							<ButtonGroup.Root>
								<Button size="sm" variant="destructive" class="border">Cancel <Ban /></Button>
								<Button size="sm" variant="outline">Finish <Check /></Button>
							</ButtonGroup.Root>
						</ButtonGroup.Root>
					</div>

					<div class="flex-1 overflow-y-auto p-2">
						<HierarchyEditor {items} {adapter} onMove={() => {}} class="space-y-1">
							{#snippet item({ item })}
								<a
									href={item.isDirectory
										? '#'
										: `/reviews/${data.review.id}/blob/${data.review.sha}/${item.path}`}
									class="group flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-accent/50 hover:text-foreground"
								>
									{#if item.isDirectory}
										<Folder size={16} class="shrink-0" />
									{:else}
										<File
											size={16}
											class="shrink-0 text-muted-foreground transition-colors group-hover:text-foreground"
										/>
									{/if}
									<span class="truncate">{item.path.slice(item.path.lastIndexOf('/') + 1)}</span>
								</a>
							{/snippet}
						</HierarchyEditor>
					</div>
				</div>
			</svelte:boundary>
		{/snippet}
		{#snippet right()}
			<div class="max-h-[calc(100svh-var(--header-height)-2rem)]">
				{@render children()}
			</div>

			<!-- <Resizable.PaneGroup direction="vertical">
				<Resizable.Pane defaultSize={50} maxSize={75} minSize={10}>
				</Resizable.Pane>

				<Resizable.Handle withHandle class="border" />

				<Resizable.Pane defaultSize={50}>
					<Resizable.PaneGroup direction="horizontal" class="h-full min-h-0 min-w-0 overflow-hidden">
						<Resizable.Pane defaultSize={50}>
							<div
								class="h-full min-h-0 min-w-0 overflow-x-hidden overflow-y-auto overscroll-contain border-b border-l p-4"
							>
								<ViewRubric />
							</div>
						</Resizable.Pane>

						<Resizable.Handle withHandle />

						<Resizable.Pane defaultSize={50}>
							<div
								class="h-full min-h-0 min-w-0 overflow-auto overflow-x-hidden overscroll-contain border-r border-b p-4"
							>
								<ViewProject />
							</div>
						</Resizable.Pane>
					</Resizable.PaneGroup>
				</Resizable.Pane>
			</Resizable.PaneGroup> -->
		{/snippet}
	</Layout>
{/if}

<Drawer.Root modal={false} direction="right">
	<Drawer.Trigger class={buttonVariants({
		class: "fixed top-1/2 -right-9 z-30 rounded-b-none -translate-y-1/2 -rotate-90"
	})}>Non Modal</Drawer.Trigger>
	<Drawer.Content>
		<Drawer.Header>
			<Drawer.Title>Non Modal Drawer</Drawer.Title>
		</Drawer.Header>
		<div class="flex-1 p-4">
			<div class="size-full rounded-2xl bg-muted"></div>
		</div>
		<Drawer.Footer>
			<Drawer.Close class={buttonVariants()}>Close</Drawer.Close>
		</Drawer.Footer>
	</Drawer.Content>
</Drawer.Root>

<!-- {#if data.review.reviewer || data.review.state === 'Pending'}
	<Dialog.Root open>
		<Dialog.Content
			onCloseAutoFocus={(e) => e.preventDefault()}
			onEscapeKeydown={(e) => e.preventDefault()}
			onInteractOutside={(e) => e.preventDefault()}
			showCloseButton={false}
			class="max-w-lg"
		>
			<Dialog.Header>
				<Dialog.Title class="flex items-center gap-2 text-xl font-semibold">
					<HeartHandshake class="h-5 w-5 text-primary" />
					About this review
				</Dialog.Title>
				<Dialog.Description class="space-y-4 pt-2">
					{#if data.review.kind !== 'Auto'}
						<Alert.Root variant="warning">
							<InfoIcon class="h-4 w-4" />
							<Alert.Title>About starting and finishing reviews</Alert.Title>
							<Alert.Description class="mt-1 text-sm">
								{#if data.review.kind === 'Self'}
									You can start and determine when to finish this review at any point in time.
								{:else if data.review.kind === 'Async' || data.review.kind === 'Peer'}
									Upon claiming this review slot, you have 2 days to start the review and 1 when starting the
									review to finish the evaluation. Failing to complete the review in the allotted time will
									result in you being removed from the review.
								{/if}
							</Alert.Description>
						</Alert.Root>
					{/if}

					<div class="space-y-3 text-sm text-muted-foreground">
						{#if data.review.kind === 'Peer'}
							<p>
								This review has not started yet, so there is nothing to review quite yet. A peer review is a
								review of someone else's work, and it becomes available once it is claimed. Once claimed, you
								will be expected to begin the evaluation within the start window and complete it within the
								review window.
							</p>
							<div class="flex flex-col gap-2 pt-2 sm:flex-row">
								<Button class="flex-1 gap-2">
									Start Review <ArrowRight class="h-4 w-4" />
								</Button>
								<Button variant="outline" class="flex-1 gap-2">
									<Home class="h-4 w-4" /> Go Home
								</Button>
							</div>
						{:else if data.review.kind === 'Async'}
							<p>
								This asynchronous review is waiting for a reviewer. It has a start window, which is the time
								you are expected to take ownership of it, and a separate conduct window in which you complete
								the evaluation.
							</p>
							<div class="flex flex-col gap-2 pt-2 sm:flex-row">
								<Button class="flex-1 gap-2">
									Start Review <ArrowRight class="h-4 w-4" />
								</Button>
								<Button variant="outline" class="flex-1 gap-2">
									<Home class="h-4 w-4" /> Go Home
								</Button>
							</div>
						{:else if data.review.kind === 'Self'}
							<p>
								A self review is a personal reflection of either you and/or the team. The aim is to make you
								go over your own work and reflect the quality. You are reviewing your own contribution, so
								there is no other person involved in the evaluation.
							</p>
							<Button class="mt-2 w-full gap-2">
								Start my Review <ArrowRight class="h-4 w-4" />
							</Button>
						{:else if data.review.kind === 'Auto'}
							<p>
								This review is handled by the platform automatically. The system evaluates the work on your
								behalf, and once the assessment is complete, the result will be made available here. Please
								hold tight and return once the evaluation is finished.
							</p>
						{/if}
					</div>
				</Dialog.Description>
			</Dialog.Header>
		</Dialog.Content>
	</Dialog.Root>

	<div class="container mx-auto mt-4 blur">
		<div class="mb-2 flex items-center gap-2">
			<Skeleton class="h-12 w-full animate-none bg-accent/25" />
			<Skeleton class="h-12 w-full animate-none bg-accent/25" />
			<Skeleton class="h-12 w-full animate-none bg-accent/25" />
			<Skeleton class="h-12 w-full animate-none bg-accent/25" />
		</div>
		<Layout classR="max-h-[unset]! space-y-2">
			{#snippet left()}
				<Skeleton class="h-[400px] w-full animate-none bg-accent/25" />
			{/snippet}
			{#snippet right()}
				<Skeleton class="h-12 w-full animate-none bg-accent/25" />
				<Skeleton class="h-[400px] w-full animate-none bg-accent/25" />
			{/snippet}
		</Layout>
	</div>
{:else}
	<div class="container mx-auto mt-4 blur-2xl">
		<Layout classR="max-h-[unset]!">
			{#snippet left()}
				<svelte:boundary>
					{@const root = await Git.getTree({ id: data.review.userProject.gitInfoId, ref: data.review.sha! })}
					{@const items = root.map((f) => {
						return {
							isDirectory: f.directory,
							path: f.path
						};
					})}

					<HierarchyEditor {items} {adapter} onMove={() => {}} class="flex-1 rounded border p-2">
						{#snippet item({ item })}
							<a
								href="/reviews/{data.review.id}/blob/{data.review.sha}/{item.path}"
								class="font-medium text-foreground hover:underline"
							>
								{item.path.slice(item.path.lastIndexOf('/') + 1)}
							</a>
						{/snippet}
					</HierarchyEditor>
				</svelte:boundary>
			{/snippet}
			{#snippet right()}
				{@render children()}
			{/snippet}
		</Layout>
	</div>
{/if} -->
