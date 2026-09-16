<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { Button, buttonVariants } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import type { components } from '$lib/api/api';
	import { Hammer, HatGlasses, IdCard, Snowflake, User } from '@lucide/svelte';
	import { useDialog } from '$lib/components/dialog';
	import * as Dialog from '$lib/components/dialog';
	import { Input } from '$lib/components/input';
	import * as Field from '$lib/components/field';
	import { Textarea } from '$lib/components/textarea';
	import Separator from '$lib/components/separator/separator.svelte';

	interface Props {
		user: components['schemas']['UserDO'];
	}

	const dialog = useDialog();

	let freezeOpen = $state(false);
	let { user }: Props = $props();

	async function anonymize() {
		await dialog
			.confirm(
				`Anonymize '${user.login}'`,
				'This action is final and cannot be undone, please consider this action carefully.'
			)
			.input(user.login, user.login)
			.ok(() => {
				console.log('Yup!');
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
			<DropdownMenu.Label>Account</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(user.id)}>
				<IdCard />
				Copy User ID
			</DropdownMenu.Item>
			<DropdownMenu.Separator />

			<DropdownMenu.Item href="/users/{user.id}">
				<User />
				View Profile
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Sub>
				<DropdownMenu.SubTrigger>
					<Hammer />
					Actions
				</DropdownMenu.SubTrigger>
				<DropdownMenu.SubContent>
					<DropdownMenu.Item variant="destructive" onclick={() => (freezeOpen = true)}>
						<Snowflake />
						Freeze
					</DropdownMenu.Item>
					<DropdownMenu.Item variant="destructive" onclick={anonymize}>
						<HatGlasses />
						Anonymize
					</DropdownMenu.Item>
				</DropdownMenu.SubContent>
			</DropdownMenu.Sub>
		</DropdownMenu.Group>
	</DropdownMenu.Content>
</DropdownMenu.Root>

<Dialog.Root bind:open={freezeOpen}>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Freeze {user.login}</Dialog.Title>
			<Dialog.Description>
				A freeze temporarily closes a users account for a given duration.
			</Dialog.Description>
		</Dialog.Header>

		<!-- TODO: Fetch a users freeze status first. -->

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
						<Input id="start" type="datetime-local" />
						<Field.Description>When to freeze the account.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="end">Until</Field.Label>
						<Input id="end" type="datetime-local" />
						<Field.Description>When to allow access again.</Field.Description>
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit">Save changes</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
