<script lang="ts">
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const decoder = new TextDecoder();
	const blob = $derived(Git.blob({
		id: params.id,
		branch: params.branch,
		path: params.path
	}));

	// TODO: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint8Array/fromBase64
	const decoded = $derived(decoder.decode(Uint8Array.from(atob(await blob), (c) => c.charCodeAt(0))));
</script>

<pre><code>{decoded}</code></pre>
