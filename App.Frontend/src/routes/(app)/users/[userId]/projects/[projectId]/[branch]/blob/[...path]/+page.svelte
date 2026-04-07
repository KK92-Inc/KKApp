<script lang="ts">
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const blob = $derived(
		await Git.blobViaUser({
			id: params.userId,
			projectId: params.projectId,
			branch: params.branch,
			path: params.path
		})
	);

	const decoded = $derived(new TextDecoder().decode(Uint8Array.from(atob(blob), (c) => c.charCodeAt(0))));
</script>

<pre><code>{decoded}</code></pre>
