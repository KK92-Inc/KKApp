<script lang="ts">
	import {
		Bell,
		CircleHelp,
		Cog,
		Ellipsis,
		LogOut,
		Moon,
		Rocket,
		Sun
	} from '@lucide/svelte';
	import * as Avatar from '$lib/components/avatar/';
	import type { LayoutProps } from './$types';
	import Button from '$lib/components/button/button.svelte';
	import * as ButtonGroup from '$lib/components/button-group/';
	import * as DropdownMenu from '$lib/components/dropdown-menu/';
	import { logout } from '../auth/auth.remote';
	import { page } from '$app/state';
	import { afterNavigate } from '$app/navigation';
	import Sidebar from '$lib/components/sidebar.svelte';
	import Favicon from '$lib/assets/hive.svg?raw';
	import { theme, toggleMode } from 'mode-watcher';
	import Search from '$lib/components/search.svelte';

	let open = $state(false);
	let { children, data }: LayoutProps = $props();

	afterNavigate(() => (open = false));
</script>

<div class="relative z-10 flex min-h-svh flex-col bg-background">
	<header class="bg-background sticky top-0 z-50 w-full border-b px-5">
		<nav class="flex h-(--header-height) items-center **:data-[slot=separator]:h-4! container mx-auto">
			<!-- Left: Menu button + Logo + App name -->
			<div class="flex items-center gap-3">
				<Sidebar bind:open />
				<Button
					href="/"
					variant="ghost"
					class="text-lg leading-none font-semibold [&>svg]:size-20!"
				>
					{@html Favicon}
				</Button>
			</div>

			<div class="flex items-center gap-2 ml-auto">
				<Search />
				<!-- <Button variant="outline" size="icon" aria-label="Notifications">
				<Plus />
			</Button> -->
				<Button onclick={toggleMode} variant="outline" size="icon" aria-label="Notifications">
					{#if theme.current === 'dark'}
						<Moon />
					{:else}
						<Sun />
					{/if}
				</Button>
				<ButtonGroup.Root>
					<Button variant="outline" href="/users/{page.data.session.userId}">
						<Avatar.Root class="size-6">
							<Avatar.Image src="https://github.com/w2wizard.png" alt="@evilrabbit" />
							<Avatar.Fallback>ER</Avatar.Fallback>
						</Avatar.Root>
						Account
					</Button>
					<DropdownMenu.Root>
						<DropdownMenu.Trigger>
							{#snippet child({ props })}
								<Button variant="outline" size="icon" aria-label="More Options" {...props}>
									<Ellipsis />
								</Button>
							{/snippet}
						</DropdownMenu.Trigger>
						<DropdownMenu.Content align="end" class="w-52">
							<DropdownMenu.Group>
								<DropdownMenu.Item href="/settings">
									<Cog />
									Settings
								</DropdownMenu.Item>
								<DropdownMenu.Item href="/notifications">
									<Bell />
									Notifications
								</DropdownMenu.Item>
							</DropdownMenu.Group>
							<DropdownMenu.Separator />
							<DropdownMenu.Group>
								<DropdownMenu.Item href="/">
									<CircleHelp />
									FAQ
								</DropdownMenu.Item>
								{#if data.session.roles.includes('staff')}
									<DropdownMenu.Item href="/admin">
										<Rocket />
										Dashboard
									</DropdownMenu.Item>
								{/if}
							</DropdownMenu.Group>
							<DropdownMenu.Separator />
							<DropdownMenu.Group>
								<DropdownMenu.Item remote={logout} variant="destructive">
									<LogOut />
									Logout
								</DropdownMenu.Item>
							</DropdownMenu.Group>
						</DropdownMenu.Content>
					</DropdownMenu.Root>
				</ButtonGroup.Root>
			</div>
		</nav>
	</header>

	<main class="flex flex-1 flex-col">
		{@render children?.()}
	</main>
</div>
