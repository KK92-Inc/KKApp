<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import { Button } from '$lib/components/button';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import { Crown } from '@lucide/svelte';

	interface Props {
		userProjectId: string;
	}

	const { userProjectId }: Props = $props();
</script>

<ul class="flex items-center gap-1">
	<svelte:boundary>
		{@const members = await getUserProjectMembers(userProjectId)}

		{#snippet pending()}
			<Skeleton class="size-8 rounded" />
			<Skeleton class="size-8 rounded" />
		{/snippet}

		{#each members.filter((m) => !m.leftAt) as member (member.id)}
			<li id="member-{member.id}" class="relative w-fit">
				<Button size="icon" href="/users/{member.userId}" variant="ghost" class="size-8">
					<Avatar.Root class="size-7 rounded">
						<Avatar.Image src={member.user.avatarUrl} alt={member.user.displayName} />
						<Avatar.Fallback class="text-[10px]">
							{member.user.login.slice(0, 2)}
						</Avatar.Fallback>
					</Avatar.Root>
				</Button>
				{#if member.role === 'Leader'}
					<Crown
						size={12}
						class="absolute -top-1.5 left-1/2 -translate-x-1/2 text-yellow-500"
					/>
				{/if}
			</li>
		{/each}
	</svelte:boundary>
</ul>
