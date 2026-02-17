<!-- @component Confirm action dialog (with optional input-to-confirm) -->
<script lang="ts">
	import * as Dialog from "$lib/components/dialog";
	import { Input } from "$lib/components/input";
	import { buttonVariants } from "$lib/components/button";
	import type { DialogActionContext } from "./context.svelte.js";

	interface Props {
		ctx: DialogActionContext;
	}

	const { ctx }: Props = $props();

	const isOpen = $derived(ctx.current?.options.type === "confirm");
	const options = $derived(ctx.current?.options);
	const requiresInput = $derived(!!options?.inputMatch);
	const inputMatches = $derived(
		!requiresInput || ctx.inputValue === options?.inputMatch
	);
</script>

<Dialog.Root
	open={isOpen}
	onOpenChange={(open) => {
		if (!open) ctx.dismiss();
	}}
>
	<Dialog.Content showCloseButton={false} class="sm:max-w-md">
		<Dialog.Header>
			{#if options?.title}
				<Dialog.Title>{options.title}</Dialog.Title>
			{/if}
			{#if options?.message}
				<Dialog.Description>{options.message}</Dialog.Description>
			{/if}
		</Dialog.Header>

		{#if requiresInput}
			<div class="flex flex-col gap-1.5">
				<p class="text-muted-foreground text-sm">
					Type <strong class="text-foreground select-none">{options?.inputMatch}</strong> to confirm.
				</p>
				<Input
					bind:value={ctx.inputValue}
					placeholder={options?.placeholder ?? ""}
					onkeydown={(e) => {
						if (e.key === "Enter" && inputMatches) ctx.accept();
					}}
				/>
			</div>
		{/if}

		<Dialog.Footer>
			<button
				class={buttonVariants({ variant: "outline" })}
				onclick={() => ctx.dismiss()}
			>
				{options?.cancelLabel ?? "Cancel"}
			</button>
			<button
				class={buttonVariants({ variant: "default" })}
				disabled={!inputMatches}
				onclick={() => ctx.accept()}
			>
				{options?.confirmLabel ?? "Confirm"}
			</button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
