<script lang="ts">
	import { Rocket, RotateCcw, X } from '@lucide/svelte';
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import { getSpotlights } from '$lib/remotes/spotlight.remote';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Empty from '$lib/components/empty';
	import type { HttpError } from '@sveltejs/kit';
</script>

<svelte:boundary>
	{@const spotlights = await getSpotlights()}
	{@const spotlight = spotlights.at(0)!}

	{#snippet pending()}
		<Skeleton class="h-80" />
	{/snippet}

	{#snippet failed(error, reset)}
		{@const e = error as HttpError}
		<Empty.Root class="bg-card h-80">
			<Empty.Header>
				<Empty.Media variant="icon">
					<X />
				</Empty.Media>
				<Empty.Title>{e.body.message}</Empty.Title>
			</Empty.Header>
			<Empty.Content>
				<Button onclick={reset}>
					<RotateCcw />
					Try Again
				</Button>
			</Empty.Content>
		</Empty.Root>
	{/snippet}

	<Card.Root class="group relative h-80 overflow-hidden border bg-black text-white">
		<div class="absolute inset-0 z-0">
			<img
				src="https://images.unsplash.com/photo-1556761175-5973dc0f32e7?q=80&w=2832&auto=format&fit=crop"
				alt="PeerU 2025"
				class="h-full w-full object-cover opacity-80 transition-transform duration-700 motion-safe:group-hover:scale-110"
			/>
			<div class="absolute inset-0 bg-linear-to-t from-black via-black/50 to-transparent"></div>
		</div>

		<div class="relative z-10 flex h-full flex-col justify-end p-6">
			<div class="space-y-1">
				<h2
					class="bg-linear-to-br from-white via-white to-white/60 bg-clip-text font-mono text-4xl font-black tracking-tighter text-transparent drop-shadow-sm"
				>
					{spotlight.title}
				</h2>
				<p class="text-sm font-medium text-gray-300">{spotlight.description}</p>
			</div>

			<div class="mt-6">
				<Button
					class="w-full bg-white font-bold text-black transition-all hover:bg-white/90 motion-safe:hover:-translate-y-0.5"
				>
					<Rocket class="mr-2 h-4 w-4" /> Register Now
				</Button>
			</div>
		</div>
	</Card.Root>
</svelte:boundary>
