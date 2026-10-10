<script lang="ts">
	import * as Empty from '$lib/components/empty';
	import { FileQuestionMark } from '@lucide/svelte';
</script>

<Empty.Root>
	<Empty.Header>
		<Empty.Media variant="icon">
			<FileQuestionMark />
		</Empty.Media>
		<Empty.Title>No data</Empty.Title>
		<Empty.Description>This file has no data to be displayed.</Empty.Description>
	</Empty.Header>
</Empty.Root>
