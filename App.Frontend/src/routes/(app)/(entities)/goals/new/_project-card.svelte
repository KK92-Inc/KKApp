<script lang="ts">
	import { Plus, X } from '@lucide/svelte';
	import type { components } from '$lib/api/api';

	type Project = components['schemas']['ProjectDO'];

	interface Props {
		project?: Project | null;
		onselect?: () => void;
		onremove?: () => void;
	}

	const { project = null, onselect, onremove }: Props = $props();
</script>

{#if project}
	<button
		type="button"
		class="bg-card group relative flex w-full items-start gap-3 rounded-lg border p-4 text-left transition-colors hover:bg-accent"
		onclick={onselect}
	>
		<div class="min-w-0 flex-1">
			<p class="text-sm font-medium truncate">{project.name}</p>
			{#if project.description}
				<p class="text-muted-foreground mt-1 line-clamp-2 text-xs">{project.description}</p>
			{/if}
		</div>
		<!-- <button
			type="button"
			class="text-muted-foreground hover:text-destructive -mt-1 -mr-1 shrink-0 rounded p-1 opacity-0 transition-opacity group-hover:opacity-100"
			onclick={onremove}
			aria-label="Remove project"
		>
			<X class="size-4" />
		</button> -->
	</button>
{:else}
	<button
		type="button"
		class="text-muted-foreground hover:border-primary hover:text-primary flex w-full flex-col items-center justify-center gap-2 rounded-lg border border-dashed p-6 transition-colors"
		onclick={onselect}
	>
		<Plus class="size-5" />
		<span class="text-sm">Add project</span>
	</button>
{/if}
