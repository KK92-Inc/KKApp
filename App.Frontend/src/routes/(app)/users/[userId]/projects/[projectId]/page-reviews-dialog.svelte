<script>
	import { Button, buttonVariants } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
</script>

<Dialog.Root>
	<Dialog.Trigger
		type="button"
		class={buttonVariants({ variant: 'outline', size: 'sm', class: 'h-5 px-1.5 text-[10px]' })}
	>
		Request Review
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
