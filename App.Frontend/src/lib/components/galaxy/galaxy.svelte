<script lang="ts">
	import type { components } from "$lib/api/api";
	import Galaxy, { init } from "./context.svelte";

	interface Props {
		data: components["schemas"]["CursusTrackDTO"];
	}

	const { data }: Props = $props();
	const ctx = init(new Galaxy(data));
</script>

<div class="flex h-screen w-full flex-col bg-gradient-to-tr to-blue-950/10">
	<div class="relative flex-grow">
		<svg {@attach ctx.attachment()} class="h-full w-full"></svg>
	</div>
</div>
