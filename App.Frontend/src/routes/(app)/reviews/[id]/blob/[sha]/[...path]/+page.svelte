<script lang="ts">
	import * as Page from '../../../context.svelte';
	import * as Remote from '../../../page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';
	import { ExplorerFile } from '$lib/components/explorer';

	const context = Page.getContext();
	const data = $derived(await Remote.getData(context.reviewId()));

	const { params }: PageProps = $props();
</script>

<svelte:boundary>
	{@const content = await Git.getBlob({
		id: data.review.userProject.gitInfoId,
		branch: data.review.ref,
		path: params.path
	})}

	<ExplorerFile name={params.path.split('/').pop() ?? ''} {content} />
</svelte:boundary>
