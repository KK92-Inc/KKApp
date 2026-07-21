<script lang="ts">
	import type { LayoutProps } from './$types';
	import * as Page from './index.svelte';
	let { children, params }: LayoutProps = $props();
	Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.projectId
		)
	);

</script>

{@render children?.()}
