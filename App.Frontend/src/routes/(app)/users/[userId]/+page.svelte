<script lang="ts">
	import { page } from '$app/state';
	import * as Avatar from '$lib/components/avatar/index.js';
	import { Badge } from '$lib/components/badge/index.js';
	import * as Card from '$lib/components/card/index.js';
	import * as Empty from '$lib/components/empty/index.js';
	import * as Item from '$lib/components/item/index.js';
	import Navgroup from '$lib/components/navgroup.svelte';
	import { Separator } from '$lib/components/separator/index.js';
	import * as User from '$lib/remotes/user.remote';
	import { cn } from '$lib/utils.js';
	import { Calendar, Code, ExternalLink, FileText, Globe, Link2, MessageCircle } from '@lucide/svelte';
	import type { PageProps } from './$types';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { PUBLIC_S3_ENDPOINT } from '$env/static/public';
	import { DateFormatter } from '@internationalized/date';

	const { params }: PageProps = $props();
	const formatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric'
	});

	const user = $derived(await User.get(params.userId));
	const avatar = $derived(`${PUBLIC_S3_ENDPOINT}/avatars/${user.id}`);
	const socials = $derived(
		[
			{ label: 'Website', url: user.details?.websiteUrl, icon: Globe },
			{ label: 'LinkedIn', url: user.details?.linkedinUrl, icon: Link2 },
			{ label: 'Reddit', url: user.details?.redditUrl, icon: MessageCircle },
			{ label: 'GitHub', url: user.details?.githubUrl, icon: Globe }
		].filter((s) => s.url)
	);

	const roleStyles: Record<string, string> = {
		developer: "bg-[url('/dev.gif')] bg-cover",
		staff: 'bg-red-700'
	};
</script>

{#snippet roleBadge(role: string)}
	{#if roleStyles[role]}
		<Badge
			variant="outline"
			class={cn('flex items-center gap-1.5 border-black px-2.5 py-1 text-white', roleStyles[role])}
		>
			<Code class="size-3.5" fill="transparent" />
			<span class="text-xs capitalize">{role}</span>
		</Badge>
	{/if}
{/snippet}

	<div class="container mx-auto max-w-5xl py-8 grid grid-cols-1 items-start gap-6 lg:grid-cols-[280px_1fr] px-4">
		<div class="flex flex-col gap-3 lg:sticky lg:top-8">
			<Card.Root class="gap-0 overflow-hidden p-0 shadow-sm">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Avatar.Root class="mx-auto size-36 rounded-lg border-2 border-background shadow-md">
						<Avatar.Image src={avatar} alt={user.login} class="block" />
						<Avatar.Fallback class="rounded-lg text-xl font-bold">
							{user.login.slice(0, 2).toUpperCase()}
						</Avatar.Fallback>
					</Avatar.Root>

					<h1 class="mt-4 text-xl font-bold tracking-tight">
						{user.displayName ?? user.login}
					</h1>
					<p class="font-mono text-sm text-muted-foreground">
						@{user.login}
					</p>

					{#if page.data.session?.roles?.length}
						<div class="mt-3 flex flex-wrap justify-center gap-1.5">
							{#each page.data.session.roles as role (role)}
								{@render roleBadge(role)}
							{/each}
						</div>
					{/if}
				</div>

				<div class="p-2">
					{#if socials.length > 0}
						<Item.Group>
							{#each socials as social, i (social.label)}
								{@const Icon = social.icon}
								<Item.Root size="sm">
									{#snippet child({ props })}
										<a href={social.url} target="_blank" rel="noopener noreferrer" {...props}>
											<Item.Media variant="icon">
												<Icon class="size-4" />
											</Item.Media>
											<Item.Content>
												<Item.Title>{social.label}</Item.Title>
											</Item.Content>
											<Item.Actions>
												<ExternalLink class="size-3.5 text-muted-foreground" />
											</Item.Actions>
										</a>
									{/snippet}
								</Item.Root>
								{#if i !== socials.length - 1}
									<Item.Separator />
								{/if}
							{/each}
						</Item.Group>
					{:else}
						<p class="px-3 py-2 text-sm text-muted-foreground">No links added yet.</p>
					{/if}

					<Separator class="my-2" />

					<div class="flex items-center gap-1.5 px-3 py-2 text-xs text-muted-foreground">
						<Calendar class="size-3.5" />
						Joined {formatter.format(new Date(user.createdAt))}
					</div>
				</div>
			</Card.Root>

			<!-- Navigation sits right under identity, still inside the sidebar -->
			<Card.Root class="p-2 shadow-sm">
				<Navgroup
					title="Navigation"
					args={{ userId: params.userId }}
					routes={[
						'/(app)/users/[userId]/cursus',
						'/(app)/users/[userId]/projects',
						'/(app)/users/[userId]/goals',
						'/(app)/users/[userId]/galaxy'
					]}
				/>
			</Card.Root>
		</div>

		<!-- Main column: bio -->
		<Card.Root class="gap-2 shadow-sm">
			{#if user.details?.markdown}
				<Card.Header class="flex items-center gap-2 border-b">
					<FileText class="size-4 text-muted-foreground" />
					<Card.Title class="text-sm font-medium text-muted-foreground">About</Card.Title>
				</Card.Header>
				<Card.Content class="markdown max-h-125 overflow-auto">
					<Markdown value={user.details.markdown} />
				</Card.Content>
			{:else}
				<Card.Content>
					<Empty.Root>
						<Empty.Header>
							<Empty.Media variant="icon">
								<FileText />
							</Empty.Media>
							<Empty.Title>No bio yet</Empty.Title>
							<Empty.Description>
								{user.displayName ?? user.login} hasn't written anything about themselves.
							</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				</Card.Content>
			{/if}
		</Card.Root>
	</div>
