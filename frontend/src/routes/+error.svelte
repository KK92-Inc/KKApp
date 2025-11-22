<script>
	import { page } from "$app/state";
	import { House, RefreshCw } from "@lucide/svelte";
	import { Button } from "$lib/components/button";
</script>

<div class="min-h-screen flex items-center justify-center p-4">
	<div class="w-full max-w-md bg-card/90 backdrop-blur rounded-xl shadow-xl border overflow-hidden">
		<div class="overflow-hidden">
			<img
				class="w-full"
				src="https://http.cat/{page.status}"
				alt="Error {page.status}"
			/>
		</div>

		<div class="p-6 space-y-6">
			<div class="space-y-2">
				<h2 class="text-2xl font-bold">Houston, we have a problem</h2>
				<p class="text-muted-foreground">
					{#if page.status >= 500}
						<span class="font-medium">Silly cats!</span> Rest assured this issue has been logged & reported.
					{:else if page.status >= 400}
						{page.error?.message ?? "Oops! Something wasn't quite right with that request."}
					{/if}
				</p>
			</div>

			<div class="flex gap-3">
				<Button variant="default" class="flex-1 group" href="..">
					<House class="mr-2 h-4 w-4 group-hover:shake transition-transform" />
					Back to safety
				</Button>

				<Button variant="outline" class="group" href={page.url.pathname}>
					<RefreshCw class="h-4 w-4 group-hover:rotate-180 transition-transform duration-500" />
				</Button>
			</div>
		</div>
	</div>
</div>
