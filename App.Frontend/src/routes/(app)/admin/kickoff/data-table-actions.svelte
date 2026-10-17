<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { IdCard, Pencil, Trash2 } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { useDialog } from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as Kickoffs from '$lib/remotes/kickoff.remote';
	import KickoffDialog from './kickoff-dialog.svelte';

	interface Props {
		kickoff: components['schemas']['KickoffDO'];
	}

	const dialog = useDialog();

	let editOpen = $state(false);
	let { kickoff }: Props = $props();

	async function remove() {
		await dialog
			.confirm(
				`Delete '${kickoff.name}'`,
				'This action is final and cannot be undone. A kickoff that still has users cannot be deleted, remove them first.'
			)
			.ok(async () => {
				await Problem.try(async () => {
					await Kickoffs.remove(kickoff.id);
				});
			});
	}
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger>
		{#snippet child({ props })}
			<Button {...props} variant="ghost" size="icon" class="relative size-8 p-0">
				<span class="sr-only">Open menu</span>
				<EllipsisIcon />
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content>
		<DropdownMenu.Group>
			<DropdownMenu.Label>Kickoff</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(kickoff.id)}>
				<IdCard />
				Copy Kickoff ID
			</DropdownMenu.Item>
			<DropdownMenu.Item onclick={() => (editOpen = true)}>
				<Pencil />
				Edit
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Item variant="destructive" onclick={remove}>
				<Trash2 />
				Delete
			</DropdownMenu.Item>
		</DropdownMenu.Group>
	</DropdownMenu.Content>
</DropdownMenu.Root>

<KickoffDialog bind:open={editOpen} {kickoff} />
