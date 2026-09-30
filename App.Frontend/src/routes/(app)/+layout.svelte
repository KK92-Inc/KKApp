<script lang="ts">
	import type { LayoutProps } from './$types';
	import { afterNavigate, goto } from '$app/navigation';
	import * as Header from '$lib/components/header';
	import Button from '$lib/components/button/button.svelte';
	import WhiteLabel from '$lib/components/white-label.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { isHttpError } from '@sveltejs/kit';
	import { Calendar } from '@lucide/svelte';

	let open = $state(false);
	afterNavigate(() => (open = false));
	const { children }: LayoutProps = $props();
</script>

<svelte:window
	onunhandledrejection={async (e) => {
		// We're being told to GTFO, so let's leave.
		if (isHttpError(e.reason, 401)) {
			console.log("jIJ")
			e.preventDefault();
			await goto('/auth');
		}
		if (isHttpError(e.reason, 403)) {
			e.preventDefault();
			await goto('/');
		}
	}}
/>

<div class="relative z-10 flex min-h-svh flex-col bg-background">
	<header class="sticky top-0 z-50 w-full border-b bg-background px-5">
		<nav class="container mx-auto flex h-(--header-height) items-center **:data-[slot=separator]:h-6!">
			<div class="flex items-center gap-3">
				<Header.Sidebar bind:open />
				<Button href="/" variant="ghost" class="text-lg leading-none font-semibold [&>svg]:size-20!">
					<WhiteLabel />
				</Button>
			</div>

			<div class="ml-auto flex items-center gap-2">
				<!-- <Header.Search /> -->
				<Header.Theme />
				<Separator orientation="vertical" class="max-md:hidden" />
				<Button href="/events" variant="outline">
					<Calendar />
					<span class="max-md:hidden">Events</span>
				</Button>
				<Separator orientation="vertical" />
				<Header.Create />
				<Header.Dropdown />
			</div>
		</nav>
	</header>

	<main class="flex flex-1 flex-col">
		{@render children?.()}
	</main>
</div>
