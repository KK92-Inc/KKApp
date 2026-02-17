<!-- @component Alert action dialog -->
<script lang="ts">
	import * as Dialog from "$lib/components/dialog";
	import { buttonVariants } from "$lib/components/button";
	import type { DialogActionContext } from "./context.svelte.js";

	interface Props {
		ctx: DialogActionContext;
	}

	const { ctx }: Props = $props();

	const isOpen = $derived(ctx.current?.options.type === "alert");
	const options = $derived(ctx.current?.options);
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
		<Dialog.Footer>
			<button
				class={buttonVariants({ variant: "default" })}
				onclick={() => ctx.accept()}
			>
				{options?.confirmLabel ?? "OK"}
			</button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
