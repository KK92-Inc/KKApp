<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { Plus, X } from '@lucide/svelte';

	// Shadcn UI Components
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import { Label } from '$lib/components/label';
	import * as Card from '$lib/components/card';
	import * as Dialog from '$lib/components/dialog';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';

	// Assume data comes from a +page.ts load function if editing
	let { data } = $props();

	// Initialize the state manager
	const context = Page.setContext(new Page.GoalContext());

	// If we landed on an edit page, hydrate the state
	$effect(() => {
		if (data?.existingGoal) {
			context.hydrate(data.existingGoal, data.associatedProjectIds || []);
		}
	});

	async function handleSubmit() {
		const result = await context.submit();
		// Navigate to the newly created/updated goal page
		await goto(`/users/${page.data.session.userId}/goals/${result.id}`);
	}

	function addProject(id: string) {
		if (context.data.projects.length < 4 && !context.data.projects.includes(id)) {
			context.data.projects.push(id);
		}
	}

	function removeProject(index: number) {
		context.data.projects.splice(index, 1);
	}
</script>

<div class="container mx-auto my-8 max-w-5xl rounded-xl border bg-card p-8 shadow-sm space-y-6">
	<div>
		<h1 class="text-2xl font-bold tracking-tight">
			{context.id ? 'Edit Goal' : 'Create Goal'}
		</h1>
		<p class="text-muted-foreground text-sm">
			{context.id ? 'Update the details and associated projects for this goal.' : 'Define a new goal and attach up to 4 projects to it.'}
		</p>
	</div>

	<Separator />

	<div class="grid grid-cols-1 md:grid-cols-12 gap-8">
		<div class="md:col-span-4 space-y-6">
			<div class="space-y-3">
				<Label for="thumbnail">Thumbnail</Label>
				<Thumbnail src="https://placehold.co/128x128?text=Goal" />
			</div>

			<div class="space-y-3">
				<Label for="name">Name*</Label>
				<Input id="name" placeholder="e.g., Learn C Basics" bind:value={context.data.name} />
			</div>

			<div class="space-y-3">
				<Label for="description">Description</Label>
				<Textarea
					id="description"
					rows={4}
					class="resize-y"
					placeholder="Describe the objective of this goal..."
					bind:value={context.data.description}
				/>
			</div>

			<div class="space-y-4 rounded-lg border p-4 bg-muted/20">
				<div class="flex items-center justify-between">
					<div class="space-y-0.5">
						<Label>Public</Label>
						<p class="text-xs text-muted-foreground">Visible to everyone.</p>
					</div>
					<Switch bind:checked={context.data.public} />
				</div>
				<div class="flex items-center justify-between">
					<div class="space-y-0.5">
						<Label>Active</Label>
						<p class="text-xs text-muted-foreground">Enable progression.</p>
					</div>
					<Switch bind:checked={context.data.active} />
				</div>
			</div>
		</div>

		<div class="md:col-span-8">
			<div class="mb-4">
				<Label class="text-base">Associated Projects</Label>
				<p class="text-sm text-muted-foreground">Select up to 4 projects that users must complete for this goal.</p>
			</div>

			<div class="grid grid-cols-2 gap-4">
				{#each Array(4) as _, i (i)}
					{@const projectId = context.data.projects[i]}

					{#if projectId}
						<Card.Root class="relative flex h-40 flex-col items-center justify-center border-solid transition-all hover:border-primary/50">
							<Button
								variant="ghost"
								size="icon-sm"
								class="absolute top-2 right-2 text-muted-foreground hover:text-destructive"
								onclick={() => removeProject(i)}
							>
								<X class="size-4" />
							</Button>
							<Card.Content class="p-4 text-center">
								<p class="font-medium text-sm">Project Attached</p>
								<p class="text-xs text-muted-foreground font-mono mt-1">{projectId}</p>
							</Card.Content>
						</Card.Root>
					{:else}
						<Dialog.Root>
							<Dialog.Trigger>
								{#snippet child({ props })}
									<button
										{...props}
										class="flex h-40 w-full flex-col items-center justify-center rounded-xl border-2 border-dashed border-muted-foreground/25 hover:border-muted-foreground/60 hover:bg-muted/30 transition-all focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
									>
										<Plus class="mb-2 h-6 w-6 text-muted-foreground" />
										<span class="text-sm font-medium text-muted-foreground">Add Project</span>
									</button>
								{/snippet}
							</Dialog.Trigger>
							<Dialog.Content class="sm:max-w-md">
								<Dialog.Header>
									<Dialog.Title>Select a Project</Dialog.Title>
									<Dialog.Description>
										Choose a placeholder project to attach to this slot.
									</Dialog.Description>
								</Dialog.Header>
								<div class="grid gap-2 py-4">
									<Button variant="outline" class="justify-start" onclick={() => addProject('proj-uuid-1111')}>
										Placeholder Project A
									</Button>
									<Button variant="outline" class="justify-start" onclick={() => addProject('proj-uuid-2222')}>
										Placeholder Project B
									</Button>
								</div>
							</Dialog.Content>
						</Dialog.Root>
					{/if}
				{/each}
			</div>
		</div>
	</div>

	<Separator />

	<div class="flex justify-end gap-3">
		<Button variant="outline" href="/manage/goals">Cancel</Button>
		<Button onclick={handleSubmit}>
			{context.id ? 'Save Changes' : 'Create Goal'}
		</Button>
	</div>
</div>
