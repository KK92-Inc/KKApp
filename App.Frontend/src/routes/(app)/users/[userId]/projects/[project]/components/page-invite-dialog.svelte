<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import * as Avatar from '$lib/components/avatar';
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Badge } from '$lib/components/badge';
	import { Separator } from '$lib/components/separator';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Empty from '$lib/components/empty';
	import { getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import { getUsers } from '$lib/remotes/user.remote';
	import {
		sendInvite,
		revokeInvite,
		kickMember,
		leaveProject,
		transferLeadership
	} from '$lib/remotes/invite.remote';
	import {
		Search,
		UserPlus,
		Crown,
		X,
		DoorOpen,
		UserMinus,
		ArrowRightLeft
	} from '@lucide/svelte';
	import type { components } from '$lib/api/api';

	type Member = components['schemas']['UserProjectMemberDO'];
	type User = components['schemas']['UserDO'];

	interface Props {
		userProjectId: string;
		currentUserId: string;
		open?: boolean;
	}

	let { userProjectId, currentUserId, open = $bindable(false) }: Props = $props();

	let searchQuery = $state('');
	let searchResults = $state<User[]>([]);
	let searching = $state(false);
	let searchTimeout: ReturnType<typeof setTimeout>;

	let members = $state<Member[]>([]);
	let loadingMembers = $state(true);

	const activeMembers = $derived(members.filter((m) => !m.leftAt && m.role !== 'Pending'));
	const pendingMembers = $derived(members.filter((m) => !m.leftAt && m.role === 'Pending'));
	const currentMember = $derived(members.find((m) => m.userId === currentUserId && !m.leftAt));
	const isLeader = $derived(currentMember?.role === 'Leader');

	// Filter search results to exclude already members/pending
	const memberUserIds = $derived(new Set(members.filter((m) => !m.leftAt).map((m) => m.userId)));
	const filteredResults = $derived(
		searchResults.filter((u) => !memberUserIds.has(u.id) && u.id !== currentUserId)
	);

	async function loadMembers() {
		loadingMembers = true;
		try {
			members = await getUserProjectMembers(userProjectId);
		} finally {
			loadingMembers = false;
		}
	}

	function handleSearchInput(e: Event & { currentTarget: HTMLInputElement }) {
		searchQuery = e.currentTarget.value;
		clearTimeout(searchTimeout);

		if (!searchQuery.trim()) {
			searchResults = [];
			searching = false;
			return;
		}

		searching = true;
		searchTimeout = setTimeout(async () => {
			try {
				searchResults = await getUsers({
					login: searchQuery.trim(),
					size: 5,
					page: 1
				});
			} catch {
				searchResults = [];
			} finally {
				searching = false;
			}
		}, 300);
	}

	function resetSearch() {
		searchQuery = '';
		searchResults = [];
		searching = false;
	}

	$effect(() => {
		if (open) {
			loadMembers();
			resetSearch();
		}
	});
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-md">
		<Dialog.Header>
			<Dialog.Title>Manage Members</Dialog.Title>
			<Dialog.Description>
				{#if isLeader}
					Invite users, manage pending invites, and organize your team.
				{:else}
					View team members and pending invites.
				{/if}
			</Dialog.Description>
		</Dialog.Header>

		<!-- Search & Invite (leader only) -->
		{#if isLeader}
			<div class="space-y-2">
				<div class="relative">
					<Search
						size={16}
						class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
					/>
					<Input
						placeholder="Search users by login..."
						class="pl-9"
						value={searchQuery}
						oninput={handleSearchInput}
					/>
				</div>

				{#if searchQuery.trim()}
					<div class="max-h-40 space-y-0.5 overflow-y-auto rounded-md border p-1">
						{#if searching}
							<div class="space-y-2 p-2">
								<Skeleton class="h-8 w-full" />
								<Skeleton class="h-8 w-full" />
							</div>
						{:else if filteredResults.length === 0}
							<p class="p-3 text-center text-xs text-muted-foreground">
								No eligible users found for "{searchQuery}"
							</p>
						{:else}
							{#each filteredResults as user (user.id)}
								<div
									class="flex items-center justify-between rounded-md px-2 py-1.5 hover:bg-accent"
								>
									<div class="flex items-center gap-2">
										<Avatar.Root class="size-6 rounded">
											<Avatar.Image src={user.avatarUrl} alt={user.displayName ?? user.login} />
											<Avatar.Fallback class="text-[9px]">
												{user.login.slice(0, 2)}
											</Avatar.Fallback>
										</Avatar.Root>
										<div class="min-w-0">
											<p class="truncate text-sm font-medium">{user.login}</p>
											{#if user.displayName}
												<p class="truncate text-[11px] text-muted-foreground">
													{user.displayName}
												</p>
											{/if}
										</div>
									</div>
									<form
										{...sendInvite}
										onsubmit={() => {
											setTimeout(() => {
												loadMembers();
												resetSearch();
											}, 200);
										}}
									>
										<input
											hidden
											{...sendInvite.fields.inviteeId.as('text')}
											value={user.id}
										/>
										<input
											hidden
											{...sendInvite.fields.userProjectId.as('text')}
											value={userProjectId}
										/>
										<Button
											type="submit"
											size="sm"
											variant="ghost"
											class="h-7 gap-1 px-2 text-xs"
											loading={sendInvite.pending > 0}
										>
											<UserPlus size={14} />
											Invite
										</Button>
									</form>
								</div>
							{/each}
						{/if}
					</div>
				{/if}
			</div>

			<Separator />
		{/if}

		<!-- Pending Invites -->
		{#if pendingMembers.length > 0}
			<div class="space-y-2">
				<h4 class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
					Pending Invites ({pendingMembers.length})
				</h4>
				<div class="space-y-1">
					{#each pendingMembers as member (member.id)}
						<div class="flex items-center justify-between rounded-md px-2 py-1.5">
							<div class="flex items-center gap-2">
								<Avatar.Root class="size-7 rounded">
									<Avatar.Image
										src={member.user.avatarUrl}
										alt={member.user.displayName ?? member.user.login}
									/>
									<Avatar.Fallback class="text-[10px]">
										{member.user.login.slice(0, 2)}
									</Avatar.Fallback>
								</Avatar.Root>
								<div class="min-w-0">
									<p class="truncate text-sm">{member.user.login}</p>
								</div>
								<Badge variant="outline" class="text-[10px]">Pending</Badge>
							</div>
							{#if isLeader}
								<form
									{...revokeInvite}
									onsubmit={() => {
										setTimeout(loadMembers, 200);
									}}
								>
									<input
										hidden
										{...revokeInvite.fields.inviteeId.as('text')}
										value={member.userId}
									/>
									<input
										hidden
										{...revokeInvite.fields.userProjectId.as('text')}
										value={userProjectId}
									/>
									<Button
										type="submit"
										size="sm"
										variant="ghost"
										class="h-7 px-2 text-destructive hover:text-destructive"
										loading={revokeInvite.pending > 0}
									>
										<X size={14} />
									</Button>
								</form>
							{/if}
						</div>
					{/each}
				</div>
			</div>

			<Separator />
		{/if}

		<!-- Active Members -->
		<div class="space-y-2">
			<h4 class="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
				Members ({activeMembers.length})
			</h4>
			{#if loadingMembers}
				<div class="space-y-2">
					<Skeleton class="h-10 w-full" />
					<Skeleton class="h-10 w-full" />
				</div>
			{:else}
				<div class="space-y-1">
					{#each activeMembers as member (member.id)}
						<div class="flex items-center justify-between rounded-md px-2 py-1.5">
							<div class="flex items-center gap-2">
								<div class="relative">
									<Avatar.Root class="size-7 rounded">
										<Avatar.Image
											src={member.user.avatarUrl}
											alt={member.user.displayName ?? member.user.login}
										/>
										<Avatar.Fallback class="text-[10px]">
											{member.user.login.slice(0, 2)}
										</Avatar.Fallback>
									</Avatar.Root>
									{#if member.role === 'Leader'}
										<Crown
											size={10}
											class="absolute -top-1 left-1/2 -translate-x-1/2 text-yellow-500"
										/>
									{/if}
								</div>
								<div class="min-w-0">
									<p class="truncate text-sm">{member.user.login}</p>
								</div>
								<Badge variant={member.role === 'Leader' ? 'default' : 'secondary'} class="text-[10px]">
									{member.role}
								</Badge>
							</div>

							<div class="flex items-center gap-1">
								{#if isLeader && member.userId !== currentUserId}
									<!-- Transfer leadership -->
									<form
										{...transferLeadership}
										onsubmit={() => {
											setTimeout(loadMembers, 200);
										}}
									>
										<input
											hidden
											{...transferLeadership.fields.userProjectId.as('text')}
											value={userProjectId}
										/>
										<input
											hidden
											{...transferLeadership.fields.newLeaderId.as('text')}
											value={member.id}
										/>
										<Button
											type="submit"
											size="sm"
											variant="ghost"
											class="h-7 px-2"
											title="Transfer leadership"
											loading={transferLeadership.pending > 0}
										>
											<ArrowRightLeft size={14} />
										</Button>
									</form>
									<!-- Kick -->
									<form
										{...kickMember}
										onsubmit={() => {
											setTimeout(loadMembers, 200);
										}}
									>
										<input
											hidden
											{...kickMember.fields.memberId.as('text')}
											value={member.id}
										/>
										<input
											hidden
											{...kickMember.fields.userProjectId.as('text')}
											value={userProjectId}
										/>
										<Button
											type="submit"
											size="sm"
											variant="ghost"
											class="h-7 px-2 text-destructive hover:text-destructive"
											title="Kick member"
											loading={kickMember.pending > 0}
										>
											<UserMinus size={14} />
										</Button>
									</form>
								{:else if member.userId === currentUserId && member.role !== 'Leader'}
									<!-- Leave project -->
									<form
										{...leaveProject}
										onsubmit={() => {
											setTimeout(loadMembers, 200);
										}}
									>
										<input
											hidden
											{...leaveProject.fields.userProjectId.as('text')}
											value={userProjectId}
										/>
										<Button
											type="submit"
											size="sm"
											variant="ghost"
											class="h-7 gap-1 px-2 text-destructive hover:text-destructive"
											title="Leave project"
											loading={leaveProject.pending > 0}
										>
											<DoorOpen size={14} />
										</Button>
									</form>
								{/if}
							</div>
						</div>
					{/each}
				</div>
			{/if}
		</div>
	</Dialog.Content>
</Dialog.Root>
