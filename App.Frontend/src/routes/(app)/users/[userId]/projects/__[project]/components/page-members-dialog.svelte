<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import { Button, buttonVariants } from '$lib/components/button';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Pagination from '$lib/components/pagination';
	import { getUserProjectByProjectId } from '$lib/remotes/user-project.remote';
	import { ArrowRightLeft, DoorOpen, Search, UserMinus, X } from '@lucide/svelte';
	import { page } from '$app/state';
	import * as Dialog from '$lib/components/dialog';
	import type { components } from '$lib/api/api';
	import * as InputGroup from '$lib/components/input-group';
	import Separator from '$lib/components/separator/separator.svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import * as Item from '$lib/components/item/';
	import Plus from '@lucide/svelte/icons/plus';
	import { getUsers } from '$lib/remotes/user.remote';
	import * as Invite from '$lib/remotes/member.remote';

	type Role = UserProjectMemberDO['role'];
	type UserProjectMemberDO = components['schemas']['UserProjectMemberDO'];

	interface Props {
		role: Role;
		members: UserProjectMemberDO[];
	}

	const { role, members }: Props = $props();
	const debounced = useDebounce(async (q: string) => (search = q.trim()), 300);
	let search = $state('');
	let index = $state(1);

	const userProject = $derived(
		await getUserProjectByProjectId({
			projectId: page.params.project,
			userId: page.data.session.userId
		})
	);

	const users = $derived(
		await getUsers({
			display: search.trim(),
			page: index,
			size: 5
		})
	);
</script>

<Dialog.Root>
	<Dialog.Trigger class={buttonVariants({ variant: 'outline', size: 'sm', class: 'h-5 px-1.5 text-[10px]' })}>
		{#if role === 'Leader'}Manage{:else}View{/if}
		Members
	</Dialog.Trigger>
	<Dialog.Content class="sm:max-w-[425px]">
		<Dialog.Header>
			<Dialog.Title>Members</Dialog.Title>
			<Dialog.Description>
				{#if role === 'Leader'}
					Invite users, manage pending invites, and organize your team.
				{:else}
					View team members and pending invites.
				{/if}
			</Dialog.Description>
		</Dialog.Header>

		{#if role === 'Leader'}
			<Separator />
			<div class="flex w-full max-w-md flex-col gap-2">
				<svelte:boundary>
					{#snippet pending()}
						<div class="space-y-3">
							<Skeleton class="h-5 w-full" />
							<Skeleton class="h-5 w-2/3" />
						</div>
					{/snippet}

					<h4
						class="flex items-center justify-between text-sm font-semibold tracking-wide text-muted-foreground uppercase"
					>
						Invite users
						{#if users.pages > 1 && search.length > 0}
							<Pagination.Root count={users.count} perPage={users.perPage} class="m-0 w-min">
								{#snippet children()}
									<Pagination.Content>
										<Pagination.Item><Pagination.Previous /></Pagination.Item>
										<Pagination.Item><Pagination.Next /></Pagination.Item>
									</Pagination.Content>
								{/snippet}
							</Pagination.Root>
						{/if}
					</h4>

					<InputGroup.Root>
						<InputGroup.Input
							placeholder="Search users..."
							oninput={(e) => debounced.fn(e.currentTarget.value)}
						/>
						<InputGroup.Addon><Search /></InputGroup.Addon>
					</InputGroup.Root>

					{#if search.length > 0}
						<Item.Group>
							{#each users.data as user (user.id)}
								{@const isMember = members.some((m) => m.userId === user.id && !m.leftAt)}
								<Item.Root class="p-1">
									<Item.Media>
										<Avatar.Root>
											<Avatar.Image src={user.avatarUrl} class="grayscale" />
											<Avatar.Fallback>{user.login.charAt(0)}</Avatar.Fallback>
										</Avatar.Root>
									</Item.Media>
									<Item.Content class="gap-1">
										<Item.Title>{user.login}</Item.Title>
									</Item.Content>
									<Item.Actions>
										<Button
											disabled={isMember}
											onclick={() => Invite.send({ inviteeId: user.id, userProjectId: userProject?.id })}
											variant="ghost"
											size="icon-sm"
											class="rounded"
										>
											<Plus size={16} />
										</Button>
									</Item.Actions>
								</Item.Root>
							{:else}
								<p class="p-3 text-center text-xs text-muted-foreground">
									No eligible users found for "{search}"
								</p>
							{/each}
						</Item.Group>
					{/if}
				</svelte:boundary>
			</div>
			<Separator />
		{/if}

		<div class="flex w-full max-w-md flex-col gap-2">
			<h4 class="text-xs font-semibold tracking-wide text-muted-foreground uppercase">
				Members ({members.length})
			</h4>
			<Item.Group>
				{#each members as member (member.id)}
					<Item.Root class="p-1">
						<Item.Media>
							<Avatar.Root>
								<Avatar.Image src={member.user.avatarUrl} class="grayscale" />
								<Avatar.Fallback>{member.user.login.charAt(0)}</Avatar.Fallback>
							</Avatar.Root>
						</Item.Media>
						<Item.Content class="gap-1">
							<Item.Title>{member.user.login}</Item.Title>
							<Item.Description class="text-xs text-muted-foreground">
								{member.role}
							</Item.Description>
						</Item.Content>
						<Item.Actions>
							{#if role === 'Leader' && member.userId !== page.data.session.userId}
								{#if member.role !== 'Pending'}
									<Button
										onclick={() =>
											Invite.transfer({ userProjectId: userProject?.id, newLeaderId: member.userId })}
										variant="ghost"
										size="icon-sm"
										class="rounded"
									>
										<ArrowRightLeft />
									</Button>
									<Button
										onclick={() => Invite.kick({ memberId: member.id, userProjectId: userProject?.id })}
										variant="destructive"
										size="icon-sm"
										class="rounded"
									>
										<UserMinus />
									</Button>
								{:else}
									<Button
										onclick={() => Invite.revoke({ inviteeId: member.userId, userProjectId: userProject?.id })}
										variant="destructive"
										size="icon-sm"
										class="rounded"
									>
										<X />
									</Button>
								{/if}
							{:else if member.userId === page.data.session.userId && member.role !== 'Leader'}
								<Button
									onclick={() => Invite.leave({ userProjectId: userProject?.id })}
									variant="destructive"
									size="icon-sm"
									class="rounded"
								>
									<DoorOpen />
								</Button>
							{/if}
						</Item.Actions>
					</Item.Root>
				{/each}
			</Item.Group>
		</div>
	</Dialog.Content>
</Dialog.Root>
