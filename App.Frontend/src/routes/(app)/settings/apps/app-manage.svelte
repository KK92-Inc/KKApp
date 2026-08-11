<script lang="ts">
	import { type Snippet } from 'svelte';
	import { toast } from 'svelte-sonner';

	// API & Workspace
	import { Problem, type ValidationErrors } from '$lib/api';
	import * as Workspace from '$lib/remotes/workspace.remote';

	// Components
	import * as InputGroup from '$lib/components/input-group';
	import * as Field from '$lib/components/field';
	import * as Dialog from '$lib/components/dialog';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import { Button, buttonVariants } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Separator } from '$lib/components/separator';
	import Switch from '$lib/components/switch/switch.svelte';

	// Icons
	import {
		Link,
		Plus,
		Trash2,
		Settings2,
		UserRound,
		ClipboardCheck,
		Bell,
		GitBranch,
		HeartHandshake,
		Archive
	} from '@lucide/svelte';
	import type { WithChild } from 'bits-ui';
	import type { components } from '$lib/api/api';

	type ApplicationFields = Pick<
		components['schemas']['ApplicationDO'],
		'name' | 'description' | 'enabled' | 'scopes' | 'redirectUris'
	>;

	interface Props extends WithChild {
		workspaceId: string;
		app?: components['schemas']['ApplicationDO'];
	}

	let { workspaceId, app, child: trigger }: Props = $props();
	let application = $state<ApplicationFields>({
		name: '',
		description: '',
		enabled: false,
		redirectUris: [],
		scopes: []
	});

	let open = $state(false);
	let loading = $state(false);
	let redirect = $state('');
	let errors = $state<ValidationErrors>({});
	// NOTE(W2): On Purpose we don't want it to mutate.
	// svelte-ignore state_referenced_locally
	let original = $state.snapshot(application);
	const isEditing = $derived(!!app);

	const AVAILABLE_SCOPES = [
		{
			id: 'workspace',
			label: 'Workspace',
			desc: 'Access to workspaces including read and write access to project, goals, cursi, rubrics.',
			icon: Settings2
		},
		{
			id: 'user',
			label: 'User',
			desc: 'Grants read/write access to profile info only.',
			icon: UserRound
		},
		{
			id: 'evaluation',
			label: 'Evaluations',
			desc: 'Read results and manage evaluation runs on behalf of the user.',
			icon: HeartHandshake
		},
		{
			id: 'repository',
			label: 'Repository',
			desc: 'Grants full access to make commits to git tracked entities such as projects or rubrics',
			icon: GitBranch
		},
		{
			id: 'subscription',
			label: 'Subscriptions',
			desc: 'Grants subscribe/unsubscribe access to your goals, cursi and projects.',
			icon: Archive
		}
	];

	$effect(() => {
		if (!open) {
			application = original;
			return;
		}
		if (!app) return;
		application = { ...app };
	});

	function addRedirect() {
		try {
			new URL(redirect);
			if (!application.redirectUris?.includes(redirect)) {
				application.redirectUris?.push(redirect);
			}
			redirect = '';
		} catch (_) {
			toast.error('Invalid URI format. Must be a valid URL (e.g., https://example.com/callback)');
		}
	}

	async function submit() {
		try {
			loading = true;
			if (isEditing && app?.id) {
				await Workspace.updateApplication({ ...application, id: workspaceId, appId: app.id });
				toast.success('Application updated successfully.');
			} else {
				await Workspace.createApplication({ ...application, id: workspaceId });
				toast.success('Application created successfully.');
			}
			open = false;
		} catch (error) {
			const resolved = Problem.resolve(error);
			if (resolved.kind === 'validation') {
				errors = resolved.fields;
			} else {
				toast.error(resolved.message);
			}
		} finally {
			loading = false;
		}
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Trigger>
		{#snippet child(props)}
			{@render trigger?.(props)}
		{/snippet}
	</Dialog.Trigger>

	<Dialog.Content class="flex flex-col gap-0 p-0 md:max-w-3xl">
		<Dialog.Header class="px-6 py-4">
			<Dialog.Title>
				{#if isEditing}
					Edit '{application.name}'
				{:else}
					Create new Application
				{/if}
			</Dialog.Title>
		</Dialog.Header>

		<!-- Replaced the 2-column grid with a scrollable flex column layout -->
		<div class="flex flex-col gap-8 overflow-y-auto p-6">
			<!-- 1. General Info -->
			<Field.Set>
				<Field.Group class="flex flex-col gap-4">
					<Field.Field>
						<Field.Label for="name">Name</Field.Label>
						<Input
							id="name"
							autocomplete="off"
							placeholder="Release Bot"
							bind:value={application.name}
							aria-invalid={!!errors.name}
						/>
						<Field.Error errors={errors.name} />
					</Field.Field>

					<Field.Field>
						<Field.Label for="description">Description</Field.Label>
						<Textarea
							id="description"
							autocomplete="off"
							placeholder="What does this application do, and who's it for?"
							bind:value={application.description}
							aria-invalid={!!errors.description}
						/>
						<Field.Error errors={errors.description} />
					</Field.Field>
				</Field.Group>
			</Field.Set>

			<Separator />

			<!-- 2. Callbacks -->
			<Field.Set>
				<Field.Legend class="flex items-center gap-2">
					Callback URLs
					{#if application.redirectUris?.length}
						<Badge variant="secondary" class="px-1.5 tabular-nums">
							{application.redirectUris.length}
						</Badge>
					{/if}
				</Field.Legend>
				<Field.Description>
					Once someone approves access, they're sent back to one of these URLs.
				</Field.Description>
				<Field.Group class="mt-2 gap-1.5">
					<InputGroup.Root>
						<InputGroup.Input
							bind:value={redirect}
							placeholder="http://localhost:5000/callback"
							onkeydown={(e) => {
								if (e.key === 'Enter') {
									e.preventDefault();
									addRedirect();
								}
							}}
						/>
						<InputGroup.Addon align="inline-end">
							<InputGroup.Button variant="secondary" onclick={addRedirect}>
								Add
								<Plus />
							</InputGroup.Button>
						</InputGroup.Addon>
					</InputGroup.Root>

					<Item.Group class="mt-2 gap-2">
						{#each application.redirectUris as uri (uri)}
							<Item.Root variant="outline" class="px-1 py-1 pl-3!">
								<Item.Content>
									<Item.Title class="font-mono">
										{uri}
									</Item.Title>
								</Item.Content>
								<Item.Actions>
									<Button
										type="button"
										variant="ghost"
										size="icon"
										onclick={() => {
											application.redirectUris = application.redirectUris?.filter((u) => u !== uri);
										}}
										class="size-7 text-muted-foreground hover:text-destructive"
										aria-label="Remove URI"
									>
										<Trash2 size={14} />
									</Button>
								</Item.Actions>
							</Item.Root>
						{:else}
							<Empty.Root class="border border-dashed p-2!">
								<Empty.Header>
									<Empty.Title class="text-md">No Callbacks configured</Empty.Title>
									<Empty.Description>
										Add a callback URL above, without one, nobody can finish authenticating.
									</Empty.Description>
								</Empty.Header>
							</Empty.Root>
						{/each}
					</Item.Group>
				</Field.Group>
			</Field.Set>

			<Separator />

			<!-- 3. Permissions -->
			<Field.Set>
				<Field.Legend class="flex items-center gap-2">
					Permissions
					{#if application.scopes?.length}
						<Badge variant="secondary" class="px-1.5 tabular-nums">
							{application.scopes.length}
						</Badge>
					{/if}
				</Field.Legend>
				<Field.Description>
					Scopes let you specify exactly what type of access you need. Scopes limit access for OAuth tokens.
					They do not grant any additional permission beyond that which the user already has.
				</Field.Description>
				<Field.Group class="mt-2">
					<Item.Group class="grid grid-cols-1 gap-2 md:grid-cols-2">
						{#each AVAILABLE_SCOPES as scope (scope.id)}
							{@const ScopeIcon = scope.icon}
							{@const checked = application.scopes?.includes(scope.id)}
							<Item.Root variant="outline" class={checked ? 'border-primary/40 bg-primary/3' : ''}>
								<Item.Media variant="icon">
									<ScopeIcon />
								</Item.Media>
								<Item.Content class="gap-1">
									<div class="flex flex-wrap items-center gap-1.5">
										<Item.Title>{scope.label}</Item.Title>
										<Badge variant="outline" class="font-normal text-muted-foreground">Read & write</Badge>
									</div>
									<Item.Description class="text-xs">{scope.desc}</Item.Description>
								</Item.Content>
								<Item.Actions>
									<Switch
										id={`scope-${scope.id}`}
										{checked}
										onCheckedChange={(v) => {
											if (v) {
												if (!application.scopes?.includes(scope.id)) application.scopes?.push(scope.id);
											} else {
												application.scopes = application.scopes?.filter((s) => s !== scope.id);
											}
										}}
										class="mt-0.5"
									/>
								</Item.Actions>
							</Item.Root>
						{/each}
					</Item.Group>
				</Field.Group>
			</Field.Set>
		</div>

		<!-- Pinned Footer -->
		<Dialog.Footer class="border-t px-6 py-4">
			<Button variant="outline" onclick={() => (open = false)} disabled={loading}>Cancel</Button>
			<Button onclick={submit} disabled={loading || !application.name}>
				{isEditing ? 'Save changes' : 'Create application'}
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
