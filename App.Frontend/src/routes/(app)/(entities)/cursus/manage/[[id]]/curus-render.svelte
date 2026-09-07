<script lang="ts">
	import type { GalaxyNode } from "$lib/components/galaxy/types";


	const render = (tree: GalaxyNode<TrackNode>): Attachment<SVGElement> => {
		return (element) => renderer.mount(element, tree);
	};
</script>
