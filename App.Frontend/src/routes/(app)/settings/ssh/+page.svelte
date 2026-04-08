<script lang="ts">
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button/index.js';
	import { KeyRound, Trash2Icon, TriangleAlert, X } from '@lucide/svelte';
	import * as SSH from '$lib/remotes/ssh.remote';
	import { page } from '$app/state';
	import * as Empty from '$lib/components/empty';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
	import SSHAdd from './ssh-add.svelte';
	import SSHHelp from './ssh-help.svelte';
	import { useDialog } from '$lib/components/dialog';

	const dialog = useDialog();
	const keys = $derived(await SSH.get({}));
	const confirm = $derived(
		dialog.confirm(
			'Delete SSH Key?',
			"This will permanently delete the SSH key and if you'd like to use it in the future, you will need to upload it again."
		)
	);
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

	{#each keys as key (key.fingerprint)}
		<Item.Root variant="outline">
			<Item.Content class="min-w-0">
				<Item.Title>
					<KeyRound size={16} />
					<span>{key.title}</span>
				</Item.Title>
				<Item.Description>
					<span class="block">
						{key.fingerprint}
					</span>
					<span>{key.keyType}</span>
					<span>•</span>
					<span>Created {formatter.format(new Date(key.createdAt))}</span>
				</Item.Description>
			</Item.Content>
			<Item.Actions>
				<Button
					onclick={async () => {
						if (await confirm) {
							await SSH.remove({ fingerprint: key.fingerprint });
						}
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
	{:else}
		<Empty.Root class="m-auto h-80 bg-card/30">
			<Empty.Header>
				<Empty.Media variant="icon">
					<X />
				</Empty.Media>
				<Empty.Title>No SSH keys found.</Empty.Title>
			</Empty.Header>
		</Empty.Root>
	{/each}
	<!-- {#each keys as key (key.title)}
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
					onclick={async () => {
						if (await confirm) {
							await SSH.remove({ fingerprint: key.fingerprint });
						}
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
	{:else}
		<Empty.Root class="m-auto h-80 bg-card/30">
			<Empty.Header>
				<Empty.Media variant="icon">
					<X />
				</Empty.Media>
				<Empty.Title>No SSH keys found.</Empty.Title>
			</Empty.Header>
		</Empty.Root>
	{/each} -->
</div>
