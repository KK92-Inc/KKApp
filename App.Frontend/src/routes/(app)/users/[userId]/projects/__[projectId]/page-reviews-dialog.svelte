<script>
	import * as Page from './index.svelte';
	import * as Dialog from '$lib/components/dialog';
	import * as Rubric from '$lib/remotes/rubric.remote';
	import { Button, buttonVariants } from '$lib/components/button';

	const context = Page.getContext();
	const rubrics = $derived(await Rubric.getPage({

	}))
</script>

<Dialog.Root>
	<Dialog.Trigger
		type="button"
		class={buttonVariants({ variant: 'outline', size: 'sm'})}
	>
		Request
	</Dialog.Trigger>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Request Review</Dialog.Title>
			<Dialog.Description>Choose a rubric to evaluate your project with.</Dialog.Description>
		</Dialog.Header>

		<Dialog.Footer class="flex gap-2">
			<Button variant="outline" size="sm">Back</Button>
			<Button type="submit" size="sm" class="w-full flex-1">Request review</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
