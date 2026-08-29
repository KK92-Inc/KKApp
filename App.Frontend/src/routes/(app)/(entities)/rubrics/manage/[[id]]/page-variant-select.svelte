<script lang="ts">
  import { UserRound, Users, MessagesSquare, Bot } from '@lucide/svelte';
  import * as Item from '$lib/components/item';
  import { Slider } from '$lib/components/slider';
  import Badge from '$lib/components/badge/badge.svelte';
  import { cn } from '$lib/utils';
  import * as Page from './context.svelte';
  import type { components } from '$lib/api/api';

  type Variant = components['schemas']['RubricVariantDO'];
  type Kind = Variant['kind'];

  const MAX_REQUIRED = 5;
	// TODO: Unlock the rest once we have proper backend support.
  const ALL_KINDS: Kind[] = ['Self', /* 'Peer', */ 'Async', /*'Auto' */];

  const context = Page.getContext();

  function getVariant(kind: Kind): Variant {
    return context.fields.variants.find((v) => v.kind === kind) ?? ({ kind, required: 0 } as Variant);
  }

  function updateVariant(kind: Kind, required: number) {
    if (required <= 0) {
      context.fields.variants = context.fields.variants.filter((v) => v.kind !== kind);
      return;
    }

    const index = context.fields.variants.findIndex((v) => v.kind === kind);
    if (index !== -1) {
      context.fields.variants[index].required = required;
    } else {
      context.fields.variants = [...context.fields.variants, { kind, required } as Variant];
    }
  }
</script>

{#snippet variant(variant: Variant, onValueChange: (v: number) => void)}
	{@const isActive = variant.required > 0}
	<Item.Root
		variant="outline"
		class={cn(
			'relative transition-all duration-200',
			isActive
				? 'border-primary/50 bg-primary/5 shadow-xs ring-1 ring-primary/20'
				: 'border-border/60 opacity-75 hover:opacity-100'
		)}
	>
		<Item.Media
			variant="icon"
			class={cn(
				'transition-colors',
				isActive ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground dark:bg-muted/60'
			)}
		>
			{#if variant.kind === 'Self'}
				<UserRound size={16} />
			{:else if variant.kind === 'Async'}
				<MessagesSquare size={16} />
			{:else if variant.kind === 'Auto'}
				<Bot size={16} />
			{:else if variant.kind === 'Peer'}
				<Users size={16} />
			{/if}
		</Item.Media>

		<Item.Content>
			<div class="flex items-center justify-between gap-2">
				<Item.Title class={cn(isActive && 'font-semibold text-foreground')}>
					{variant.kind}
				</Item.Title>
				<Badge
					variant={isActive ? 'default' : 'outline'}
					class={cn('font-mono text-xs transition-colors', !isActive && 'text-muted-foreground opacity-60')}
				>
					{variant.required} / {MAX_REQUIRED}
				</Badge>
			</div>

			<Item.Description class="mt-1">
				{#if variant.kind === 'Self'}
					Self reflection on how the project was completed.
				{:else if variant.kind === 'Async'}
					Peer review without a physical obligation, can be done anywhere anytime.
				{:else if variant.kind === 'Auto'}
					Run automated checks such as tests or LLMs against the project.
				{:else if variant.kind === 'Peer'}
					Conducting a peer review physically with each other inside the building.
				{/if}
			</Item.Description>
		</Item.Content>

			<Slider
				type="single"
				disabled={context.fields.deprecated}
				value={variant.required}
				{onValueChange}
				min={0}
				max={MAX_REQUIRED}
				step={1}
				class={cn('transition-opacity', isActive ? 'opacity-100' : 'opacity-60 hover:opacity-100')}
			/>
	</Item.Root>
{/snippet}

<Item.Group class="grid gap-3 sm:grid-cols-2">
	{#each ALL_KINDS as kind (kind)}
		{@const v = getVariant(kind)}
		{@render variant(v, (value) => updateVariant(kind, value))}
	{/each}
</Item.Group>
