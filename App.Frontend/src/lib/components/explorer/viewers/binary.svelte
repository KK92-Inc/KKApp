<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Empty from '$lib/components/empty';
	import { Download, FileExclamationPoint } from '@lucide/svelte';

	const { download }: { download: () => void } = $props();
</script>

<Empty.Root>
	<Empty.Header>
		<Empty.Media variant="icon">
			<FileExclamationPoint />
		</Empty.Media>
		<Empty.Title>Binary File</Empty.Title>
		<Empty.Description>
			This is a binary file and thus it is not possible at this time to view it.
		</Empty.Description>
	</Empty.Header>
	<Empty.Content>
		<Button variant="outline" aria-label="Download" onclick={download}>
			<Download class="size-4" />
			Download File
		</Button>
	</Empty.Content>
</Empty.Root>
