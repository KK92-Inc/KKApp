<script lang="ts">
	import { cn } from '$lib/utils.js';
	import type { RemoteForm } from '@sveltejs/kit';
	import { DropdownMenu as DropdownMenuPrimitive, useId } from 'bits-ui';

	let {
		ref = $bindable(null),
		class: className,
		inset,
		href,
		remote,
		variant = 'default',
		...restProps
	}: DropdownMenuPrimitive.ItemProps & {
		inset?: boolean;
		/** If provided, the item will be rendered as an anchor element */
		href?: string;
		/** If provided, the item will be rendered as a form button */
		remote?: RemoteForm<any, unknown>;
		variant?: 'default' | 'destructive';
	} = $props();
</script>

{#snippet primitive()}
	<DropdownMenuPrimitive.Item
		bind:ref
		data-slot="dropdown-menu-item"
		data-inset={inset}
		data-variant={variant}
		class={cn(
			"relative flex cursor-pointer items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-hidden select-none data-highlighted:bg-accent data-highlighted:text-accent-foreground data-[disabled]:pointer-events-none data-[disabled]:opacity-50 data-[inset]:pl-8 data-[variant=destructive]:text-destructive data-[variant=destructive]:data-highlighted:bg-destructive/10 data-[variant=destructive]:data-highlighted:text-destructive dark:data-[variant=destructive]:data-highlighted:bg-destructive/20 [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4 [&_svg:not([class*='text-'])]:text-muted-foreground data-[variant=destructive]:*:[svg]:!text-destructive",
			className
		)}
		{...restProps}
	/>
{/snippet}

{#if remote}
	<form {...remote}>
		<button class="w-full">
			{@render primitive()}
		</button>
	</form>
{:else if href}
	<a href={href}>
		{@render primitive()}
	</a>
{:else}
	{@render primitive()}
{/if}
