<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { IdCard, Pencil, Trash2, UserCheck } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { useDialog } from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as CheckIns from '$lib/remotes/checkin.remote';
	import AttendeesDialog from '../attendees-dialog.svelte';
	import CheckInDialog from './checkin-dialog.svelte';

	interface Props {
		checkIn: components['schemas']['ApplyEventDO'];
	}

	const dialog = useDialog();

	let editOpen = $state(false);
	let attendeesOpen = $state(false);
	let { checkIn }: Props = $props();

	async function remove() {
		await dialog
			.confirm(
				`Delete '${checkIn.name}'`,
				'This action is final and cannot be undone. People who signed up but were never verified are removed with it. A check-in where someone was verified cannot be deleted.'
			)
			.ok(async () => {
				const deleted = await Problem.try(async () => {
					await CheckIns.remove(checkIn.id);
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
			<DropdownMenu.Label>Check-In</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(checkIn.id)}>
				<IdCard />
				Copy Check-In ID
			</DropdownMenu.Item>
			<DropdownMenu.Item onclick={() => (attendeesOpen = true)}>
				<UserCheck />
				Attendees
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

<CheckInDialog bind:open={editOpen} {checkIn} />
<AttendeesDialog
	bind:open={attendeesOpen}
	title={checkIn.name}
	load={(params) => CheckIns.getRegistrations({ id: checkIn.id, ...params })}
	verify={(registration, attended) =>
		CheckIns.setAttendance({ id: checkIn.id, registrationId: registration.id, attended })}
	remove={(registration) => CheckIns.unregister({ id: checkIn.id, registrationId: registration.id })}
/>
