<script lang="ts">
	import { onDestroy } from 'svelte';
	import type { Component } from 'svelte';
	import { Check, X } from '@lucide/svelte';
	import { cn } from '$lib/utils.js';
	import * as Stepper from './index.svelte';

	interface Props {
		/** Unique numeric identifier for this step. */
		value: number;
		/** Label shown next to / below the indicator. */
		title?: string;
		/** Optional secondary line below the title. */
		subtitle?: string;
		/** Force this step into the completed state. */
		complete?: boolean;
		/** Put this step into an error state. */
		error?: boolean;
		/** Replace the default number / check with a custom Lucide icon. */
		icon?: Component;
		class?: string;
	}

	let {
		value,
		title = '',
		subtitle = '',
		complete = false,
		error = false,
		icon: Icon = undefined,
		class: className = ''
	}: Props = $props();

	const ctx = Stepper.getContext();

	// Register / unregister lifecycle
	ctx.registerStep(value);
	onDestroy(() => ctx.unregisterStep(value));

	// Derived visual states
	const isActive = $derived(ctx.isActive(value));
	const isCompleted = $derived(complete || ctx.isCompleted(value));
	const isLast = $derived(ctx.isLastStep(value));
	const isClickable = $derived(ctx.editable && (isCompleted || isActive));

	function handleClick() {
		if (isClickable) ctx.step = value;
	}
</script>

<!--
  Each item is itself a flex container that holds:
    • The indicator circle (number / icon)
    • The text block (title + subtitle)
    • The connector line that stretches to the next item
      (hidden for the last step)
-->
<div
	class={cn(
		'flex min-w-0',
		ctx.vertical ? 'flex-row gap-4' : cn('flex-col', !isLast && 'flex-1'),
		ctx.altLabels && !ctx.vertical ? 'items-center' : '',
		className
	)}
>
	<!-- Top row: indicator + (if not altLabels) text + connector -->
	<div
		class={cn(
			'flex items-center',
			ctx.vertical ? 'flex-col' : 'w-full',
			!ctx.altLabels && !ctx.vertical ? 'gap-3' : ''
		)}
	>
		<!-- Indicator bubble -->
		<button
			type="button"
			disabled={!isClickable}
			onclick={handleClick}
			class={cn(
				'relative z-10 flex h-9 w-9 shrink-0 items-center justify-center rounded-full',
				'text-sm font-semibold transition-all duration-200',
				'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
				'disabled:cursor-default',
				isActive &&
					!error &&
					'bg-primary text-primary-foreground shadow-md ring-2 ring-primary ring-offset-2',
				isCompleted && !error && 'bg-primary text-primary-foreground',
				error && 'bg-destructive text-destructive-foreground',
				!isActive && !isCompleted && !error && 'bg-muted text-muted-foreground',
				isClickable && 'cursor-pointer hover:opacity-90'
			)}
			aria-current={isActive ? 'step' : undefined}
			aria-label={`Step ${value}${title ? ': ' + title : ''}`}
		>
			{#if error}
				<X class="h-4 w-4" />
			{:else if isCompleted}
				<Check class="h-4 w-4" />
			{:else if Icon}
				<Icon class="h-4 w-4" />
			{:else}
				{value}
			{/if}
		</button>

		<!-- Inline text (non-altLabels, horizontal) -->
		{#if !ctx.altLabels && !ctx.vertical && (title || subtitle)}
			<div class="min-w-0 shrink-0">
				{#if title}
					<p
						class={cn(
							'text-sm font-medium leading-tight',
							isActive ? 'text-foreground' : 'text-muted-foreground'
						)}
					>
						{title}
					</p>
				{/if}
				{#if subtitle}
					<p class="text-xs text-muted-foreground">{subtitle}</p>
				{/if}
			</div>
		{/if}

		<!-- Connector line -->
		{#if !isLast}
			<div
				class={cn(
					'transition-colors duration-300',
					ctx.vertical
						? 'mx-auto mt-1 h-8 w-0.5'
						: 'mx-2 h-0.5 flex-1',
					isCompleted ? 'bg-primary' : 'bg-border'
				)}
				aria-hidden="true"
			></div>
		{/if}
	</div>

	<!-- Alt-labels text (below indicator, horizontal) -->
	{#if ctx.altLabels && !ctx.vertical && (title || subtitle)}
		<div class="mt-2 min-w-0 text-center">
			{#if title}
				<p
					class={cn(
						'text-sm font-medium leading-tight',
						isActive ? 'text-foreground' : 'text-muted-foreground'
					)}
				>
					{title}
				</p>
			{/if}
			{#if subtitle}
				<p class="text-xs text-muted-foreground">{subtitle}</p>
			{/if}
		</div>
	{/if}

	<!-- Vertical inline text (beside indicator) -->
	{#if ctx.vertical && (title || subtitle)}
		<div class="min-w-0 py-1">
			{#if title}
				<p
					class={cn(
						'text-sm font-medium leading-tight',
						isActive ? 'text-foreground' : 'text-muted-foreground'
					)}
				>
					{title}
				</p>
			{/if}
			{#if subtitle}
				<p class="text-xs text-muted-foreground">{subtitle}</p>
			{/if}
		</div>
	{/if}
</div>
