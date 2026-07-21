<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import * as Alert from '$lib/components/alert';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { Crown, ClockFading, Users, CalendarDaysIcon, Plus, Settings } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { Badge } from '$lib/components/badge';
	import * as HoverCard from '$lib/components/hover-card';
	import { DateFormatter } from '@internationalized/date';
	import Separator from '$lib/components/separator/separator.svelte';
	import Failed from '$lib/components/empty/failed.svelte';
	import * as ButtonGroup from '$lib/components/button-group';

	const context = Page.getContext();
	const formatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric'
	});
</script>

<Card.Root class="gap-2 py-3">
	<svelte:boundary>
		{#snippet failed(error, reset)}
			<Failed {error} {reset} />
		{/snippet}

		{@const members = await context.members()}
		{@const abandoned = members.find((v) => v.userId === page.data.session.userId && v.leftAt)}
		<Card.Header class="flex items-center justify-between px-4">
			<Card.Title
				class="flex items-center gap-2 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
			>
				<Users size={16} />
				Members
				{members.length} / {context.project.maxMembers}
			</Card.Title>
			<Card.Action>
				<ButtonGroup.Root>
					<Button size="sm" variant="outline">Manage <Settings /></Button>
					<!-- <Button size="sm" variant="outline">View All <HeartHandshake /></Button> -->
				</ButtonGroup.Root>
			</Card.Action>
		</Card.Header>
		<Card.Content class="px-3">
			{#if abandoned}
				<Alert.Root class="border-dashed">
					<ClockFading />
					<Alert.Title>Project is currently Inactive</Alert.Title>
					<Alert.Description>
						You left this project on {formatter.format(new Date(abandoned.leftAt!))}.
					</Alert.Description>
				</Alert.Root>
			{:else if !context.userProject}
				<p class="text-xs text-muted-foreground">
					{#if context.userId() === page.data.session.userId}
						You haven't subscribed to this project yet.
					{:else}
						This user hasn't subscribed to this project yet.
					{/if}
				</p>
			{/if}

			<ul class="flex items-center gap-1">
				{#each members?.filter((m) => !m.leftAt) as member (member.id)}
					<HoverCard.Root>
						<HoverCard.Trigger
							href="/users/{member.user.id}"
							target="_blank"
							rel="noreferrer noopener"
							class="font-bold underline-offset-4 hover:underline"
						>
							{#snippet child({ props })}
								<li id="member-{member.id}" class="relative w-fit">
									<Button {...props} size="icon" variant="ghost" class="size-8">
										<Avatar.Root class="size-8 rounded-sm">
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
							{/snippet}
						</HoverCard.Trigger>
						<HoverCard.Content class="text-left">
							<div class="flex gap-3">
								<Avatar.Root class="size-12 shrink-0 rounded-sm border">
									<Avatar.Image
										class="rounded-sm"
										src={member.user.avatarUrl ?? 'https://placehold.co/400'}
									/>
									<Avatar.Fallback class="rounded-sm">
										{member.user.login.slice(0, 2).toUpperCase()}
									</Avatar.Fallback>
								</Avatar.Root>

								<div class="flex-1 space-y-1">
									<h4 class="grid grid-cols-[auto_1fr_auto] items-center text-sm leading-none font-bold">
										{member.user.displayName}
										<span class="ml-1 text-xs font-normal text-muted-foreground">
											@{member.user.login}
										</span>
										{#if member.role === 'Leader'}
											<Badge>Leader <Crown /></Badge>
										{/if}
									</h4>
									<Separator />
									<div class="flex items-center text-xs text-muted-foreground">
										<CalendarDaysIcon class="me-1.5 size-3.5 opacity-70" />
										<span>Joined {formatter.format(new Date(member.user.createdAt))}</span>
									</div>
									<div class="flex items-center text-xs text-muted-foreground">
										<CalendarDaysIcon class="me-1.5 size-3.5 opacity-70" />
										<span>Member since {formatter.format(new Date(member.createdAt))}</span>
									</div>
								</div>
							</div>
						</HoverCard.Content>
					</HoverCard.Root>
				{/each}
			</ul>
		</Card.Content>
	</svelte:boundary>
</Card.Root>
