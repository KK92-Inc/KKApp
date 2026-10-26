<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import type { components } from '$lib/api/api';
	import {
		HatGlasses,
		IdCard,
		Plus,
		Snowflake,
		User,
		UserPen,
		Zap
	} from '@lucide/svelte';
	import { useDialog } from '$lib/components/dialog';
	import DataTableActionsFreeze from './data-table-actions-freeze.svelte';
	import DataTableActionsSubscribe from './data-table-actions-subscribe.svelte';

	interface Props {
		user: components['schemas']['UserDO'];
	}

	const dialog = useDialog();

	let freeze = $state(false);
	let subscribe = $state(false);
	let { user }: Props = $props();

	async function anonymize() {
		await dialog
			.confirm(
				`Anonymize '${user.login}'`,
				'This action is final and cannot be undone, please consider this action carefully.'
			)
			.input(user.login, user.login)
			.ok(() => {});
	}
</script>

<DataTableActionsFreeze bind:open={freeze} {user} />
<DataTableActionsSubscribe bind:open={subscribe} {user} />
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
				View Account
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/users/manage/{user.id}">
				<UserPen />
				Edit Account
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Sub>
				<DropdownMenu.SubTrigger>
					<Zap />
					Actions
				</DropdownMenu.SubTrigger>
				<DropdownMenu.SubContent>
					<DropdownMenu.Item onclick={() => (subscribe = true)}>
						<Plus />
						Manage Subscriptions
					</DropdownMenu.Item>
					<DropdownMenu.Separator />
					<DropdownMenu.Item variant="destructive" onclick={() => (freeze = true)}>
						<Snowflake />
						View Freeze
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
