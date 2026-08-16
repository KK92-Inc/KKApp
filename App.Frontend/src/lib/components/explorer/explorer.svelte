<script lang="ts">
	import type { FileNode } from '.';
	import ExplorerNodeTree from './explorer-node-tree.svelte';

	interface Props {
		nodes: FileNode[];
		baseUrl: string;
		branch: string;
		dotdotHref?: string;
	}

	const props: Props = $props();
</script>

<ExplorerNodeTree {...props} />
