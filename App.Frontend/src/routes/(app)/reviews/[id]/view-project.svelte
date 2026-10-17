<script lang="ts">
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { CircleAlert } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	const context = Page.getContext();
	const { review } = $derived(await Remote.load(context.reviewId()));
</script>

<svelte:boundary>
	{@const root = await Git.getBlob({
		id: review.userProject.projectGitInfoId,
		ref: review.ref,
		path: 'README.md'
	})}


	{#snippet failed(error)}
		<Alert.Root variant="destructive">
			<CircleAlert />
			<Alert.Title>Failed to find subject</Alert.Title>
			<Alert.Description>
				Something went wrong. If you believe this is an error let staff know.
				<details>
					<summary>Technical Details</summary>
					<pre><code>{JSON.stringify(error)}</code></pre>
				</details>
			</Alert.Description>
		</Alert.Root>
	{/snippet}

	{#snippet pending()}
		<Skeleton class="w-full h-20"/>
	{/snippet}

	<Markdown value={new TextDecoder('utf-8', { fatal: true }).decode(root)} />
</svelte:boundary>
