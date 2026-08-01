<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import { Archive, CalendarDays, Plus, Search, Settings, Users, UserSearch } from '@lucide/svelte';
	import * as Avatar from '$lib/components/avatar';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import * as User from '$lib/remotes/user.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { Label } from '$lib/components/label';
	import { Checkbox } from '$lib/components/checkbox';
	import Paginate from '$lib/components/paginate.svelte';
	import * as Empty from '$lib/components/empty';
	import * as Tabs from '$lib/components/tabs';
	import * as Page from './context.svelte';
	import { Problem } from '$lib/api';
	import Badge from '$lib/components/badge/badge.svelte';
	import UserTile from '$lib/components/user-tile.svelte';
	import * as Project from '$lib/remotes/projects.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	let index = $state(1);
	let search = $state('');
	let login = $state(false);
	let tab = $state<'search' | 'manage'>('manage');

	const context = Page.getContext();
	const project = await Project.get(context.projectId());
	const session = $derived(
		await UserProject.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search = '';
		else search = query;
	});
</script>

{#if session}
	<svelte:boundary>
		{@const members = await UserProject.getMembersPage({ id: session.id })}

		{#snippet pending()}
			<Skeleton class="h-20 w-50" />
		{/snippet}

		{#if session && session.state === 'Active'}
			<Dialog.Root>
				<Dialog.Trigger>
					{#snippet child({ props })}
						<Button {...props} size="sm" variant="outline">Manage <Settings /></Button>
					{/snippet}
				</Dialog.Trigger>
				<Dialog.Content>
					<Dialog.Header>
						<Dialog.Title class="flex items-center gap-2">
							<Users />
							Project Peers
						</Dialog.Title>
						<Dialog.Description>
							{#if tab === 'manage'}
								Here you can search for new members or manage you current ones.
							{:else if tab === 'search'}
								You can look for eligible users for this project and try to find new peers.
							{/if}
						</Dialog.Description>
					</Dialog.Header>

					<Tabs.Root bind:value={tab}>
						<Tabs.List class="w-full">
							<Tabs.Trigger value="manage">
								<Users />
								Peers
							</Tabs.Trigger>
							<Tabs.Trigger value="search">
								<UserSearch />
								Find Peers
							</Tabs.Trigger>
						</Tabs.List>
						<Tabs.Content value="manage">
							<div class="flex gap-2">
								{#each members.data as member (member.id)}
									<UserTile user={member.user}>
										{#snippet actions()}
											{#if member.role === 'Leader'}
												<Badge variant="outline" class="rounded-sm">Project Leader</Badge>
											{:else if member.role === 'Pending'}
												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(async () => {
															await UserProject.cancel({
																id: session.id,
																userId: member.user.id
															});
														});
													}}
													class="z-20 h-7"
													variant="outline"
													size="sm"
												>
													Cancel
												</Button>
											{:else}
												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(async () => {
															await UserProject.transfer({
																id: session.id,
																newLeaderId: member.id
															});
														});
													}}
													class="z-20 h-7"
													variant="outline"
													size="sm"
												>
													Make Leader
												</Button>

												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(async () => {
															await UserProject.kick({
																id: session.id,
																memberId: member.id
															});
														});
													}}
													class="z-20 h-7"
													variant="outline"
													size="sm"
												>
													Kick
												</Button>
											{/if}
										{/snippet}
									</UserTile>
								{:else}
									<Empty.Root class="col-span-full">
										<Empty.Header>
											<Empty.Media variant="icon">
												<Users />
											</Empty.Media>
											<Empty.Title>Nothing here</Empty.Title>
											<Empty.Description>
												No users were found that match what you're looking for.
											</Empty.Description>
										</Empty.Header>
									</Empty.Root>
								{/each}
							</div>
						</Tabs.Content>
						<Tabs.Content value="search" class="space-y-2">
							<InputGroup.Root class="w-auto">
								<InputGroup.Input
									placeholder="Search users..."
									value={search}
									oninput={(e) => debounced.fn(e.currentTarget.value)}
								/>
								<InputGroup.Addon>
									<Search />
								</InputGroup.Addon>
								<InputGroup.Addon align="inline-end" class="text-xs">
									<Tooltip.Root delayDuration={100}>
										<Tooltip.Trigger>
											{#snippet child({ props })}
												<span {...props} class="flex items-center gap-1">
													<Label class="text-xs" for="filter-login">Login</Label>
													<Checkbox
														id="filter-login"
														checked={login}
														onCheckedChange={() => {
															search = '';
														}}
													/>
												</span>
											{/snippet}
										</Tooltip.Trigger>
										<Tooltip.Content>
											<p>Search users by their login handle instead</p>
										</Tooltip.Content>
									</Tooltip.Root>
								</InputGroup.Addon>
							</InputGroup.Root>

							<svelte:boundary>
								{@const viable = await User.getEligiblePage({
									size: 3,
									page: index,
									id: project.id,
									type: 'Project',
									display: login ? undefined : search,
									login: login ? search : undefined
								})}

								{#snippet pending()}
									<Skeleton class="h-20 w-50" />
								{/snippet}

								<div class="flex gap-2">
									{#each viable.data.filter((u) => u.id !== page.data.session.userId) as user (user.id)}
										<UserTile {user}>
											{#snippet actions()}
												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(async () => {
															await UserProject.invite({
																id: session.id,
																userId: user.id
															});
														});
													}}
													class="z-20 h-7"
													variant="outline"
													size="sm"
												>
													Invite
													<Plus size={12} />
												</Button>
											{/snippet}
										</UserTile>
									{:else}
										<Empty.Root class="col-span-full">
											<Empty.Header>
												<Empty.Media variant="icon">
													<Users />
												</Empty.Media>
												<Empty.Title>Nothing here</Empty.Title>
												<Empty.Description>
													No users were found that match what you're looking for.
												</Empty.Description>
											</Empty.Header>
										</Empty.Root>
									{/each}
								</div>
								<Paginate
									page={index}
									onPageChange={(p) => (index = p)}
									perPage={viable.perPage}
									count={viable.count}
								/>
							</svelte:boundary>
						</Tabs.Content>
					</Tabs.Root>
				</Dialog.Content>
			</Dialog.Root>
		{/if}
	</svelte:boundary>
{/if}
