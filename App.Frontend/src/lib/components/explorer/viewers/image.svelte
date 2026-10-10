<script lang="ts">
	interface Props {
		content: ArrayBuffer;
		name: string;
	}

	const { content, name }: Props = $props();

	const url = $derived.by(() => {
		const ext = name.split('.').pop()?.toLowerCase() ?? 'png';
		const mime = ext === 'svg' ? 'image/svg+xml' : `image/${ext}`;
		const blob = new Blob([content], { type: mime });
		const url = URL.createObjectURL(blob);
		return url;
	});

	$effect(() => {
		return () => URL.revokeObjectURL(url);
	});
</script>

<div class="flex min-h-64 items-center justify-center p-8 bg-muted/20">
	<img
		src={url}
		alt={name}
		class="max-h-96 max-w-full rounded-md object-contain shadow-sm border bg-background"
	/>
</div>
