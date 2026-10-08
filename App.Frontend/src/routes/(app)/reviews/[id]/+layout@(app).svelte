<script lang="ts">
	import type { LayoutProps } from './$types';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';

	const { params, children }: LayoutProps = $props();

	Page.setContext(new Page.Context(() => params.id));
	const [review, round] = $derived(await Remote.getData(params.id));
</script>

<pre><code>{JSON.stringify(review, null, 2)}</code></pre>

---

<pre><code>{JSON.stringify(round, null, 2)}</code></pre>
{@render children()}
