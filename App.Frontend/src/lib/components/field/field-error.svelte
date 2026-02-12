<script lang="ts">
	import { cn } from "$lib/utils.js";
	import type { HTMLAttributes } from "svelte/elements";
	import type { Snippet } from "svelte";

	let {
		class: className,
		children,
		errors,
		...restProps
	}: HTMLAttributes<HTMLDivElement> & {
		children?: Snippet;
		errors?: { message?: string }[];
	} = $props();

	const messages = $derived(
		(errors ?? []).map((e) => e?.message).filter((m): m is string => !!m)
	);

	const hasContent = $derived(!!children || messages.length > 0);
</script>

{#if hasContent}
	<div
		role="alert"
		data-slot="field-error"
		class={cn("text-destructive text-sm font-normal", className)}
		{...restProps}
	>
		{#if children}
			{@render children()}
		{:else if messages.length === 1}
			{messages[0]}
		{:else}
			<ul class="ml-4 flex list-disc flex-col gap-0.5 text-xs">
				{#each messages as msg (msg)}
					<li>{msg}</li>
				{/each}
			</ul>
		{/if}
	</div>
{/if}
