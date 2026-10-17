<script lang="ts">
	interface Props {
		content: ArrayBuffer;
		name: string;
		mime: string;
	}

	const { content, name, mime }: Props = $props();

	let url = $state<string>();

	// Object URLs are a side effect, so they live in an effect with cleanup.
	$effect(() => {
		const objectUrl = URL.createObjectURL(new Blob([content], { type: mime }));
		url = objectUrl;
		return () => URL.revokeObjectURL(objectUrl);
	});
</script>

<div class="flex min-h-64 items-center justify-center bg-muted/20 p-8">
	{#if url}
		<img
			src={url}
			alt={name}
			class="max-h-96 max-w-full rounded-md border bg-background object-contain shadow-sm"
		/>
	{/if}
</div>
