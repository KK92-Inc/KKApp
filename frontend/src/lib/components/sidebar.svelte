<script lang="ts">
	import * as Sheet from '$lib/components/sheet';
	import { buttonVariants } from '$lib/components/button';
	import { today } from '@internationalized/date';
	import { page } from '$app/state';
	import {
		Archive,
		Github,
		GraduationCap,
		HeartHandshake,
		House,
		Menu,
		MessageSquare,
		Rss,
		Sparkle,
		Trophy,
		Users
	} from '@lucide/svelte';
	import Button from './button/button.svelte';
	import Favicon from '$lib/assets/hive.svg?raw';
	import Navgroup from './navgroup.svelte';

	let { open = $bindable(false) } = $props();
</script>

<Sheet.Root bind:open>
	<Sheet.Trigger class={buttonVariants({ variant: 'outline', size: 'icon' })}>
		<Menu />
	</Sheet.Trigger>
	<Sheet.Content dir="rtl" side="left" class="overflow-y-auto rounded-tr-xl rounded-br-xl">
		<Sheet.Header dir="ltr">
			<Sheet.Title class="[&>svg]:w-20!">
				{@html Favicon}
			</Sheet.Title>
		</Sheet.Header>
		<div dir="ltr" class="grid flex-1 auto-rows-min gap-y-4 px-4">
			<Navgroup
				title="Personal"
				args={{
					userId: page.data.session.userId,
				}}
				routes={[
					'/(app)/users/[userId]/projects',
					'/(app)/users/[userId]/goals',
					'/(app)/users/[userId]/cursus',
					'/(app)/users/[userId]/galaxy',
				]}
			/>
			<Navgroup
				title="Community"
				routes={[
					'/(app)/users',
					'/(app)/reviews',
				]}
			/>
		</div>
		<!-- <div class="grid flex-1 auto-rows-min gap-6 px-4">
			<div class="grid gap-3">
				<Label for="name" class="text-right">Name</Label>
				<Input id="name" value="Pedro Duarte" />
			</div>
			<div class="grid gap-3">
				<Label for="username" class="text-right">Username</Label>
				<Input id="username" value="@peduarte" />
			</div>
		</div> -->
		<Sheet.Footer dir="ltr" class="flex justify-between gap-1 border-t px-4 py-3">
			<div class="flex items-center gap-3 text-sm text-muted-foreground">
				<span>© {today(page.data.tz).year} W2 B.V.</span>
				<span aria-hidden="true">•</span>
				<nav>
					<Button href="#" size="icon" variant="ghost"><Github /></Button>
					<Button href="#" size="icon" variant="ghost"><Rss /></Button>
				</nav>
			</div>
			<div class="hidden items-center text-sm text-muted-foreground md:flex">
				<span>Made with <span aria-hidden="true">❤️</span> by the open-source community</span>
			</div>
		</Sheet.Footer>
	</Sheet.Content>
</Sheet.Root>
