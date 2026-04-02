<script lang="ts">
	import * as Page from './index.svelte.ts';
	import * as Rubric from '$lib/remotes/rubric.remote';
	import * as Stepper from '$lib/components/stepper/index.svelte';

	const context = Page.setContext(new Page.Context());
</script>

<Stepper.Root class="container mx-auto max-w-4xl my-8 rounded-xl border bg-card p-8 shadow-sm">
	<Stepper.Header>
		<Stepper.Item value={1} title="Setup" subtitle="Identity & variants" />
		<Stepper.Item value={2} title="Criteria" subtitle="Evaluation content" />
		<Stepper.Item value={3} title="Review" subtitle="Confirm & create" />
	</Stepper.Header>

	<Stepper.Window>
		<Stepper.WindowItem value={1} class="space-y-4">
			<Page.Setup />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={2} class="space-y-4">
			<Page.Criteria />
		</Stepper.WindowItem>
		<Stepper.WindowItem value={3} class="space-y-4">
			<Page.Review />
		</Stepper.WindowItem>
	</Stepper.Window>

	<Stepper.Actions
		loading={Rubric.create.pending > 0}
		finishLabel="Create rubric"
		onfinish={async () => {
			// TODO: Finish the rest...
			await Rubric.create({
				name: context.name,
				public: context.public,
				enabled: context.enabled
			});
		}}
	/>
</Stepper.Root>
