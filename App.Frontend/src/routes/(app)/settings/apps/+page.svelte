<script lang="ts">
	// Svelte & Core
	import { page } from '$app/state';
	import { toast } from 'svelte-sonner';

	// API & Remotes
	import { Problem } from '$lib/api';
	import * as Workspace from '$lib/remotes/workspace.remote';

	// UI Components
	import * as AlertDialog from '$lib/components/alert-dialog';
	import Badge from '$lib/components/badge/badge.svelte';
	import { Button, buttonVariants } from '$lib/components/button';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Dialog from '$lib/components/dialog';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import * as Item from '$lib/components/item';
	import { Separator } from '$lib/components/separator';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Switch from '$lib/components/switch/switch.svelte';
	import * as Tabs from '$lib/components/tabs';
	import * as Tooltip from '$lib/components/tooltip';

	// Icons
	import {
		Blocks,
		Calendar,
		CheckCheck,
		EyeDashed,
		Handshake,
		Link,
		Pencil,
		Plus,
		RefreshCw,
		Settings,
		Trash2Icon,
		X
	} from '@lucide/svelte';

	// Local Components
	import AppHelp from './app-help.svelte';
	import AppManage from './app-manage.svelte';
	import AppConsent from './app-consent.svelte';
	import { DateFormatter } from '@internationalized/date';
	import { Toggle } from '$lib/components/toggle';

	const dialog = Dialog.useDialog();
	let consented = $state(false);
	let open = $state(false);
	let secret = $state('');
	let target = $state<'current' | 'root'>('current');
	let space = $derived(target === 'root' ? Workspace.root() : Workspace.current());
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});

	async function toggle(workspaceId: string, appId: string, enabled: boolean) {
		await Problem.try(async () => {
			await Workspace.updateApplication({ id: workspaceId, appId, enabled });
			toast.success(`Application ${enabled ? 'Enabled' : 'Disabled'}.`);
		});
	}

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
			Applications
			{#if !consented}
				<span class="text-sm text-muted-foreground">
					Limit
					{#if target === 'root'}
						({apps.length}/&infin;)
					{:else}
						({apps.length}/3)
					{/if}
				</span>
			{/if}
		</h1>
		<Separator class="flex-1" />
		<ButtonGroup.Root>
			<ButtonGroup.Root>
				<Toggle
					bind:pressed={consented}
					aria-label="Toggle bookmark"
					size="sm"
					variant="outline"
					class="h-6 data-[state=on]:bg-transparent data-[state=on]:*:[svg]:stroke-primary"
				>
					<EyeDashed />
					Show Consented
				</Toggle>
			</ButtonGroup.Root>
			<ButtonGroup.Root>
				<AppHelp />
				{#if target === 'root' || apps.length < 3}
					<AppManage workspaceId={s.id}>
						{#snippet child({ props })}
							<Button size="sm" {...props}>
								Create App
								<Plus />
							</Button>
						{/snippet}
					</AppManage>
				{:else}
					<Button disabled size="sm">Limit Reached</Button>
				{/if}
			</ButtonGroup.Root>
		</ButtonGroup.Root>
	</div>

	{#if consented}
		<p class="text-xs text-muted-foreground">
			Here you can see all current applications to which you granted your explicit consent. You are free to
			revoke access to any application. You must grant access again later if you want to use the application
			again.
		</p>
		<Separator class="my-2" />
		<AppConsent />
	{:else}
		<p class="text-xs text-muted-foreground">
			You can create custom apps to interact with the applications API. These apps can also allow you to
			authenticate other users with your app.
		</p>
		<Separator class="my-2" />

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
				<Item.Root variant="outline" class="items-start">
					<Item.Content class="min-w-0">
						<Item.Title class="flex w-full items-center gap-2">
							<Blocks size={16} />
							<span>{app.name}</span>
							{#if app.enabled}
								<Badge
									variant="outline"
									class="rounded-sm border-green-500/20 bg-green-500/10 text-green-500"
								>
									Active
								</Badge>
							{:else}
								<Badge variant="outline" class="rounded-sm">Disabled</Badge>
							{/if}
							<Separator class="w-full flex-1" />
							<DropdownMenu.Root>
								<DropdownMenu.Trigger class={buttonVariants({ variant: 'outline', size: 'sm' })}>
									Settings
									<Settings />
								</DropdownMenu.Trigger>
								<DropdownMenu.Content class="w-56" align="start">
									<DropdownMenu.Label>App Settings</DropdownMenu.Label>
									<DropdownMenu.Separator />
									<DropdownMenu.Group>
										<DropdownMenu.Item onclick={() => rotate(s.id, app.id)}>
											<RefreshCw />
											Rotate Secret
										</DropdownMenu.Item>
										<DropdownMenu.Item>
											{#snippet child({ props })}
												<AppManage workspaceId={s.id} {app} {...props}>
													{#snippet child({ props })}
														<Button
															{...props}
															variant="ghost"
															class="w-full justify-start px-2! py-1.5! font-normal! transition-none"
														>
															<Pencil class="text-muted-foreground" />
															Edit App
														</Button>
													{/snippet}
												</AppManage>
											{/snippet}
										</DropdownMenu.Item>
										<DropdownMenu.Separator />
										<DropdownMenu.Item onclick={() => remove(s.id, app.id)} variant="destructive">
											<Trash2Icon class=" text-white" />
											Delete
										</DropdownMenu.Item>
									</DropdownMenu.Group>
								</DropdownMenu.Content>
							</DropdownMenu.Root>
							<Tooltip.Root>
								<Tooltip.Trigger>
									{#snippet child({ props })}
										<Switch
											{...props}
											checked={app.enabled}
											inert={Workspace.updateApplication.pending > 0}
											onCheckedChange={(v) => toggle(s.id, app.id, v)}
										/>
									{/snippet}
								</Tooltip.Trigger>
								<Tooltip.Content>
									<p>Enable or disable the application</p>
								</Tooltip.Content>
							</Tooltip.Root>
						</Item.Title>
						<Item.Description class="mt-2 flex flex-col gap-3">
							<div class="flex flex-wrap items-center gap-1 text-xs text-muted-foreground">
								<span class="flex items-center gap-1" title="Created At">
									<Calendar size={14} />
									{formatter.format(new Date(app.createdAt))}
								</span>
								<Separator orientation="vertical" class="h-3" />
								<span class="flex items-center gap-1" title="Last Updated">
									<RefreshCw size={14} />
									{formatter.format(new Date(app.updatedAt))}
								</span>
								{#if app.redirectUris && app.redirectUris.length > 0}
									<Separator orientation="vertical" class="h-3" />
									<span class="flex items-center gap-1">
										<Link size={14} />
										{app.redirectUris.length} Redirect URI(s)
									</span>
								{/if}
							</div>

							<span class="line-clamp-2 block text-sm">
								{app.description || 'No description provided.'}
							</span>

							<div
								class="flex flex-col gap-1 rounded-md bg-muted/30 p-2 font-mono text-xs text-muted-foreground"
							>
								<span class="mr-1 text-foreground/70">App ID: {app.id}</span>
								<span class="mr-1 text-foreground/70">Client ID: {app.clientId}</span>
							</div>

							{#if app.redirectUris && app.redirectUris.length > 0}
								<div class="flex flex-wrap gap-1.5">
									{#each app.redirectUris as uri (uri)}
										<Badge variant="secondary" class="px-1.5 py-0 text-[10px] font-normal">
											{uri}
										</Badge>
									{/each}
								</div>
							{/if}
						</Item.Description>
					</Item.Content>
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
	{/if}
</svelte:boundary>
