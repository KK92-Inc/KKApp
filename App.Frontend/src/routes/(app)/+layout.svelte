<script lang="ts">
	import type { LayoutProps } from './$types';
	import { afterNavigate } from '$app/navigation';
	import * as Header from "$lib/components/header";
	import Button from '$lib/components/button/button.svelte';
	import { EventSourceContext, init } from '$lib/contexts/events.svelte';

	let open = $state(false);
	let { children }: LayoutProps = $props();

	const events = init(new EventSourceContext("/proxy/events"));
	events.listen("DemoEvent", (data) => {
		console.log(data);
	});

	afterNavigate(() => (open = false));
</script>

<div class="relative z-10 flex min-h-svh flex-col bg-background">
	<header class="bg-background sticky top-0 z-50 w-full border-b px-5">
		<nav class="flex h-(--header-height) items-center **:data-[slot=separator]:h-4! container mx-auto">
			<div class="flex items-center gap-3">
				<Header.Sidebar bind:open />
				<Button
					href="/"
					variant="ghost"
					class="text-lg leading-none font-semibold"
				>
					Home
				</Button>
			</div>

			<div class="flex items-center gap-2 ml-auto">
				<Header.Search />
				<Header.Theme />
				<Header.Dropdown />
			</div>
		</nav>
	</header>

	<main class="flex flex-1 flex-col">
		{@render children?.()}
	</main>
</div>
