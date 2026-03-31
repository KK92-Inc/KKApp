<script lang="ts">
	import type { LayoutProps } from "./$types";
	import * as Page from "./index.svelte";

	const { children, params }: LayoutProps = $props();
	Page.setContext(new Page.Context(() => params.id));
</script>

{@render children()}
