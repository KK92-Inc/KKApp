<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { IdCard, Pencil, Trash2, UserCheck } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { useDialog } from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as OpenDays from '$lib/remotes/openday.remote';
	import AttendeesDialog from '../attendees-dialog.svelte';
	import OpenDayDialog from './openday-dialog.svelte';

	interface Props {
		openDay: components['schemas']['ApplyEventDO'];
	}

	const dialog = useDialog();

	let editOpen = $state(false);
	let attendeesOpen = $state(false);
	let { openDay }: Props = $props();

	async function remove() {
		await dialog
			.confirm(
				`Delete '${openDay.name}'`,
				'This action is final and cannot be undone. People who signed up but were never verified are removed with it. An open day where someone was verified cannot be deleted.'
			)
			.ok(async () => {
				const deleted = await Problem.try(async () => {
					await OpenDays.remove(openDay.id);
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
			<DropdownMenu.Label>Open Day</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(openDay.id)}>
				<IdCard />
				Copy Open Day ID
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

<OpenDayDialog bind:open={editOpen} {openDay} />
<AttendeesDialog
	bind:open={attendeesOpen}
	title={openDay.name}
	load={(params) => OpenDays.getRegistrations({ id: openDay.id, ...params })}
	verify={(registration, attended) => OpenDays.setAttendance({ id: openDay.id, registrationId: registration.id, attended })}
	remove={(registration) => OpenDays.unregister({ id: openDay.id, registrationId: registration.id })}
/>
