<script lang="ts">
	import { Save } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button, buttonVariants } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import InputDate from '$lib/components/input-date.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Problem, type ValidationErrors } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as CheckIns from '$lib/remotes/checkin.remote';
	import { equal } from '$lib/datetime';

	interface Props {
		open: boolean;
		/** When given the dialog edits this check-in, otherwise it creates one. */
		checkIn?: components['schemas']['ApplyEventDO'];
	}

	let { open = $bindable(false), checkIn }: Props = $props();

	let errors = $state<ValidationErrors>({});
	let fields = $state({
		name: '',
		description: '',
		location: '',
		capacity: null as number | null,
		startsAt: undefined as string | undefined,
		deadlineAt: undefined as string | undefined
	});

	// Start from the check-in (or from scratch) every time the dialog opens.
	$effect(() => {
		if (!open) return;
		errors = {};
		fields = {
			name: checkIn?.name ?? '',
			description: checkIn?.description ?? '',
			location: checkIn?.location ?? '',
			capacity: checkIn?.capacity ?? null,
			startsAt: checkIn?.startsAt,
			deadlineAt: checkIn?.deadlineAt
		};
	});

	const saving = $derived(CheckIns.create.pending + CheckIns.update.pending > 0);
	async function submit() {
		errors = {};
		const saved = await Problem.try(
			() => {
				if (!checkIn) {
					return CheckIns.create({
						name: fields.name,
						description: fields.description,
						location: fields.location,
						capacity: fields.capacity ?? undefined,
						startsAt: fields.startsAt!,
						deadlineAt: fields.deadlineAt!
					});
				}

				const cleared = fields.capacity == null && checkIn.capacity != null;
				return CheckIns.update({
					id: checkIn.id,
					name: fields.name,
					description: fields.description,
					location: fields.location,
					capacity: fields.capacity,
					unlimited: cleared ? true : undefined,
					startsAt: fields.startsAt,
					deadlineAt: fields.deadlineAt
				});
			},
			{ onValidation: (found) => (errors = found) }
		);

		if (!saved) return;
		await refreshAll({ includeLoadFunctions: false });
		open = false;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>{checkIn ? `Edit ${checkIn.name}` : 'Create check-in'}</Dialog.Title>
			<Dialog.Description>
				A check-in is the first step of admissions. Staff verify in person that an applicant is who they say they
				are, which is what lets them sign up for an open day.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="checkin-name">Name</Field.Label>
					<Input id="checkin-name" autocomplete="off" placeholder="Check-In::2026::November" bind:value={fields.name} />
					<Field.Error errors={errors.name} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="checkin-location">Location</Field.Label>
					<Input id="checkin-location" autocomplete="off" placeholder="Street 1, Amsterdam" bind:value={fields.location} />
					<Field.Error errors={errors.location} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="checkin-description">Description</Field.Label>
					<Textarea id="checkin-description" placeholder="What to bring, where to go..." bind:value={fields.description} />
					<Field.Error errors={errors.description} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="checkin-capacity">Capacity</Field.Label>
					<Input id="checkin-capacity" type="number" min={1} placeholder="No limit" bind:value={fields.capacity} />
					<Field.Description>
						{#if checkIn}
							Leave empty for no limit. Can't go below the {checkIn.registered} people signed up.
						{:else}
							The max amount of people that can sign up. Leave empty for no limit.
						{/if}
					</Field.Description>
					<Field.Error errors={errors.capacity} />
				</Field.Field>

				<div class="grid grid-cols-2 gap-2">
					<Field.Field>
						<Field.Label for="checkin-starts">Starts</Field.Label>
						<InputDate id="checkin-starts" bind:value={fields.startsAt} />
						<Field.Description>When the check-in begins.</Field.Description>
						<Field.Error errors={errors.startsAt} />
					</Field.Field>
					<Field.Field>
						<Field.Label for="checkin-deadline">Sign-up deadline</Field.Label>
						<InputDate id="checkin-deadline" bind:value={fields.deadlineAt} max={fields.startsAt} />
						<Field.Description>The last moment to sign up.</Field.Description>
						<Field.Error errors={errors.deadlineAt} />
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" loading={saving} onclick={submit}>
				{checkIn ? 'Save' : 'Create'}
				<Save />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
