<!-- @component Raw markdown renderer -->
<script lang="ts">
	import { untrack } from 'svelte';
	import { Markdown } from './render';
	import { cn } from 'tailwind-variants';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Skeleton from '../skeleton/skeleton.svelte';

	interface Props {
		value: string;
		class?: string;
	}

	let { value, class: className }: Props = $props();

	// Initialize with prop value directly
	let renderValue = $derived(value);
	const update = useDebounce((v: string) => (renderValue = v), 250);

	$effect(() => {
		const v = value; // subscribe to value only
		untrack(() => update.fn(v)); // don't track debounce internals
	});
</script>

<svelte:boundary>
	{#snippet pending()}
		<Skeleton class="h-32" />
	{/snippet}

	<div class={cn('markdown', className)}>
		<!-- NOTE(W2): We sanitize the input via rehype-sanitize -->
		<!-- eslint-disable-next-line svelte/no-at-html-tags -->
		{@html await Markdown.render(renderValue)}
	</div>
</svelte:boundary>
