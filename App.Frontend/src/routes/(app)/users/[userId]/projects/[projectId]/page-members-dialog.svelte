<script lang="ts">
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import {
		ArrowUp,
		Clock3,
		Crown,
		LogOut,
		Plus,
		Search,
		Settings,
		UserRound,
		Users,
		UserSearch,
		X
	} from '@lucide/svelte';
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
	// import * as Project from '$lib/remotes/projects.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	let index = $state(0);
	let search = $state('');
	let login = $state(false);
	let tab = $state<'search' | 'manage'>('manage');

	const context = Page.getContext();
	// const project = await Project.get(context.projectId());
	const session = $derived(
		await UserProject.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const debounced = useDebounce((query: string) => {
		index = 0;
		search = query;
	});
</script>

{#if session}
	<svelte:boundary>
		{@const members = await UserProject.getMembersPage({ id: session.id })}
		{@const team = members.data.filter((m) => !m.leftAt)}
		{@const currentMember = team.find((member) => member.userId === page.data.session.userId)}
		{@const isLeader = currentMember?.role === 'Leader'}

		{#snippet pending()}
			<Skeleton class="h-20 w-50" />
		{/snippet}

		{#if session && session.state === 'Active'}
			<Dialog.Root>
				<Dialog.Trigger>
					{#snippet child({ props })}
						<Button {...props} size="sm" variant="outline">
							{#if isLeader}
								Manage
								<Settings />
							{:else}
								View Members
								<Users />
							{/if}
						</Button>
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
							{#if isLeader}
								<Tabs.Trigger value="search">
									<UserSearch />
									Find Peers
								</Tabs.Trigger>
							{/if}
						</Tabs.List>
						<Tabs.Content value="manage">
							<Item.Group class="grid grid-cols-3 gap-2">
								{#each team as member (member.id)}
									<Item.User user={member.user}>
										{#snippet actions()}
											{#if member.role === 'Leader'}
												<Badge
													variant="outline"
													class="rounded-sm p-1.5"
													title="Project Leader"
													aria-label="Project Leader"
												>
													<Crown size={16} />
												</Badge>
											{:else if member.role === 'Pending'}
												<Badge variant="outline" class="rounded-sm p-1.5" title="Pending" aria-label="Pending">
													<Clock3 size={16} />
												</Badge>
											{:else}
												<Badge
													variant="outline"
													class="rounded-sm p-1.5"
													title="Project Member"
													aria-label="Project Member"
												>
													<UserRound size={16} />
												</Badge>
											{/if}

											{#if isLeader && member.role === 'Pending'}
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
													class="z-20"
													variant="outline"
													size="icon-sm"
													title="Cancel invitation"
													aria-label="Cancel invitation"
												>
													<X size={14} />
												</Button>
											{:else if isLeader && member.role !== 'Leader'}
												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(async () => {
															await UserProject.transfer({
																id: session.id,
																// A user id: the backend looks the member up by entity + user, not by the membership row.
																newLeaderId: member.userId
															});
														});
													}}
													class="z-20"
													variant="outline"
													size="icon-sm"
													title="Make project leader"
													aria-label="Make project leader"
												>
													<ArrowUp size={14} />
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
													class="z-20"
													variant="outline"
													size="icon-sm"
													title="Remove member"
													aria-label="Remove member"
												>
													<X size={14} />
												</Button>
											{:else if currentMember?.userId === member.userId}
												<Button
													onclick={(e) => {
														e.preventDefault();
														e.stopPropagation();
														Problem.try(() => UserProject.leave(session.id));
													}}
													class="z-20"
													variant="outline"
													size="icon-sm"
													title="Leave project"
													aria-label="Leave project"
												>
													<LogOut size={14} />
												</Button>
											{/if}
										{/snippet}
									</Item.User>
								{:else}
									<Item.Root class="col-span-full">
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
									</Item.Root>
								{/each}
							</Item.Group>
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
														onCheckedChange={(checked) => {
															login = checked;
															index = 0;
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
									id: context.projectId(),
									type: 'Project',
									display: login ? undefined : search || undefined,
									login: login ? search || undefined : undefined
								})}

								<!-- Nobody who is already on the team, or invited, needs to be offered again. -->
								{@const offered = viable.data.filter(
									(u) => u.id !== page.data.session.userId && !team.some((m) => m.userId === u.id)
								)}

								{#snippet pending()}
									<Skeleton class="h-20 w-50" />
								{/snippet}

								<Item.Group class="grid grid-cols-3 gap-2">
									{#each offered as user (user.id)}
										<Item.User {user}>
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
										</Item.User>
									{:else}
										<Item.Root class="col-span-full">
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
										</Item.Root>
									{/each}
								</Item.Group>
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
