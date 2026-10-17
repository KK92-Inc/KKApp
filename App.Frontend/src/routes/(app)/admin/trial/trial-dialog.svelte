<script lang="ts">
	import { Save } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button, buttonVariants } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import * as Field from '$lib/components/field';
	import * as Select from '$lib/components/select';
	import { Input } from '$lib/components/input';
	import InputDate from '$lib/components/input-date.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { PAGINATION_MAX, Problem, type ValidationErrors } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as Cursus from '$lib/remotes/cursus.remote';
	import * as Trials from '$lib/remotes/trial.remote';
	import { sameMoment } from '../format';

	interface Props {
		open: boolean;
		/** When given the dialog edits this trial, otherwise it creates one. */
		trial?: components['schemas']['TrialDO'];
	}

	let { open = $bindable(false), trial }: Props = $props();

	let errors = $state<ValidationErrors>({});
	let fields = $state({
		name: '',
		cursusId: '',
		capacity: null as number | null,
		startsAt: undefined as string | undefined,
		endsAt: undefined as string | undefined
	});

	// Start from the trial (or from scratch) every time the dialog opens.
	$effect(() => {
		if (!open) return;
		errors = {};
		fields = {
			name: trial?.name ?? '',
			cursusId: trial?.cursusId ?? '',
			capacity: trial?.capacity ?? null,
			startsAt: trial?.startsAt,
			endsAt: trial?.endsAt
		};
	});

	const saving = $derived(Trials.create.pending + Trials.update.pending > 0);

	async function submit() {
		errors = {};
		if (!fields.cursusId) errors.cursusId = [{ message: 'Pick a cursus.' }];
		if (fields.capacity === null) errors.capacity = [{ message: 'Enter a capacity.' }];
		if (!fields.startsAt) errors.startsAt = [{ message: 'Pick a start date.' }];
		if (!fields.endsAt) errors.endsAt = [{ message: 'Pick an end date.' }];
		if (Object.keys(errors).length > 0) return;

		const saved = await Problem.try(
			() => {
				if (!trial) {
					return Trials.create({
						name: fields.name,
						cursusId: fields.cursusId,
						capacity: fields.capacity!,
						startsAt: fields.startsAt!,
						endsAt: fields.endsAt!
					});
				}

				// Only send what changed: a running trial refuses a new start date or cursus, even an identical one.
				return Trials.update({
					id: trial.id,
					name: fields.name !== trial.name ? fields.name : undefined,
					cursusId: fields.cursusId !== trial.cursusId ? fields.cursusId : undefined,
					capacity: fields.capacity !== trial.capacity ? fields.capacity! : undefined,
					startsAt: !sameMoment(fields.startsAt, trial.startsAt) ? fields.startsAt : undefined,
					endsAt: !sameMoment(fields.endsAt, trial.endsAt) ? fields.endsAt : undefined
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
			<Dialog.Title>{trial ? `Edit ${trial.name}` : 'Create trial'}</Dialog.Title>
			<Dialog.Description>
				A trial (piscine) is a period in which registered users work on a cursus. Its users are activated when it
				starts and wrapped up when it ends.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="trial-name">Name</Field.Label>
					<Input id="trial-name" autocomplete="off" placeholder="October 2026" bind:value={fields.name} />
					<Field.Error errors={errors.name} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="trial-cursus">Cursus</Field.Label>
					<svelte:boundary>
						{@const cursi = await Cursus.getPage({ page: 0, size: PAGINATION_MAX })}

						{#snippet pending()}
							<Skeleton class="h-9" />
						{/snippet}

						{#snippet failed()}
							<div class="text-sm text-red-500">Error loading cursus</div>
						{/snippet}

						<Select.Root
							type="single"
							name="trial-cursus"
							bind:value={fields.cursusId}
							disabled={!!trial?.startedAt}
						>
							<Select.Trigger id="trial-cursus">
								{cursi.data.find((c) => c.id === fields.cursusId)?.name ?? 'Select a cursus'}
							</Select.Trigger>
							<Select.Content>
								<Select.Group>
									<Select.Label>Cursus</Select.Label>
									{#each cursi.data as c (c.id)}
										<Select.Item value={c.id} label={c.name}>
											{c.name}
										</Select.Item>
									{/each}
								</Select.Group>
							</Select.Content>
						</Select.Root>
					</svelte:boundary>
					<Field.Description>
						{trial?.startedAt ? 'Already running, the cursus is fixed.' : 'What the participants work on.'}
					</Field.Description>
					<Field.Error errors={errors.cursusId} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="trial-capacity">Capacity</Field.Label>
					<Input id="trial-capacity" type="number" min={1} bind:value={fields.capacity} />
					<Field.Description>
						{#if trial}
							Can't go below the {trial.participants} current participants.
						{:else}
							The max amount of users that can take part.
						{/if}
					</Field.Description>
					<Field.Error errors={errors.capacity} />
				</Field.Field>

				<div class="grid grid-cols-2 gap-2">
					<Field.Field>
						<Field.Label for="trial-starts">Starts</Field.Label>
						<InputDate id="trial-starts" bind:value={fields.startsAt} disabled={!!trial?.startedAt} />
						<Field.Description>When the trial begins.</Field.Description>
						<Field.Error errors={errors.startsAt} />
					</Field.Field>
					<Field.Field>
						<Field.Label for="trial-ends">Ends</Field.Label>
						<InputDate
							id="trial-ends"
							bind:value={fields.endsAt}
							min={fields.startsAt}
							disabled={!!trial?.endedAt}
						/>
						<Field.Description>When the trial is wrapped up.</Field.Description>
						<Field.Error errors={errors.endsAt} />
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" loading={saving} onclick={submit}>
				{trial ? 'Save' : 'Create'}
				<Save />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
