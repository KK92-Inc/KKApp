<script>
	import { Moon, Sun, LogIn } from '@lucide/svelte';
	import { toggleMode } from 'mode-watcher';
	import { Button } from '$lib/components/button';
	import { login } from './auth.remote';
	import { fade } from 'svelte/transition';
	import WhiteLabel from '$lib/components/white-label.svelte';
</script>

<div class="grid min-h-dvh grid-cols-[var(--padding)_1px_1fr_1px_var(--padding)] grid-rows-[var(--padding)_1px_1fr_1px_var(--padding)] [--padding:--spacing(4)] sm:[--padding:--spacing(10)]">
	<div class="relative col-span-5 col-start-1 row-start-2 bg-black/10 before:absolute before:inset-x-0 before:-inset-y-2 before:content-[''] dark:bg-white/10"></div>
	<div class="relative col-span-5 col-start-1 row-start-4 bg-black/10 before:absolute before:inset-x-0 before:-inset-y-2 before:content-[''] dark:bg-white/10"></div>
	<div class="relative col-start-2 row-span-5 row-start-1 bg-black/10 before:absolute before:-inset-x-2 before:inset-y-0 before:content-[''] dark:bg-white/10"></div>
	<div class="relative col-start-4 row-span-5 row-start-1 bg-black/10 before:absolute before:-inset-x-2 before:inset-y-0 before:content-[''] dark:bg-white/10"></div>
	<div class="relative rounded -right-1 -bottom-1 col-start-1 row-start-1 size-1.75 place-self-end border border-black/10 bg-white dark:border-white/10 dark:bg-neutral-950"></div>
	<div class="relative rounded -bottom-1 -left-1 col-start-5 row-start-1 size-1.75 self-end justify-self-start border border-black/10 bg-white dark:border-white/10 dark:bg-neutral-950"></div>
	<div class="relative rounded -top-1 -right-1 col-start-1 row-start-5 size-1.75 self-start justify-self-end border border-black/10 bg-white dark:border-white/10 dark:bg-neutral-950"></div>
	<div class="relative rounded -top-1 -left-1 col-start-5 row-start-5 size-1.75 place-self-start border border-black/10 bg-white dark:border-white/10 dark:bg-neutral-950"></div>
	<div class="col-start-3 row-start-3 flex flex-col gap-16 p-4 sm:p-10">
		<div class="flex items-center justify-between">
			<WhiteLabel />
		</div>
		<main class="flex-1">
			<div class="grid h-full content-end">
				<h1 class="max-w-7xl text-[min(8.5vw,128px)]/[1] font-[450] tracking-tighter text-black dark:text-white" in:fade={{ delay: 0 }}>
					Be your own Future
				</h1>
				<p class="mt-7 max-w-2xl text-lg/8 text-neutral-700 sm:text-xl/9 md:text-2xl/10 dark:text-neutral-400" in:fade={{ delay: 0 }}>
					Continue your project-based learning experience.
				</p>
				<div class="mt-10 flex flex-col gap-10 md:flex-row md:items-end md:justify-between">
					<form {...login} class="flex gap-4">
						<Button onclick={toggleMode} variant="outline" size="icon" class="size-12">
							<Sun class="dark:scale-0 dark:-rotate-90" />
							<Moon class="absolute scale-0 dark:scale-100 dark:rotate-0" />
							<span class="sr-only">Toggle theme</span>
						</Button>
						<Button type="submit" class="flex h-12 w-36 items-center gap-2">
							Signin <LogIn class="size-4" />
						</Button>
					</form>
				</div>
			</div>
		</main>
	</div>
</div>
