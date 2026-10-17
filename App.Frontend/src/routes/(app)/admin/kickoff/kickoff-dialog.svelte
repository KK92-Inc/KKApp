<script lang="ts">
	import { Save } from '@lucide/svelte';
	import { Button, buttonVariants } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import InputDate from '$lib/components/input-date.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Problem, type ValidationErrors } from '$lib/api';
	import type { components } from '$lib/api/api';
	import * as Kickoffs from '$lib/remotes/kickoff.remote';

	interface Props {
		open: boolean;
		/** When given the dialog edits this kickoff, otherwise it creates one. */
		kickoff?: components['schemas']['KickoffDO'];
	}

	let { open = $bindable(false), kickoff }: Props = $props();

	let errors = $state<ValidationErrors>({});
	let fields = $state<Pick<components['schemas']['KickoffDO'], 'name' | 'capacity' | 'startsAt'>>({
		name: '',
		capacity: 0,
		startsAt: "",
	});

	// Start from the kickoff (or from scratch) every time the dialog opens.
	$effect(() => {
		if (!open) return;
		errors = {};
		fields = {
			name: kickoff?.name ?? '',
			capacity: kickoff?.capacity ?? 0,
			startsAt: kickoff?.startsAt ?? ""
		};
	});

	const saving = $derived(Kickoffs.create.pending + Kickoffs.update.pending > 0);

	async function submit() {
		errors = {};
		const saved = await Problem.try(
			() => {
				if (!kickoff)
					return Kickoffs.create({ ...fields });
				return Kickoffs.update({ id: kickoff.id, ...fields });
			},
			{ onValidation: (fields) => (errors = fields) }
		);

		if (!saved) return;
		open = false;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>{kickoff ? `Edit ${kickoff.name}` : 'Create kickoff'}</Dialog.Title>
			<Dialog.Description>
				A kickoff is the launch date of a cohort. Once it starts, its applicants are promoted to students.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="kickoff-name">Name</Field.Label>
					<Input id="kickoff-name" autocomplete="off" placeholder="October 2026" bind:value={fields.name} />
					<Field.Error errors={errors.name} />
				</Field.Field>
				<div class="grid grid-cols-2 gap-2">
					<Field.Field>
						<Field.Label for="kickoff-capacity">Capacity</Field.Label>
						<Input id="kickoff-capacity" type="number" min={1} bind:value={fields.capacity} />
						<Field.Description>
							{#if kickoff}
								Can't go below the {kickoff.members} current members.
							{:else}
								The max amount of users that can join.
							{/if}
						</Field.Description>
						<Field.Error errors={errors.capacity} />
					</Field.Field>
					<Field.Field>
						<Field.Label for="kickoff-starts">Starts</Field.Label>
						<InputDate id="kickoff-starts" bind:value={fields.startsAt} disabled={!!kickoff?.startedAt} />
						<Field.Description>
							{kickoff?.startedAt ? 'Already started, the date is fixed.' : 'When the cohort is launched.'}
						</Field.Description>
						<Field.Error errors={errors.startsAt} />
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" loading={saving} onclick={submit}>
				{kickoff ? 'Save' : 'Create'}
				<Save />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
