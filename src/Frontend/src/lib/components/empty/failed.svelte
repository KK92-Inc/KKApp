<script lang="ts">
	import { RotateCcw, X } from '@lucide/svelte';
	import * as Empty from '.';
	import { Button } from '../button';
	import type { HttpError } from '@sveltejs/kit';
	import type { Snippet } from 'svelte';

	interface Props {
		error: unknown;
		reset: () => void;
		icon?: Snippet<[]>;
	}

	const { error, reset, icon }: Props = $props();
</script>

<Empty.Root class="h-80 bg-card">
	<Empty.Header>
		<Empty.Media variant="icon">
			{#if icon}
				{@render icon()}
			{:else}
				<X />
			{/if}
		</Empty.Media>
		<Empty.Title>{(error as HttpError).body.message}</Empty.Title>
	</Empty.Header>
	<Empty.Content>
		<Button onclick={reset}>
			<RotateCcw />
			Try Again
		</Button>
	</Empty.Content>
</Empty.Root>
