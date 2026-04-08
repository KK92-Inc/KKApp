<script lang="ts">
	import * as Page from './index.svelte.ts';
	import * as Field from '$lib/components/field';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';

	const context = Page.getContext();
</script>

<Field.Set>
	<Field.Legend>Project Markdown</Field.Legend>
	<Field.Description>
		Write your project curriculum and instructions using Markdown.
	</Field.Description>
	<Field.Field>
		<MarkdownTextarea bind:value={context.markdown} />
	</Field.Field>
</Field.Set>
