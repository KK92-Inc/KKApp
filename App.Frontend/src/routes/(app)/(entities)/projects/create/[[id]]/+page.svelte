<script lang="ts">
	import * as Page from './context.svelte';
	import * as Project from '$lib/remotes/project.remote';
	import * as Stepper from '$lib/components/stepper/index.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import PageStepOverview from './page-step-overview.svelte';
	import PageSetupStructure from './page-setup-structure.svelte';
	import Button from '$lib/components/button/button.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context());
	async function submit() {
		const project = await context.submit();
		await goto(`/users/${page.data.session.userId}/projects/${project.id}`);
	}
</script>

<div class="container mx-auto my-8 max-w-4xl rounded-xl border bg-card p-8 shadow-sm space-y-3">
	<PageStepOverview />
	{#if !params.id}
		<PageSetupStructure />
	{/if}
	<Separator />
	<Button class="ms-auto" onclick={submit}>
		Create Project
	</Button>
</div>

<!-- <Stepper.Root class="container mx-auto my-8 max-w-4xl rounded-xl border bg-card p-8 shadow-sm">
	<Stepper.Header>
		<Stepper.Item value={1} title="Setup" subtitle="Identity & configuration" />
		<Stepper.Item value={2} title="Structure" subtitle="Project content" />
		<Stepper.Item value={3} title="Review" subtitle="Confirm & create" />
	</Stepper.Header>

	<Stepper.Window>
		<Stepper.WindowItem value={1} class="space-y-4">
			<Separator />
			<Overview />
			<Separator />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={2} class="space-y-4">
			<Separator />
			<Structure />
			<Separator />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={3} class="space-y-4">
			<Separator />
			<Final />
			<Separator />
		</Stepper.WindowItem>
	</Stepper.Window>

	<Stepper.Actions
		loading={Project.create.pending > 0}
		finishLabel="Create project"
		onfinish={async () => {
			await Project.create({
				workspace: context.workspace,
				active: true,
				public: true,
				...context.project,
			});
		}}
	/>
</Stepper.Root> -->
