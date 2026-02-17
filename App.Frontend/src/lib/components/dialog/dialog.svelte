<!-- @component Re-usable common dialog -->
<script lang="ts">
	import type { Icon } from '@lucide/svelte';
	import * as Dialog from '.';
	import type { Snippet } from 'svelte';
	import { buttonVariants } from '../button';

	interface Props {
		title?: string;
		description?: string;
		icon?: typeof Icon;
		trigger?: Snippet<[]>;
		footer?: Snippet<[]>;
	}

	const { title, icon, trigger, description }: Props = $props();
</script>

<Dialog.Root>
	<Dialog.Trigger class={buttonVariants({ variant: 'outline' })}>
		{#if trigger}
			{@render trigger()}
		{:else}
			Open
		{/if}
	</Dialog.Trigger>

	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header class="pb-2">
			{#if title}
				<Dialog.Title>Add SSH Key for Git Access</Dialog.Title>
			{/if}
			{#if description}
				<Dialog.Description>
					Add a new SSH key, give it a name you recognize to which device it belongs to.
				</Dialog.Description>
			{/if}
		</Dialog.Header>

		<Separator class="my-1" />
		<Dialog.Footer>
			<Dialog.Close class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" class={buttonVariants({ variant: 'default' })}>Add</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
