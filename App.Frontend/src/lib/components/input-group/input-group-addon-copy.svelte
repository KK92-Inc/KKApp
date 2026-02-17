<script lang="ts">
	import { page } from '$app/state';
	import { CircleQuestionMark, Copy, CopyCheck } from '@lucide/svelte';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import { fade } from 'svelte/transition';

	interface Props {
		value: string;
	}

	const { value }: Props = $props();

	let executed = $state(false);
	async function copyToClipboard() {
		executed = true;
		await navigator.clipboard.writeText(value);
		setTimeout(() => (executed = false), 1000);
	}
</script>

<Tooltip.Root>
	<Tooltip.Trigger>
		{#snippet child({ props })}
			<InputGroup.Button {...props} onclick={copyToClipboard} class="rounded-full" size="icon-xs">
				<div class="grid place-items-center">
					{#if executed}
						<span transition:fade={{ duration: 100 }} class="col-start-1 row-start-1">
							<CopyCheck />
						</span>
					{:else}
						<span transition:fade={{ duration: 100 }} class="col-start-1 row-start-1">
							<Copy />
						</span>
					{/if}
				</div>
			</InputGroup.Button>
		{/snippet}
	</Tooltip.Trigger>
	<Tooltip.Content>Copy to Clipboard</Tooltip.Content>
</Tooltip.Root>
