<script lang="ts">
	import { Download, FileExclamationPoint } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Empty from '$lib/components/empty';

	const { download }: { download: () => void } = $props();
</script>

<Empty.Root>
	<Empty.Header>
		<Empty.Media variant="icon">
			<FileExclamationPoint />
		</Empty.Media>
		<Empty.Title>Binary file</Empty.Title>
		<Empty.Description>This file can't be previewed. Download it to open it.</Empty.Description>
	</Empty.Header>
	<Empty.Content>
		<Button variant="outline" onclick={download}>
			<Download class="size-4" />
			Download file
		</Button>
	</Empty.Content>
</Empty.Root>
