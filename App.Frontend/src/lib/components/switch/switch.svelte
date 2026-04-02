<script lang="ts">
	import { Switch as SwitchPrimitive } from "bits-ui";
	import { cn, type WithoutChildrenOrChild } from "$lib/utils.js";

	let {
		ref = $bindable(null),
		class: className,
		checked = $bindable(false),
		...restProps
	}: WithoutChildrenOrChild<SwitchPrimitive.RootProps> = $props();
</script>

<SwitchPrimitive.Root
	bind:ref
	bind:checked
	data-slot="switch"
	class={cn(
		"group peer relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-md border px-0.5",
		"data-[state=checked]:bg-primary data-[state=unchecked]:bg-input dark:data-[state=unchecked]:bg-input/80",
		"shadow-xs outline-none transition-all",
		"focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50",
		"disabled:cursor-not-allowed disabled:opacity-50",
		className
	)}
	{...restProps}
>
	<!-- "I" label — visible when ON -->
	<span
		class="pointer-events-none absolute left-2.5 select-none text-[9px] font-bold leading-none text-primary-foreground opacity-0 transition-opacity group-data-[state=checked]:opacity-100"
	>
		I
	</span>

	<!-- "O" label — visible when OFF -->
	<span
		class="pointer-events-none absolute right-2.5 select-none text-[9px] font-bold leading-none text-foreground opacity-100 transition-opacity group-data-[state=checked]:opacity-0"
	>
		O
	</span>

	<SwitchPrimitive.Thumb
		data-slot="switch-thumb"
		class={cn(
			"pointer-events-none block size-4 rounded ring-0 transition-transform",
			"bg-background dark:data-[state=unchecked]:bg-foreground dark:data-[state=checked]:bg-primary-foreground",
			"data-[state=unchecked]:translate-x-0 data-[state=checked]:translate-x-5"
		)}
	/>
</SwitchPrimitive.Root>
