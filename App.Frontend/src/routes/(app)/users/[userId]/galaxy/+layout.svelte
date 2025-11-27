<script lang="ts">
	import * as InputGroup from "$lib/components/input-group";
	import Input from '$lib/components/input/input.svelte';
	import { FlaskConical, Search } from '@lucide/svelte';
	import type { LayoutProps } from './$types';
	import { Separator } from "$lib/components/separator";
	import * as Select from "$lib/components/select";

	let { children }: LayoutProps = $props();
</script>

<div class="fixed z-10 overflow-auto">
	<div
		class="bg-card/50 backdrop-blur-lg flex min-h-12 flex-col items-start gap-2 rounded-br-lg border-r border-b p-2"
	>
		<div class="flex items-center gap-2">
			<InputGroup.Root class="w-[200px]">
				<InputGroup.Input placeholder="Search..." />
				<InputGroup.Addon>
					<Search />
				</InputGroup.Addon>
				<InputGroup.Addon align="inline-end">12 items</InputGroup.Addon>
			</InputGroup.Root>

			<Select.Root type="single">
				<Select.Trigger class="w-[120px] capitalize">
					Cursus
				</Select.Trigger>
				<Select.Content>
					<!-- {#each availableCursi as option}
						<Select.Item value={option.value}>
							{option.label}
						</Select.Item>
					{/each} -->
				</Select.Content>
			</Select.Root>
		</div>
		<div class="flex gap-2 text-xs">
			<span class="text-primary flex items-center font-bold uppercase">
				<FlaskConical size={16} class="mr-1" />
				beta
			</span>
			<Separator orientation="vertical" />
			<span class="text-muted-foreground underline">
				The graph is currently experimental
			</span>
		</div>
	</div>
	<span id="breadcrumbs">
		<!--Teleport Breadcrumbs-->
	</span>
</div>

{@render children?.()}
