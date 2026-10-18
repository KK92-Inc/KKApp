<script lang="ts">
	import { buttonVariants } from '$lib/components/button';
	import * as Dialog from '$lib/components/dialog';
	import Separator from '$lib/components/separator/separator.svelte';
	import { formatDate } from '$lib/datetime';
	import FreezeStatus from './freeze-status.svelte';
	import type { Freeze } from './state';

	interface Props {
		open: boolean;
		freeze: Freeze;
	}

	let { open = $bindable(false), freeze }: Props = $props();
</script>

<Dialog.Root bind:open>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Freeze of {freeze.user.login}</Dialog.Title>
			<Dialog.Description>
				A freeze temporarily closes an account for a given duration.
			</Dialog.Description>
		</Dialog.Header>

		<FreezeStatus {freeze} />

		<dl class="grid grid-cols-2 gap-x-4 gap-y-3 text-sm">
			<div>
				<dt class="text-xs text-muted-foreground">From</dt>
				<dd>{formatDate(freeze.startsAt)}</dd>
			</div>
			<div>
				<dt class="text-xs text-muted-foreground">Until</dt>
				<dd>{formatDate(freeze.endsAt)}</dd>
			</div>
			<div>
				<dt class="text-xs text-muted-foreground">Created</dt>
				<dd>{formatDate(freeze.createdAt)}</dd>
			</div>
			{#if freeze.invalidatedAt}
				<div>
					<dt class="text-xs text-muted-foreground">Cancelled</dt>
					<dd>{formatDate(freeze.invalidatedAt)}</dd>
				</div>
			{/if}
		</dl>

		<div>
			<p class="mb-1 text-xs text-muted-foreground">Reason</p>
			<p class="max-h-60 overflow-y-auto rounded-md border bg-muted/30 p-3 text-sm break-words whitespace-pre-wrap">
				{freeze.reason}
			</p>
		</div>

		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Close</Dialog.Close>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
