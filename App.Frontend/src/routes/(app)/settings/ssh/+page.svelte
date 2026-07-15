<script lang="ts">
	import * as Account from '$lib/remotes/account.remote';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button } from '$lib/components/button';
	import { useDialog } from '$lib/components/dialog';
	import { Separator } from '$lib/components/separator';
	import { KeyRound, Trash2Icon, TriangleAlert, X } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import SshHelp from './ssh-help.svelte';
	import SshAdd from './ssh-add.svelte';
	import * as Item from '$lib/components/item';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';
	import * as Empty from '$lib/components/empty';
	import Badge from '$lib/components/badge/badge.svelte';

	const dialog = useDialog();
	const keys = await Account.getKeys();
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});
</script>

<div class="flex items-center justify-between gap-4 pb-2">
	<h1 class="text-xl font-bold">Account Settings</h1>
	<Separator class="flex-1" />
	<ButtonGroup.Root>
		<SshHelp />
		<SshAdd />
	</ButtonGroup.Root>
</div>

<Alert.Root variant="warning">
	<TriangleAlert class="h-4 w-4" />
	<Alert.Title>Make sure you recognize your keys!</Alert.Title>
</Alert.Root>
<Separator class="my-2" />

<Item.Group class="gap-2">
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
					<Badge variant="outline" class="rounded-sm">{key.keyType}</Badge>
					<span>•</span>
					<span>Created {formatter.format(new Date(key.createdAt))}</span>
				</Item.Description>
			</Item.Content>
			<Item.Actions>
				<Button
					onclick={async () => {
						if (await confirm()) {
							await Account.deleteKey(key.fingerprint);
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
</Item.Group>

<!-- <div class="flex flex-col gap-2">
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


</div> -->
