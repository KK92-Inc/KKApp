<script lang="ts">
	import type { LayoutProps, PageProps } from './$types';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';

	// const { params, children }: PageProps = $props();

	const context = Page.getContext();
	const [review, round] = $derived(await Remote.getData(context.reviewId()));
</script>

<h1 class="my-12  text-4xl">!!!PAGE DATA!!!</h1>

<pre><code>{JSON.stringify(review, null, 2)}</code></pre>

---

<pre><code>{JSON.stringify(round, null, 2)}</code></pre>
