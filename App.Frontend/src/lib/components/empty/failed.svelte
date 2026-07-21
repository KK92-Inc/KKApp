<script lang="ts">
	import { RotateCcw, TriangleAlert } from '@lucide/svelte';
	import * as Empty from '.';
	import { Button } from '../button';
	import type { HttpError } from '@sveltejs/kit';
	import type { ClassValue } from 'clsx';
	import { cn } from '$lib/utils';

	interface Props {
		error: unknown;
		class?: ClassValue;
		reset: () => void;
	}

	const { error, reset, class: klass }: Props = $props();
</script>

<Empty.Root class={cn(klass, 'h-auto gap-2 bg-muted')}>
	<Empty.Header>
		<Empty.Title class="text-xs text-destructive flex items-center gap-2 animate-pulse">
			<TriangleAlert size={18}/>
			{(error as HttpError).body.message}
		</Empty.Title>
	</Empty.Header>
	<Empty.Content>
		<Button size="sm" variant="outline" onclick={reset}>
			<RotateCcw />
			Try Again
		</Button>
	</Empty.Content>
</Empty.Root>
