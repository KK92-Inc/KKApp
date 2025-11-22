<script lang="ts">
	import { onMount } from "svelte";
	import { generateSampleData } from "./d3utils.svelte";
	import Graph from "./graph.svelte";
	import Galaxy from "$lib/components/galaxy/galaxy.svelte";

	// onMount(() => {
	// 	document.body.style.overflow = "hidden";
	// 	return () => {
	// 		document.body.style.overflow = "";
	// 	};
	// });
</script>
<Galaxy data={{}}/>
	<!-- <Graph  data={generateSampleData()}/> -->
