<script lang="ts">
	import { onDestroy } from 'svelte';
	import { Check } from '@lucide/svelte';
	import { cn } from '$lib/utils.js';
	import { getStepperContext } from './context.svelte';

	interface Props {
		/** Unique numeric identifier for this step. */
		value: number;
		title?: string;
		subtitle?: string;
		class?: string;
	}

	let { value, title = '', subtitle = '', class: className = '' }: Props = $props();

	const stepper = getStepperContext();
	stepper.register(value);
	onDestroy(() => stepper.unregister(value));

	const isActive = $derived(stepper.isActive(value));
	const isDone = $derived(stepper.isDone(value));
	const isLast = $derived(stepper.steps.at(-1) === value);
	const clickable = $derived(stepper.editable && (isDone || isActive));
</script>

<div
	class={cn(
		'flex min-w-0 items-center',
		stepper.vertical ? 'flex-col' : cn('flex-1', isLast && 'flex-none'),
		className
	)}
>
	<div class={cn('flex items-center', stepper.vertical ? 'flex-col' : 'w-full gap-3')}>
		<button
			type="button"
			disabled={!clickable}
			onclick={() => clickable && stepper.goto(value)}
			class={cn(
				'relative z-10 flex h-8 w-8 shrink-0 items-center justify-center rounded-full',
				'text-xs font-semibold transition-colors duration-200',
				'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
				'disabled:cursor-default',
				isActive && 'bg-primary text-primary-foreground ring-2 ring-primary/30 ring-offset-2',
				isDone && !isActive && 'bg-primary text-primary-foreground',
				!isActive && !isDone && 'bg-muted text-muted-foreground',
				clickable && 'cursor-pointer hover:opacity-90'
			)}
			aria-current={isActive ? 'step' : undefined}
			aria-label={`Step ${value}${title ? `: ${title}` : ''}`}
		>
			{#if isDone}
				<Check class="h-4 w-4" />
			{:else}
				{value}
			{/if}
		</button>

		{#if title || subtitle}
			<div class={cn('min-w-0', stepper.vertical && 'py-1 text-center')}>
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

		{#if !isLast && !stepper.vertical}
			<div
				class={cn(
					'mx-2 h-0.5 flex-1 transition-colors duration-300',
					isDone ? 'bg-primary' : 'bg-border'
				)}
				aria-hidden="true"
			></div>
		{/if}
	</div>

	{#if !isLast && stepper.vertical}
		<div
			class={cn('my-1 h-6 w-0.5 transition-colors duration-300', isDone ? 'bg-primary' : 'bg-border')}
			aria-hidden="true"
		></div>
	{/if}
</div>
