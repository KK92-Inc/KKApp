<script lang="ts">
	import { page } from '$app/state';
	import * as Avatar from '$lib/components/avatar/index.js';
	import { Badge } from '$lib/components/badge/index.js';
	import * as Card from '$lib/components/card/index.js';
	import Navgroup from '$lib/components/navgroup.svelte';
	import { Separator } from '$lib/components/separator/index.js';
	import * as User from '$lib/remotes/user.remote';
	import { cn } from '$lib/utils.js';
	import { Code, ExternalLink, Globe, MessageCircle } from '@lucide/svelte';
	import type { PageProps } from './$types';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { PUBLIC_S3_ENDPOINT } from '$env/static/public';

	const { params }: PageProps = $props();
	const user = $derived(await User.get(params.userId));
	const avatar = $derived(`${PUBLIC_S3_ENDPOINT}/avatars/${user.id}`);
	const socials = $derived([
		{ label: 'Website', url: user.details?.websiteUrl, icon: Globe },
		{ label: 'LinkedIn', url: user.details?.linkedinUrl, icon: Globe },
		{ label: 'Reddit', url: user.details?.redditUrl, icon: MessageCircle },
		{ label: 'GitHub', url: user.details?.githubUrl, icon: Globe }
	]);
</script>

<Avatar.Root class="absolute top-4 left-4 size-32 rounded shadow-xl">
	<Avatar.Image src={avatar} alt="@evilrabbit" class="block" />
	<Avatar.Fallback class="rounded text-2xl font-bold">
		{user.login.slice(0, 2).toUpperCase()}
	</Avatar.Fallback>
</Avatar.Root>

<!-- {#snippet badge(role: string)}
	{@const bg: Record<string, string> = {
		developer: "bg-[url('/dev.gif')]",
		staff: "bg-red-700"
	}}

	{#if bg[role]}
		<Badge
			variant="outline"
			class={cn('flex items-center gap-1.5 border-black px-3 py-1.5 text-white', bg[role])}
		>
			<Code class="size-4" fill="transparent" />
			<span class="capitalize">{role}</span>
		</Badge>
	{/if}
{/snippet} -->

<!-- <div class="container mx-auto max-w-5xl gap-3 py-8">
	<Card.Root class="h-min p-0 shadow-sm">
		<div class="relative h-48 rounded-t-[inherit] bg-linear-to-r from-indigo-500 via-purple-500 to-pink-500">
			<div
				class="absolute inset-0 rounded-t-[inherit] bg-[url('/graph.png')] bg-cover bg-center opacity-20"
			></div>

			<Avatar.Root class="absolute top-4 left-4 size-32 rounded shadow-xl">
				<Avatar.Image src={avatar} alt="@evilrabbit" class="block"/>
				<Avatar.Fallback class="rounded text-2xl font-bold" >
					{user.login.slice(0, 2).toUpperCase()}
				</Avatar.Fallback>
			</Avatar.Root>
		</div>

		<div class="relative px-6">
			<div class="py-6">
				<div class="flex items-start gap-2">
					<div>
						<h1 class="text-3xl font-bold tracking-tight">{user.displayName}</h1>
						<p class="text-muted-foreground">@{user.login}</p>
					</div>
					{#each page.data.session!.roles as role}
						{@render badge(role)}
					{/each}
				</div>
				<Separator class="my-4" />
				<div class="flex flex-wrap gap-3">
					{#each socials.filter((s) => s.url) as social}
						{@const Icon = social.icon}
						<Badge
							href={social.url}
							target="_blank"
							rel="noopener noreferrer"
							class="gap-1 transition-shadow hover:shadow"
							variant="outline"
						>
							<Icon class="size-3.5"/>
							{social.label}
							<ExternalLink class="size-3" />
						</Badge>
					{/each}
				</div>
			</div>
		</div>
	</Card.Root>

	<div class="grid grid-cols-1 gap-3 pt-3 md:grid-cols-[250px_1fr]">
		<Card.Root class="h-min p-4 shadow-sm ">
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
		<Card.Root class="shadow-sm">
			{#if user.details?.markdown}
				<Card.Content class="markdown max-h-125 overflow-auto">
					<Markdown  value={user.details.markdown} />
				</Card.Content>
			{:else}
			<Card.Content class="p-8 text-center text-muted-foreground">
				This user hasn't added a bio yet.
			</Card.Content>
			{/if}
		</Card.Root>
	</div>
</div> -->
