<script lang="ts">
	import type { LayoutProps } from './$types';
	import { afterNavigate } from '$app/navigation';
	import * as Header from '$lib/components/header';
	import Button from '$lib/components/button/button.svelte';
	import { EventSourceContext, init } from '$lib/contexts/events.svelte';
	import WhiteLabel from '$lib/components/white-label.svelte';
	import { page } from '$app/state';
	import Separator from '$lib/components/separator/separator.svelte';

	let open = $state(false);
	let { children }: LayoutProps = $props();

	// const events = init(new EventSourceContext('/proxy/events'));
	// events.listen('DemoEvent', (data) => {
	// 	console.log(data);
	// });

	afterNavigate(() => (open = false));
</script>

<div class="relative z-10 flex min-h-svh flex-col bg-background">
	<header class="sticky top-0 z-50 w-full border-b bg-background px-5">
		<nav
			class="container mx-auto flex h-(--header-height) items-center **:data-[slot=separator]:h-6!"
		>
			<div class="flex items-center gap-3">
				<Header.Sidebar bind:open />
				<Button
					href="/"
					variant="ghost"
					class="text-lg leading-none font-semibold [&>svg]:size-20!"
				>
					<WhiteLabel />
				</Button>
			</div>

			<div class="ml-auto flex items-center gap-2">
				<Header.Search />
				<Header.Theme />
				<Separator orientation="vertical"/>
				<Header.Create />
				<Header.Dropdown />
			</div>
		</nav>
	</header>

	<main class="flex flex-1 flex-col">
		{@render children?.()}
	</main>
</div>
