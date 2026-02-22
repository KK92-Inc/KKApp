<!-- @component Raw markdown renderer -->
<script lang="ts">
	import { untrack } from 'svelte';
	import { Markdown } from './render';
	import { cn } from 'tailwind-variants';
	import useDebounce from '$lib/hooks/debounce.svelte';

	interface Props {
		value: string;
		class?: string;
	}

	let { value, class: className }: Props = $props();

	let renderValue = $state('');
	const update = useDebounce((v: string) => (renderValue = v), 250);

	$effect(() => {
		const v = value; // subscribe to value only
		untrack(() => update.fn(v)); // don't track debounce internals
	});
</script>

<svelte:boundary>
	{#snippet pending()}
		<span>Rendering...</span>
	{/snippet}

	<div class={cn('markdown', className)}>
		<!-- NOTE(W2): We sanitize the input via rehype-sanitize -->
		<!-- eslint-disable-next-line svelte/no-at-html-tags -->
		{@html await Markdown.render(renderValue)}
	</div>
</svelte:boundary>
