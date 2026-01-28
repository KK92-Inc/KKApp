<script lang="ts">
	import { onMount } from 'svelte';

	let messages = $state.raw<string[]>([]);
	let es: EventSource;

	onMount(() => {
		es = new EventSource('/events');

		es.onmessage = (event) => {
			// Svelte 5 rune reactivity
			messages = [...messages, event.data];
		};

		es.onerror = (err) => {
			console.error('SSE error', err);
			es.close();
		};

		return () => {
			if (es) es.close(); // cleanup on navigation
		};
	});
</script>

<ul>
	{#each messages as msg, i (i)}
		<li>{msg}</li>
	{/each}
</ul>
