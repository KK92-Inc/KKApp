<script lang="ts">
	import { Separator } from '$lib/components/separator';
	import { CircleAlert, TriangleAlert, } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
</script>

<div class="flex items-center justify-between gap-4 pb-2">
	<h1 class="text-xl font-bold">Feature Settings</h1>
	<Separator class="flex-1" />
</div>

<Alert.Root variant="destructive">
	<CircleAlert />
	<Alert.Title>Not Implemented</Alert.Title>
	<Alert.Description>
		<p>Preview features is currently not yet finished, try again later.</p>
	</Alert.Description>
</Alert.Root>

