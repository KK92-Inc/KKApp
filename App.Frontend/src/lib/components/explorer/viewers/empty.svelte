<script lang="ts">
	import { FileQuestionMark } from '@lucide/svelte';
	import * as Empty from '$lib/components/empty';
</script>

<Empty.Root>
	<Empty.Header>
		<Empty.Media variant="icon">
			<FileQuestionMark />
		</Empty.Media>
		<Empty.Title>No data</Empty.Title>
		<Empty.Description>This file is empty.</Empty.Description>
	</Empty.Header>
</Empty.Root>
