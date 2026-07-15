<script lang="ts">
	import * as Alert from '$lib/components/alert';
	import { CircleAlert } from '@lucide/svelte';
</script>

<Alert.Root variant="destructive">
	<CircleAlert />
	<Alert.Title>Not Implemented</Alert.Title>
	<Alert.Description>
		<p>Creating custom apps is currently not yet finished, try again later.</p>
	</Alert.Description>
</Alert.Root>
