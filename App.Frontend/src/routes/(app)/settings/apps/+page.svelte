<script lang="ts">
	import { Separator } from '$lib/components/separator';
	import { Blocks, KeyRound, Trash2Icon, RefreshCw, X } from '@lucide/svelte';
	import * as Workspace from '$lib/remotes/workspace.remote';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button } from '$lib/components/button';
	import { useDialog } from '$lib/components/dialog';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import * as AlertDialog from '$lib/components/alert-dialog';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import { toast } from 'svelte-sonner';
	import * as Tabs from '$lib/components/tabs';

	import OauthHelp from './app-help.svelte';
	import OauthAdd from './app-add.svelte';
	import { Problem } from '$lib/api';
	import { page } from '$app/state';
	import * as InputGroup from '$lib/components/input-group';
	import Switch from '$lib/components/switch/switch.svelte';

	const dialog = useDialog();
	let open = $state(false);
	let secret = $state('');
	let target = $state<'current' | 'root'>('current');
	let space = $derived(target === 'root' ? Workspace.root() : Workspace.current());

	async function remove(workspaceId: string, appId: string) {
		const confirm = await dialog.confirm(
			'Delete Application?',
			'This action CANNOT be undone. Any users currently authenticated with this app will be disconnected.'
		);

		if (!confirm) return;
		await Problem.try(async () => {
			await Workspace.removeApplication({ id: workspaceId, appId });
			toast.success('Application deleted.');
		});
	}

	async function rotate(workspaceId: string, appId: string) {
		const confirm = await dialog.confirm(
			'Rotate Client Secret?',
			'This will immediately invalidate the old secret. Your application will be unable to authenticate until you update it with the new secret.'
		);

		if (!confirm) return;
		await Problem.try(async () => {
			secret = await Workspace.rotateApplicationSecret({ id: workspaceId, appId });
			toast.success('Client secret rotated successfully.');
			open = true;
		});
	}
</script>

<AlertDialog.Root bind:open>
	<AlertDialog.Content>
		<AlertDialog.Header>
			<AlertDialog.Title>Application Secret</AlertDialog.Title>
			<AlertDialog.Description>
				Your secret has been rotated, copy this now into a safe place as it will not be shown again.
			</AlertDialog.Description>
		</AlertDialog.Header>

		<InputGroup.Root>
			<InputGroup.Input readonly value={secret} />
			<InputGroup.Addon align="inline-end">
				<InputGroup.Copy value={secret} />
			</InputGroup.Addon>
		</InputGroup.Root>

		<AlertDialog.Footer>
			<AlertDialog.Action onclick={() => (open = false)}>Ok</AlertDialog.Action>
		</AlertDialog.Footer>
	</AlertDialog.Content>
</AlertDialog.Root>

<svelte:boundary>
	{@const s = await space}
	{@const apps = await Workspace.getApplications(s.id)}

	{#snippet pending()}
		<Skeleton class="mt-4 h-16 w-full" />
		<Skeleton class="mt-4 h-16 w-full" />
	{/snippet}

	<div class="flex items-center justify-between gap-4 pb-2">
		<h1 class="text-xl font-bold">
			Third-party API Apps
			<span class="text-sm text-muted-foreground"
				>Applications
				{#if target === 'root'}
					({apps.length}/&infin;)
				{:else}
					({apps.length}/3)
				{/if}
			</span>
		</h1>
		<Separator class="flex-1" />
		<ButtonGroup.Root>
			<OauthHelp />
			{#if target === 'root' || apps.length < 3}
				<OauthAdd workspaceId={s.id} />
			{:else}
				<Button disabled size="sm">Limit Reached</Button>
			{/if}
		</ButtonGroup.Root>
	</div>

	{#if page.data.session.roles.includes('staff')}
		<Tabs.Root bind:value={target}>
			<Tabs.List class="h-8 w-full">
				<Tabs.Trigger value="current">My Workspace</Tabs.Trigger>
				<Tabs.Trigger value="root">Root</Tabs.Trigger>
			</Tabs.List>
		</Tabs.Root>

		<Separator class="my-2" />
	{/if}

	<Item.Group class="gap-2">
		{#each apps as app (app.id)}
			<Item.Root variant="outline">
				<Item.Content class="min-w-0">
					<Item.Title class="flex items-center gap-2">
						<Blocks size={16} />
						<span>{app.name}</span>
						{#if app.enabled}
							<Badge variant="outline" class="rounded-sm border-green-500/20 bg-green-500/10 text-green-500">
								Active
							</Badge>
						{:else}
							<Badge variant="outline" class="rounded-sm">Disabled</Badge>
						{/if}
					</Item.Title>
					<Item.Description>
						<span class="mt-1 block font-mono text-xs text-muted-foreground">
							Client ID: {app.clientId}
						</span>
						<span class="mt-1 line-clamp-1 block">
							{app.description || 'No description provided.'}
						</span>
					</Item.Description>
				</Item.Content>

				<Item.Actions class="flex items-center gap-1">
					<Button
						onclick={() => rotate(s.id, app.id)}
						variant="ghost"
						size="icon"
						title="Rotate Client Secret"
					>
						<RefreshCw class="size-4 text-orange-500" />
					</Button>

					<OauthAdd workspaceId={s.id} {app} />

					<Button onclick={() => remove(s.id, app.id)} variant="ghost" size="icon" title="Delete Application">
						<Trash2Icon class="size-4 text-destructive" />
					</Button>

					<Switch />
				</Item.Actions>
			</Item.Root>
		{:else}
			<Empty.Root class=" h-80 bg-card/30">
				<Empty.Header>
					<Empty.Media variant="icon">
						<X />
					</Empty.Media>
					<Empty.Title>No applications found.</Empty.Title>
					<Empty.Description>You haven't created any third-party apps yet.</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/each}
	</Item.Group>
</svelte:boundary>

<!-- <svelte:boundary>
	{@const apps = await Workspace.getApplications(space.id)}
	{#snippet pending()}
		<Skeleton class="h-16 w-full" />
		<Skeleton class="h-16 w-full" />
	{/snippet}
	<div class="flex items-center justify-between gap-4 pb-2">
		<h1 class="text-xl font-bold">
			Third-party API Apps
			<span class="text-sm text-muted-foreground">Applications ({apps.length}/3)</span>
		</h1>
		<Separator class="flex-1" />
		<ButtonGroup.Root>
			<OauthHelp />
			{#if space.owner != null && apps.length >= 3}
				<OauthAdd workspaceId={space.id} />
			{:else}
				<Button disabled size="sm">Limit Reached</Button>
			{/if}
		</ButtonGroup.Root>
	</div>

	<Separator class="my-2" />


</svelte:boundary> -->
