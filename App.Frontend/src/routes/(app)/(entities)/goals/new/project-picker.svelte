<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button/index.js';
	import * as Dialog from '$lib/components/dialog/index.js';
	import { Input } from '$lib/components/input/index.js';
	import { Label } from '$lib/components/label/index.js';
	import { Plus, Search } from '@lucide/svelte';
	import * as InputGroup from "$lib/components/input-group";
	import { getProjects } from '$lib/remotes/project.remote';
	import useDebounce from '$lib/hooks/debounce.svelte';

	let search = $state("");
	const lookup = useDebounce((v: string) => (search = v));
</script>

<Dialog.Root>
	<Dialog.Trigger
		class="flex w-full flex-col items-center justify-center gap-2 rounded-lg border border-dashed p-6 text-muted-foreground transition-colors hover:border-primary hover:text-primary"
	>
		<Plus class="size-5" />
		<span class="text-sm">Add project</span>
	</Dialog.Trigger>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Browse Projects</Dialog.Title>
			<Dialog.Description>Search and select a project to add to this goal.</Dialog.Description>
		</Dialog.Header>
		<InputGroup.Root>
			<InputGroup.Input placeholder="Search..." oninput={(e) => lookup.fn(e.currentTarget.value)}/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>
		{#key search}
		<svelte:boundary>
			{@const projects = await getProjects({ name: search })}

			{#snippet failed()}
				Something went wrong...
			{/snippet}

			{#snippet pending()}
				Loading...
			{/snippet}

			{JSON.stringify(projects)}
		</svelte:boundary>

		{/key}
		<!-- <Dialog.Footer>
				<Dialog.Close class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
				<Button type="submit">Save changes</Button>
			</Dialog.Footer> -->
	</Dialog.Content>
</Dialog.Root>
