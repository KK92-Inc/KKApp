<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { IdCard, Pencil, Trash2 } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { useDialog } from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as Trials from '$lib/remotes/trial.remote';
	import TrialDialog from './trial-dialog.svelte';

	interface Props {
		trial: components['schemas']['TrialDO'];
	}

	const dialog = useDialog();

	let editOpen = $state(false);
	let { trial }: Props = $props();

	async function remove() {
		await dialog
			.confirm(
				`Delete '${trial.name}'`,
				'This action is final and cannot be undone. A trial that still has participants cannot be deleted.'
			)
			.ok(async () => {
				const deleted = await Problem.try(async () => {
					await Trials.remove(trial.id);
					return true;
				});

				if (deleted) await refreshAll({ includeLoadFunctions: false });
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
			<DropdownMenu.Label>Trial</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(trial.id)}>
				<IdCard />
				Copy Trial ID
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

<TrialDialog bind:open={editOpen} {trial} />
