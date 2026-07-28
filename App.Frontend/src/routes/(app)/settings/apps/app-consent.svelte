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
	import { Blocks, Calendar, Link, Pencil, Plus, RefreshCw, Settings, Trash2Icon, X } from '@lucide/svelte';

	// Local Components
	import { DateFormatter } from '@internationalized/date';

	const dialog = Dialog.useDialog();
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});

	async function revoke(appId: string) {
		if (
			await dialog.confirm(
				'Revoke Access ?',
				"This will revoke your access to the app and the app's access to your account. You can always re-grant access when trying to use the application again."
			)
		) {
			await Problem.try(async () => {
				await Workspace.revokeConsent(appId);
				toast.success(`Application access has been revoked`);
			});
		}
	}
</script>

<Item.Group class="gap-2">
	<svelte:boundary>
		{@const apps = await Workspace.getConsentedApps()}
		{#each apps as app (app.id)}
			<Item.Root variant="outline" class="items-start">
				<Item.Content class="min-w-0">
					<Item.Title class="flex w-full items-center gap-2">
						<Blocks size={16} />
						<span>{app.name}</span>
						{#if app.enabled}
							<Badge variant="outline" class="rounded-sm border-green-500/20 bg-green-500/10 text-green-500">
								Active
							</Badge>
						{:else}
							<Badge variant="outline" class="rounded-sm">Disabled</Badge>
						{/if}
						<Separator class="w-full flex-1" />

						<Tooltip.Root>
							<Tooltip.Trigger>
								{#snippet child({ props })}
									<Button {...props} size="sm" variant="destructive" onclick={() => revoke(app.id)}>
										Revoke Access
										<Trash2Icon />
									</Button>
								{/snippet}
							</Tooltip.Trigger>
							<Tooltip.Content>
								<p>Revoke your consent from the app.</p>
							</Tooltip.Content>
						</Tooltip.Root>
					</Item.Title>
					<Item.Description class="mt-2 flex flex-col gap-3">
						{#if app.scopes}
							<div class="flex flex-wrap gap-1.5">
								Scopes:
								{#each app.scopes as scope (scope)}
									<Badge variant="outline" class="rounded-sm capitalize">
										{scope}
									</Badge>
								{/each}
							</div>
							<Separator />
						{/if}
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
						</div>
						<span class="line-clamp-2 block text-sm">
							{app.description}
						</span>
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
					<Empty.Description>You haven't granted any third-party apps consent yet.</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/each}
	</svelte:boundary>
</Item.Group>
