<script lang="ts">
	import * as AlertDialog from '$lib/components/alert-dialog';
	import { buttonVariants } from '$lib/components/button';
	import type { DialogActionContext } from './context.svelte.js';

	interface Props {
		ctx: DialogActionContext;
	}

	const { ctx }: Props = $props();

	const isOpen = $derived(ctx.current?.options.type === 'alert');
	const options = $derived(ctx.current?.options);
</script>

<AlertDialog.Root
	open={isOpen}
	onOpenChange={(open) => {
		if (!open) ctx.dismiss();
	}}
>
	<AlertDialog.Content class="sm:max-w-md">
		<AlertDialog.Header>
			{#if options?.title}
				<AlertDialog.Title>{options.title}</AlertDialog.Title>
			{/if}
			{#if options?.message}
				<AlertDialog.Description>{options.message}</AlertDialog.Description>
			{/if}
		</AlertDialog.Header>
		<AlertDialog.Footer>
			<AlertDialog.Action class={buttonVariants({ variant: 'default' })} onclick={() => ctx.accept()}>
				{options?.confirmLabel ?? 'OK'}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
