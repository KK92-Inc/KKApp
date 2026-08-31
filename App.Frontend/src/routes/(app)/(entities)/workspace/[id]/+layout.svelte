<script lang="ts">
	import { page } from '$app/state'; // Import SvelteKit's page state
	import Layout from '$lib/components/layout.svelte';
	import * as Workspace from '$lib/remotes/workspace.remote';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import type { LayoutProps, PageProps } from './$types';
	import Navgroup from '$lib/components/navgroup.svelte';

	const { params, children, data }: LayoutProps = $props();

	const roles = $derived(data.session.roles);
	const staff = $derived(roles.includes('staff'));
	const subpath = $derived(page.url.pathname.replace(`/workspace/${params.id}`, '') + page.url.search);
	const user = $derived(await Workspace.current());
	const root = $derived(await (staff ? Workspace.root() : null));
</script>

<Layout cover>
	{#snippet left()}
		<Field.Set class="h-full border-r border-b bg-card p-4">
			<Field.Group class="gap-2">
				{#if staff}
					<Field.Field>
						<Field.Label class="text-foreground">Workspace</Field.Label>
						<Tabs.Root value={params.id}>
							<Tabs.List class="w-full">
								<Tabs.Trigger value={user.id} class="flex-1">
									{#snippet child({ props })}
										<a href="/workspace/{user.id}{subpath}" {...props}>My Workspace</a>
									{/snippet}
								</Tabs.Trigger>
								<Tabs.Trigger value={root!.id} class="flex-1">
									{#snippet child({ props })}
										<a href="/workspace/{root!.id}{subpath}" {...props}>Staff Workspace</a>
									{/snippet}
								</Tabs.Trigger>
							</Tabs.List>
						</Tabs.Root>
					</Field.Field>
				{/if}

				<Field.Field>
					{#key params.id}
						<Navgroup
							title="Entities"
							args={{
								id: params.id
							}}
							routes={[
								'/(app)/(entities)/workspace/[id]/cursi',
								'/(app)/(entities)/workspace/[id]/projects',
								'/(app)/(entities)/workspace/[id]/goals',
								'/(app)/(entities)/workspace/[id]/rubrics',
								'/(app)/settings/apps'
							]}
						/>
					{/key}
				</Field.Field>
			</Field.Group>
		</Field.Set>
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
