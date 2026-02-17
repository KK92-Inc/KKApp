<script lang="ts">
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button/index.js';
	import { KeyRound, Trash2Icon, TriangleAlert, X } from '@lucide/svelte';
	import { removeKey, getKeys } from '$lib/remotes/ssh.remote';
	import { page } from '$app/state';
	import * as Empty from '$lib/components/empty';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
	import SSHAdd from './ssh-add.svelte';
	import SSHHelp from './ssh-help.svelte';
	import { useDialog } from '$lib/components/dialog';

	const dialog = useDialog();
	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});
</script>

<div class="flex flex-col gap-2">
	<div class="flex items-center justify-between gap-1">
		<h1 class="text-xl font-bold">SSH Key</h1>
		<Separator class="w-min flex-1" />
		<SSHHelp />
		<SSHAdd />
	</div>

	<Alert.Root variant="warning">
		<TriangleAlert class="h-4 w-4" />
		<Alert.Title>Make sure you recognize your keys!</Alert.Title>
	</Alert.Root>

	<svelte:boundary>
		{@const keys = await getKeys()}
		{#if keys.length === 0}
			<Empty.Root class="m-auto h-80 bg-card/30">
				<Empty.Header>
					<Empty.Media variant="icon">
						<X />
					</Empty.Media>
					<Empty.Title></Empty.Title>
				</Empty.Header>
				<Empty.Content>No SSH keys found.</Empty.Content>
			</Empty.Root>
		{:else}
			<div class="grid w-full grid-cols-2 gap-2">
				{#each keys as key (key.fingerprint)}
					{@const instanceRemove = removeKey.for(key.fingerprint)}
					<form {...instanceRemove}>
						<input
							hidden
							{...instanceRemove.fields.fingerprint.as('text')}
							value={key.fingerprint}
						/>
						<Item.Root variant="outline">
							<Item.Content class="min-w-0">
								<Item.Title>
									<KeyRound size={16} />
									<span>{key.title}</span>
								</Item.Title>
								<Item.Description>
									{key.fingerprint}
									<p class="mt-1 flex gap-2 text-xs text-muted-foreground">
										<span>{key.keyType}</span>
										<span>•</span>
										<span>Created {formatter.format(new Date(key.createdAt))}</span>
									</p>
								</Item.Description>
							</Item.Content>
							<Item.Actions>
								<Button
									onclick={async (e) => {
										e.preventDefault();
										const form = e.currentTarget.closest('form');
										await dialog
											.confirm(
												'Delete SSH Key?',
												`
											This action CANNOT be undone. This will permanently delete the SSH key and if you'd like to use it in the future, you will need to upload it again.
											`
											)
											.confirmLabel('Delete')
											.ok(() => form?.requestSubmit());
									}}
									type="submit"
									variant="ghost"
									size="icon"
									aria-label="Delete SSH Key"
								>
									<Trash2Icon class="size-4 text-destructive" />
								</Button>
							</Item.Actions>
						</Item.Root>
					</form>
				{/each}
			</div>
		{/if}
	</svelte:boundary>
</div>
