<script lang="ts">
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { Skeleton } from '$lib/components/skeleton';

	const context = Page.getContext();
	const { review } = $derived(await Remote.getData(context.reviewId()));
</script>

<svelte:boundary>
	{@const root = await Git.getBlob({
		id: review.rubric.gitInfoId,
		ref: review.ref,
		path: 'README.md'
	})}

	{#snippet pending()}
		<Skeleton class="w-full h-20"/>
	{/snippet}

	<Markdown value={new TextDecoder('utf-8', { fatal: true }).decode(root)} />
</svelte:boundary>
