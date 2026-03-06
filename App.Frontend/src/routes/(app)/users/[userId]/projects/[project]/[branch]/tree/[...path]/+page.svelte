<script lang="ts">
	import { parseGitTree } from '$lib/components/explorer';
	import FileBrowser from '$lib/components/explorer/explorer.svelte';
	import type { PageProps } from './$types';


	const { params, data }: PageProps = $props();
	const files = $derived(parseGitTree(data.tree));
</script>

<FileBrowser baseUrl="./src" dotdot nodes={files} />
