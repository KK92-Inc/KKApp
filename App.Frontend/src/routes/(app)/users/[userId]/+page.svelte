<script lang="ts">
	import { page } from '$app/state';
	import * as Avatar from '$lib/components/avatar/index.js';
	import { Badge } from '$lib/components/badge/index.js';
	import * as Card from '$lib/components/card/index.js';
	import Navgroup from '$lib/components/navgroup.svelte';
	import { Separator } from '$lib/components/separator/index.js';
	import { cn } from '$lib/utils.js';
	import { Code } from '@lucide/svelte';
	// import Markdown from "svelte-exmarkdown";

	const { data } = $props();
	// const id = page.params.id;
	// const links = $state.raw([
	// 	{
	// 		icon: Archive,
	// 		href: `./${id}/projects`,
	// 		title: "Projects",
	// 	},
	// 	{
	// 		icon: Trophy,
	// 		href: `./${id}/goals`,
	// 		title: "Goals",
	// 	},
	// 	{
	// 		icon: GraduationCap,
	// 		href: `./${id}/cursus`,
	// 		title: "Cursus",
	// 	},
	// 	{
	// 		icon: Sparkles,
	// 		href: `./${id}/galaxy`,
	// 		title: "Galaxy",
	// 	},
	// ]);

	// const socials = [
	// 	{ label: "Website", url: data.user.details?.websiteUrl, icon: Globe },
	// 	{ label: "LinkedIn", url: data.user.details?.linkedinUrl, icon: Linkedin },
	// 	{ label: "Reddit", url: data.user.details?.redditUrl, icon: MessageCircle },
	// 	{ label: "GitHub", url: data.user.details?.githubUrl, icon: Github },
	// ].filter((s) => s.url);

	// let md = $state(data.bio?.toString() ?? "");

	// async function openLink(href: string, warn: boolean) {
	// 	if (warn) {
	// 		const confirm = await dialog.confirm({
	// 			title: "Navigate to URL?",
	// 			message: `Will navigate you to: ${href}`,
	// 		});

	// 		if (!confirm) return;
	// 	}
	// 	window.location.assign(href);
	// }
</script>

{#snippet badge(role: string)}
	<!-- Hard coded special roles... -->
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
{/snippet}

<div class="container mx-auto max-w-5xl gap-3 py-8">
	<Card.Root class="h-min p-0 shadow-sm">
		<div
			class="relative h-48 rounded-t-[inherit] bg-linear-to-r from-indigo-500 via-purple-500 to-pink-500"
		>
			<div
				class="absolute inset-0 rounded-t-[inherit] bg-[url('/graph.png')] bg-cover bg-center opacity-20"
			></div>
			<Avatar.Root class="absolute top-4 left-4 size-32 rounded shadow-xl">
				<Avatar.Image src="" />
				<Avatar.Fallback class="rounded text-2xl font-bold">W2</Avatar.Fallback>
			</Avatar.Root>
		</div>

		<div class="relative px-6">
			<div class="py-6">
				<div class="flex items-start gap-2">
					<div>
						<h1 class="text-3xl font-bold tracking-tight">W2Wizard</h1>
						<p class="text-muted-foreground">@w2wizard</p>
					</div>
					{#each page.data.session!.roles as role}
						{@render badge(role)}
					{/each}
				</div>
				<Separator class="my-4" />
				<!-- {#if socials.length > 0}
					<div class="flex flex-wrap gap-3">
						{#each socials as social}
							<Badge
								href={social.url}
								target="_blank"
								rel="noopener noreferrer"
								class="gap-1 transition-shadow hover:shadow"
								variant="outline"
								onclick={(e) => {
									e.preventDefault();
									openLink(e.currentTarget.href, social.label === "Website");
								}}
							>
								<svelte:component this={social.icon} class="size-3.5" />
								{social.label}
								<ExternalLink class="size-3" />
							</Badge>
						{/each}
					</div>
				{/if} -->
			</div>
		</div>
	</Card.Root>

	<div class="grid grid-cols-1 gap-3 pt-3 md:grid-cols-[250px_1fr]">
		<Card.Root class="h-min p-4 shadow-sm ">
			<Navgroup
				title="Navigation"
				args={{ userId: page.params.userId }}
				routes={[
					'/(app)/users/[userId]/cursus',
					'/(app)/users/[userId]/projects',
					'/(app)/users/[userId]/goals',
					'/(app)/users/[userId]/galaxy'
				]}
			/>
		</Card.Root>
		<Card.Root class="shadow-sm">
			<!-- {#if data.bio}
				<Card.Content class="markdown max-h-[500px] overflow-auto">
					<Markdown variant="viewer" value={md} />
				</Card.Content>
			{:else} -->
			<Card.Content class="p-8 text-center text-muted-foreground">
				This user hasn't added a bio yet.
			</Card.Content>
			<!-- {/if} -->
		</Card.Root>
	</div>
</div>
