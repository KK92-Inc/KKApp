<script lang="ts">
	import {
		Archive,
		ArrowDown,
		ChevronDown,
		GraduationCap,
		HeartHandshake,
		Info,
		Trophy,
		UserPlus
	} from '@lucide/svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { Switch } from '../switch';
	import { Separator } from '../separator';
	import ScrollArea from '../scroll-area/scroll-area.svelte';

	interface Props {
		selected: number;
		exclude: boolean;
	}

	let open = $state(false);
	let { selected = $bindable(), exclude = $bindable() }: Props = $props();

	// count set bits (popcount)
	function countBits(n: number): number {
		n = n >>> 0;
		let c = 0;
		while (n) {
			c++;
			n &= n - 1;
		}
		return c;
	}

	const selectedCount = $derived(countBits(selected ?? 0));

	const variants = {
		invite: {
			mask: 1 << 0,
			label: 'Invite',
			icon: UserPlus
		},
		project: {
			mask: 1 << 5,
			label: 'Project',
			icon: Archive
		},
		goal: {
			mask: 1 << 6,
			label: 'Goal',
			icon: Trophy
		},
		cursus: {
			mask: 1 << 7,
			label: 'Cursus',
			icon: GraduationCap
		},
		review: {
			mask: 1 << 8,
			label: 'Review',
			icon: HeartHandshake
		}
	};
</script>

<DropdownMenu.Root bind:open>
	<DropdownMenu.Trigger>
		{#snippet child({ props })}
			<Button variant="outline" {...props}>
				Filter
				<span class="text-xs text-muted-foreground">({selectedCount})</span>
				<ChevronDown />
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content>
		<DropdownMenu.Group>
			<div
				class="flex items-center justify-between gap-2 rounded-md border bg-muted/50 p-2 text-xs font-medium"
			>
				Exclude
				<Switch bind:checked={exclude} />
			</div>
			<Separator class="my-1" />
			{#each Object.values(variants) as { mask, icon: Icon, label } (mask)}
				<DropdownMenu.CheckboxItem
					closeOnSelect={false}
					bind:checked={() => Boolean(selected & mask), (v) => (selected ^= mask)}
				>
					<Icon class="size-4" />
					{label}
				</DropdownMenu.CheckboxItem>
			{/each}
		</DropdownMenu.Group>
	</DropdownMenu.Content>
</DropdownMenu.Root>
