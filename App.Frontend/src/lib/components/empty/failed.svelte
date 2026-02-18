<script lang="ts">
	import { RotateCcw, X } from '@lucide/svelte';
	import * as Empty from '.';
	import { Button } from '../button';
	import type { HttpError } from '@sveltejs/kit';
	import type { Snippet } from 'svelte';
	import type { ClassValue } from 'clsx';
	import { cn } from '$lib/utils';

	interface Props {
		error: unknown;
		class?: ClassValue;
		reset: () => void;
		icon?: Snippet<[]>;
	}

	const { error, reset, icon, class: klass }: Props = $props();
</script>

<Empty.Root class={cn(klass, "h-80 bg-muted")}>
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
