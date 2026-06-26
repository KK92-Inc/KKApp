<script lang="ts">
	import Galaxy, { init } from "./context.svelte";
	import { generateMockCursusData } from "./index";

	// Generate ~1000 nodes using our 4-child/depth-5 rule
	const mockData = generateMockCursusData();

	const ctx = init(new Galaxy(mockData));
</script>

<div class="flex h-screen w-full flex-col bg-slate-900">
    <div class="relative flex-grow overflow-hidden">
		<svg {@attach ctx.attachment()} class="h-full w-full cursor-grab active:cursor-grabbing"></svg>
	</div>
</div>
