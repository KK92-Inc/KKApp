<script lang="ts">
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import { Crown, Users } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Page from './index.svelte';

	const context = Page.getContext();
	const userProject = $derived(await context.userProject);
	const members = $derived(userProject ? await UserProjects.members({ id: userProject.id }) : []);
</script>

<Card.Root class="py-0 shadow-none">
	<Card.Content class="p-3">
		<div class="mb-2 flex items-center justify-between">
			<h3 class="flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
				<Users size={12} />
				Members
			</h3>
		</div>

		<ul class="flex items-center gap-1">
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
						<Crown size={12} class="absolute -top-1.5 left-1/2 -translate-x-1/2 text-yellow-500" />
					{/if}
				</li>
			{/each}
		</ul>
	</Card.Content>
</Card.Root>
