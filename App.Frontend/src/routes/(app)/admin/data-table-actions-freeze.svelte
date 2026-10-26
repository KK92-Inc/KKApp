<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button';
	import type { components } from '$lib/api/api';
	import {
		Snowflake,
	} from '@lucide/svelte';
	import { useDialog } from '$lib/components/dialog';
	import * as Dialog from '$lib/components/dialog';
	import InputDate from '$lib/components/input-date.svelte';
	import * as Field from '$lib/components/field';
	import { Textarea } from '$lib/components/textarea';
	import Separator from '$lib/components/separator/separator.svelte';

	interface Props {
		open: boolean;
		user: components['schemas']['UserDO'];
	}

	const dialog = useDialog();
	let startAt = $state<string>();
	let endsAt = $state<string>();
	let { user, open = $bindable(false) }: Props = $props();

	async function submit() {
		const confirm = await dialog.confirm();
		if (!confirm) return;

		// TODO: Anonymize...
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Freeze {user.login}</Dialog.Title>
			<Dialog.Description>
				A freeze temporarily closes a users account for a given duration.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="reason">
						Reason
						<span class="ml-auto text-xs font-normal">
							{0}/255
						</span>
					</Field.Label>
					<Textarea
						id="reason"
						rows={3}
						class="max-h-52 resize-y"
						maxlength={255}
						placeholder="Took an arrow to the knee."
					/>
				</Field.Field>
				<div class="grid grid-cols-2 gap-2">
					<Field.Field>
						<Field.Label for="start">From</Field.Label>
						<InputDate id="start" bind:value={startAt} />
						<Field.Description>When to freeze the account.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="end">Until</Field.Label>
						<InputDate id="end" bind:value={endsAt} min={startAt} />
						<Field.Description>When to allow access again.</Field.Description>
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" variant="destructive" onclick={() => submit()}>
				Freeze <Snowflake />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
