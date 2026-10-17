<script lang="ts">
	import { Badge } from '$lib/components/badge';
	import { describe, type Freeze } from './state';

	let { freeze }: { freeze: Freeze } = $props();
	const status = $derived(describe(freeze));
</script>

<div class="flex items-center gap-1">
	<Badge variant={status.variant}>{status.label}</Badge>
	<span class="text-xs text-muted-foreground">{status.when}</span>
</div>
