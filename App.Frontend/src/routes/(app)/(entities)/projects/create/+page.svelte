<script lang="ts">
	import * as Page from './index.svelte.ts';
	import * as Project from '$lib/remotes/project.remote'; // Ensure this matches actual remote structure
	import * as Stepper from '$lib/components/stepper/index.svelte';

	const context = Page.setContext(new Page.Context());
</script>

<Stepper.Root class="container mx-auto my-8 max-w-4xl rounded-xl border bg-card p-8 shadow-sm">
	<Stepper.Header>
		<Stepper.Item value={1} title="Setup" subtitle="Identity & configuration" />
		<Stepper.Item value={2} title="Markdown" subtitle="Project content" />
		<Stepper.Item value={3} title="Review" subtitle="Confirm & create" />
	</Stepper.Header>

	<Stepper.Window>
		<Stepper.WindowItem value={1} class="space-y-4">
			<Page.Setup />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={2} class="space-y-4">
			<Page.Markdown />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={3} class="space-y-4">
			<Page.Review />
		</Stepper.WindowItem>
	</Stepper.Window>

	<Stepper.Actions
		loading={Project.create.pending > 0}
		finishLabel="Create project"
		onfinish={async () => {
			await Project.create({
				avatarUrl: "https://placehold.co/96x96?text=Thumbnail",
				workspace: context.workspace[0] || "",
				active: context.active,
				public: context.public,
				name: context.name,
				description: context.description,
				maxUsers: context.isGroup ? context.maxUsers : 1,
				markdown: context.markdown
			});
		}}
	/>
</Stepper.Root>
