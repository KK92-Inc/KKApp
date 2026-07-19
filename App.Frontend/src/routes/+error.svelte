<script>
	import { page } from '$app/state';
	import { AlertTriangle, House, RefreshCw, TriangleAlert } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
</script>

<div class="flex min-h-screen items-center justify-center p-4">
	<div class="w-full max-w-md overflow-hidden rounded-xl border bg-card/90 shadow-xl backdrop-blur">
		<div class="overflow-hidden">
			<img class="w-full" src="https://http.cat/{page.status}" alt="Error {page.status}" />
		</div>

		<div class="space-y-6 p-6">
			<div class="space-y-2">
				<h2 class="text-2xl font-bold">Houston, we have a problem</h2>
				{#if page.error}
					<p class="flex animate-pulse items-center gap-1 text-destructive">
						<TriangleAlert size={16} />
						{page.error.message}
					</p>
				{/if}
				<p class="text-muted-foreground">
					{#if page.status >= 500}
						<span class="font-medium">Silly cats!</span> Rest assured this issue has been logged & reported.
					{/if}
				</p>
			</div>

			<div class="flex gap-3">
				<Button variant="default" class="group flex-1" href="/">
					<House class="group-hover:shake mr-2 h-4 w-4 transition-transform" />
					Back to safety
				</Button>

				<!-- <Button variant="outline" class="group" href="/">
					<RefreshCw class="h-4 w-4 group-hover:rotate-180 transition-transform duration-500" />
				</Button> -->
			</div>
		</div>
	</div>
</div>
