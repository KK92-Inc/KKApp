<script lang="ts">
	import type { TreeDTO } from '.';
	import ExplorerNodeTree from './explorer-node-tree.svelte';

	interface Props {
		nodes: TreeDTO[];
		baseUrl: string;
		branch: string;
		dotdotHref?: string;
	}

	const props: Props = $props();
</script>

<ExplorerNodeTree {...props} />
