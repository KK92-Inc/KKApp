<script>
	import { fade, fly } from 'svelte/transition';
	import { Moon, Sun, LogIn } from '@lucide/svelte';
	import { toggleMode } from 'mode-watcher';
	import { Button } from '$lib/components/button';
	import { login } from './auth.remote';
</script>

<div class="relative min-h-screen overflow-hidden bg-background">
	<!-- Background decorative blobs -->
	<div class="absolute -top-40 -right-40 h-96 w-96 rounded-full bg-primary/10 blur-3xl dark:bg-primary/5"></div>
	<div class="absolute top-1/3 -left-20 h-72 w-72 rounded-full bg-accent/10 blur-3xl dark:bg-accent/5"></div>
	<div class="absolute right-10 bottom-20 h-60 w-60 rounded-full bg-secondary/10 blur-3xl dark:bg-secondary/5"></div>

	<!-- Floating decorative elements -->
	<div class="animate-float-slow absolute top-20 right-10 hidden h-24 w-24 rotate-12 rounded-xl bg-linear-to-br from-primary/20 to-primary/10 backdrop-blur-sm md:block dark:from-primary/10 dark:to-primary/5"></div>
	<div class="animate-float-slow absolute bottom-20 left-10 hidden h-16 w-16 -rotate-12 rounded-lg bg-linear-to-br from-primary/20 to-primary/10 backdrop-blur-sm md:block dark:from-primary/10 dark:to-primary/5"></div>

	<section class="relative flex min-h-[calc(100vh-80px)] items-center py-24">
		<div class="container mx-auto px-4 md:px-6">
			<div class="grid items-center gap-12 md:grid-cols-2">
				<!-- Left column: Text and Login -->
				<div class="flex flex-col space-y-6" in:fly={{ y: 50, duration: 700 }}>
					<span class="inline-block self-start rounded-full bg-primary/10 px-4 py-1.5 text-sm font-medium text-primary dark:bg-primary/20 dark:text-primary-foreground">
						Free & Open Source Education
					</span>
					<h1 class="text-4xl font-bold leading-tight text-foreground md:text-5xl lg:text-6xl">
						Access Your
						<span class="block bg-linear-to-r from-primary to-accent bg-clip-text text-transparent">
							Learning Potential
						</span>
					</h1>
					<p class="max-w-xl text-lg text-muted-foreground md:text-xl">
						Continue your project-based learning experience.
					</p>
					<form {...login}>
						<Button type="submit" class="flex items-center gap-2">
							Signin <LogIn class="h-4 w-4" />
						</Button>
					</form>
				</div>

				<!-- Right column: Simplified UI Mockup -->
				<div class="relative flex justify-center" in:fade={{ delay: 1200, duration: 250 }}>
					<div class="animate-mock relative w-full max-w-md rounded-2xl border bg-card p-1 shadow-xl">
						<div class="flex aspect-4/3 flex-col rounded-xl bg-linear-to-br from-muted/30 to-background/30 dark:from-background/10 dark:to-muted/10">
							<!-- Mock Header -->
							<div class="flex items-center gap-2 border-b bg-background p-4 dark:bg-card">
								<div class="flex gap-1.5">
									<div class="h-3 w-3 rounded-full bg-destructive"></div>
									<div class="h-3 w-3 rounded-full bg-amber-400"></div>
									<div class="h-3 w-3 rounded-full bg-emerald-400"></div>
								</div>
								<div class="ml-auto h-4 w-32 rounded-full bg-muted"></div>
							</div>
							<!-- Mock Content -->
							<div class="flex-1 p-6">
								<div class="h-10 w-1/2 rounded-lg bg-primary/20 dark:bg-primary/10"></div>
								<div class="mt-4 space-y-2">
									<div class="h-6 w-3/4 rounded-lg bg-muted"></div>
									<div class="h-6 w-2/3 rounded-lg bg-muted"></div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</section>

	<!-- Theme Toggle Button -->
	<Button onclick={toggleMode} variant="outline" size="icon" class="absolute top-4 left-4">
		<Sun class="h-[1.2rem] w-[1.2rem] transition-all dark:scale-0 dark:-rotate-90" />
		<Moon class="absolute h-[1.2rem] w-[1.2rem] scale-0 transition-all dark:scale-100 dark:rotate-0" />
		<span class="sr-only">Toggle theme</span>
	</Button>
</div>

<style>
	@keyframes float {
		0%, 100% { transform: translateY(0) rotate(-12deg); }
		50% { transform: translateY(-10px) rotate(-8deg); }
	}
	@keyframes float-slow {
		0%, 100% { transform: translateY(0) rotate(12deg); }
		50% { transform: translateY(-15px) rotate(16deg); }
	}
	@keyframes float-mock {
		0%, 100% { transform: translateY(0) rotate(-1.5deg); }
		50% { transform: translateY(-10px) rotate(1.5deg); }
	}
	:global(.animate-float) {
		@media (prefers-reduced-motion: no-preference) {
			animation: float 4s ease-in-out infinite;
		}
	}
	:global(.animate-mock) {
		animation-delay: 250ms;
		@media (prefers-reduced-motion: no-preference) {
			animation: float-mock 10s ease-in-out infinite;
		}
	}
	:global(.animate-float-slow) {
		@media (prefers-reduced-motion: no-preference) {
			animation: float-slow 6s ease-in-out infinite;
		}
	}
</style>
