<script lang="ts">
	import * as AlertDialog from '$lib/components/alert-dialog';
	import { Input } from '$lib/components/input';
	import { buttonVariants } from '$lib/components/button';
	import type { DialogActionContext } from './context.svelte.js';

	interface Props {
		ctx: DialogActionContext;
	}

	const { ctx }: Props = $props();

	const isOpen = $derived(ctx.current?.options.type === 'confirm');
	const options = $derived(ctx.current?.options);
	const requiresInput = $derived(!!options?.inputMatch);
	const inputMatches = $derived(!requiresInput || ctx.inputValue === options?.inputMatch);
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

		{#if requiresInput}
			<div class="flex flex-col gap-1.5">
				<p class="text-sm text-muted-foreground">
					Type <strong class="text-foreground select-none">{options?.inputMatch}</strong> to confirm.
				</p>
				<Input
					bind:value={ctx.inputValue}
					placeholder={options?.placeholder ?? ''}
					onkeydown={(e) => {
						if (e.key === 'Enter' && inputMatches) ctx.accept();
					}}
				/>
			</div>
		{/if}

		<AlertDialog.Footer>
			<AlertDialog.Cancel class={buttonVariants({ variant: 'outline' })} onclick={() => ctx.dismiss()}>
				{options?.cancelLabel ?? 'Cancel'}
			</AlertDialog.Cancel>
			<AlertDialog.Action
				class={buttonVariants({ variant: 'default' })}
				disabled={!inputMatches}
				onclick={() => ctx.accept()}
			>
				{options?.confirmLabel ?? 'Confirm'}
			</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>
