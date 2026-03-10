<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { ClipboardCheck, Clock, Loader, UserCheck, ArrowRight } from '@lucide/svelte';
</script>

<div class="mx-auto max-w-2xl p-6">
	<h1 class="mb-4 text-2xl font-bold">Reviews</h1>
	<Card.Root class="shadow-none">
		<Card.Content class="p-6">
			<p class="text-sm text-muted-foreground">
				Select a project to view its reviews, or navigate to a specific review from your project page.
			</p>
		</Card.Content>
	</Card.Root>
</div>
