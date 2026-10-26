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
	import * as OpenDays from '$lib/remotes/openday.remote';
	import { equal } from '$lib/datetime';

	interface Props {
		open: boolean;
		/** When given the dialog edits this open day, otherwise it creates one. */
		openDay?: components['schemas']['ApplyEventDO'];
	}

	let { open = $bindable(false), openDay }: Props = $props();

	let errors = $state<ValidationErrors>({});
	let fields = $state({
		name: '',
		description: '',
		location: '',
		capacity: null as number | null,
		startsAt: undefined as string | undefined,
		deadlineAt: undefined as string | undefined
	});

	// Start from the open day (or from scratch) every time the dialog opens.
	$effect(() => {
		if (!open) return;
		errors = {};
		fields = {
			name: openDay?.name ?? '',
			description: openDay?.description ?? '',
			location: openDay?.location ?? '',
			capacity: openDay?.capacity ?? null,
			startsAt: openDay?.startsAt,
			deadlineAt: openDay?.deadlineAt
		};
	});

	const saving = $derived(OpenDays.create.pending + OpenDays.update.pending > 0);

	async function submit() {
		errors = {};
		const saved = await Problem.try(
			() => {
				if (!openDay) {
					return OpenDays.create({
						name: fields.name,
						description: fields.description || undefined,
						location: fields.location,
						capacity: fields.capacity ?? undefined,
						startsAt: fields.startsAt!,
						deadlineAt: fields.deadlineAt!
					});
				}

				// Only send what changed. An emptied capacity can't be told apart from an omitted one, so it is explicit.
				const cleared = fields.capacity == null && openDay.capacity != null;
				return OpenDays.update({
					id: openDay.id,
					name: fields.name !== openDay.name ? fields.name : undefined,
					description: fields.description !== openDay.description ? fields.description : undefined,
					location: fields.location !== openDay.location ? fields.location : undefined,
					capacity:
						fields.capacity != null && fields.capacity !== openDay.capacity ? fields.capacity : undefined,
					unlimited: cleared ? true : undefined,
					startsAt: !equal(fields.startsAt, openDay.startsAt) ? fields.startsAt : undefined,
					deadlineAt: !equal(fields.deadlineAt, openDay.deadlineAt) ? fields.deadlineAt : undefined
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
			<Dialog.Title>{openDay ? `Edit ${openDay.name}` : 'Create open day'}</Dialog.Title>
			<Dialog.Description>
				An open day is the second step of admissions. Applicants who were verified at a check-in get to see the school
				before they commit to a trial.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="openday-name">Name</Field.Label>
					<Input id="openday-name" autocomplete="off" placeholder="Open Day::2026::November" bind:value={fields.name} />
					<Field.Error errors={errors.name} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="openday-location">Location</Field.Label>
					<Input id="openday-location" autocomplete="off" placeholder="Street 1, Amsterdam" bind:value={fields.location} />
					<Field.Error errors={errors.location} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="openday-description">Description</Field.Label>
					<Textarea id="openday-description" placeholder="What to bring, where to go..." bind:value={fields.description} />
					<Field.Error errors={errors.description} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="openday-capacity">Capacity</Field.Label>
					<Input id="openday-capacity" type="number" min={1} placeholder="No limit" bind:value={fields.capacity} />
					<Field.Description>
						{#if openDay}
							Leave empty for no limit. Can't go below the {openDay.registered} people signed up.
						{:else}
							The max amount of people that can sign up. Leave empty for no limit.
						{/if}
					</Field.Description>
					<Field.Error errors={errors.capacity} />
				</Field.Field>

				<div class="grid grid-cols-2 gap-2">
					<Field.Field>
						<Field.Label for="openday-starts">Starts</Field.Label>
						<InputDate id="openday-starts" bind:value={fields.startsAt} />
						<Field.Description>When the open day begins.</Field.Description>
						<Field.Error errors={errors.startsAt} />
					</Field.Field>
					<Field.Field>
						<Field.Label for="openday-deadline">Sign-up deadline</Field.Label>
						<InputDate id="openday-deadline" bind:value={fields.deadlineAt} max={fields.startsAt} />
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
				{openDay ? 'Save' : 'Create'}
				<Save />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
