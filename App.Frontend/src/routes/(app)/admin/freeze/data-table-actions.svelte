<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { FileText, IdCard, SnowflakeIcon, User } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { useDialog } from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import * as Freezes from '$lib/remotes/freeze.remote';
	import FreezeDialog from './freeze-dialog.svelte';
	import { cancellable, freezeState, type Freeze } from './state';

	interface Props {
		freeze: Freeze;
	}

	const dialog = useDialog();

	let open = $state(false);
	let { freeze }: Props = $props();

	async function cancel() {
		const upcoming = freezeState(freeze) === 'upcoming';

		await dialog
			.confirm(
				`Cancel the freeze of '${freeze.user.login}'`,
				upcoming
					? 'The freeze has not started yet. Once cancelled, the account will not be frozen.'
					: 'This ends the freeze right now and re-enables the account.'
			)
			.confirmLabel('Cancel freeze')
			.cancelLabel('Keep freeze')
			.ok(async () => {
				await Problem.try(async () => {
					await Freezes.cancel(freeze.user.id);
					return true;
				});
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
			<DropdownMenu.Label>Freeze</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => (open = true)}>
				<FileText />
				View Reason
			</DropdownMenu.Item>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(freeze.id)}>
				<IdCard />
				Copy Freeze ID
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/users/{freeze.user.id}">
				<User />
				View Account
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		{#if cancellable(freeze)}
			<DropdownMenu.Separator />
			<DropdownMenu.Group>
				<DropdownMenu.Item variant="destructive" onclick={cancel}>
					<SnowflakeIcon />
					Cancel Freeze
				</DropdownMenu.Item>
			</DropdownMenu.Group>
		{/if}
	</DropdownMenu.Content>
</DropdownMenu.Root>

<FreezeDialog bind:open={open} {freeze} />
