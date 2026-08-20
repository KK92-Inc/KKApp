<script lang="ts">
	import { UserRound, Users, MessagesSquare, Bot, Hammer } from '@lucide/svelte';
	import * as Item from '$lib/components/item';
	import { Slider } from '$lib/components/slider';
	import Badge from '$lib/components/badge/badge.svelte';
	import { cn } from '$lib/utils';
	import * as Page from './context.svelte';

	const ctx = Page.getContext();
	const VARIANTS_CONFIG = [
		{
			kind: 'Self',
			title: 'Self Review',
			description: 'Self reflection on how the project was completed.',
			icon: UserRound,
			active:
				'border-sky-200 bg-sky-50 text-sky-700 shadow-sm dark:border-sky-800/80 dark:bg-sky-950/40 dark:text-sky-300',
			media: 'bg-sky-100 text-sky-600 dark:bg-sky-900/70 dark:text-sky-300',
			wip: false
		},
		{
			kind: 'Peer',
			title: 'Peer Review',
			description: 'Conducting a peer review physically with each other inside the building.',
			icon: Users,
			active:
				'border-emerald-200 bg-emerald-50 text-emerald-700 shadow-sm dark:border-emerald-800/80 dark:bg-emerald-950/40 dark:text-emerald-300',
			media: 'bg-emerald-100 text-emerald-600 dark:bg-emerald-900/70 dark:text-emerald-300',
			wip: true
		},
		{
			kind: 'Async',
			title: 'Async Review',
			description: 'Peer review without a physical obligation, can be done anywhere anytime.',
			icon: MessagesSquare,
			active:
				'border-violet-200 bg-violet-50 text-violet-700 shadow-sm dark:border-violet-800/80 dark:bg-violet-950/40 dark:text-violet-300',
			media: 'bg-violet-100 text-violet-600 dark:bg-violet-900/70 dark:text-violet-300',
			wip: false
		},
		{
			kind: 'Auto',
			title: 'Auto Review',
			description: 'Run automated checks such as tests or LLMs against the project.',
			icon: Bot,
			active:
				'border-amber-200 bg-amber-50 text-amber-700 shadow-sm dark:border-amber-800/80 dark:bg-amber-950/40 dark:text-amber-300',
			media: 'bg-amber-100 text-amber-600 dark:bg-amber-900/70 dark:text-amber-300',
			wip: true
		}
	];

	/**
	 * Reads slider value for a given kind from context state
	 */
	function get(kind: string): number {
		const found = ctx.fields.variants.find((v) => v.kind === kind);
		return found ? found.required : 0;
	}

	/**
	 * Updates the context variants array reactively
	 */
	function set(kind: string, val: number) {
		const current = [...ctx.fields.variants];
		const index = current.findIndex((v) => v.kind === kind);

		if (val > 0) {
			if (index > -1) {
				current[index] = { ...current[index], required: val };
			} else {
				current.push({ kind, required: val });
			}
		} else if (index > -1) {
			current.splice(index, 1);
		}

		ctx.fields.variants = current;
	}
</script>

<Item.Group class="grid gap-3 sm:grid-cols-2">
	{#each VARIANTS_CONFIG as item (item.kind)}
		{@const value = get(item.kind)}
		{@const active = value > 0}
		<Item.Root
			variant="muted"
			size="sm"
			class={cn(
				'border border-transparent bg-muted/30 text-muted-foreground dark:bg-muted/20',
				active && item.active
			)}
		>
			<Item.Media
				variant="icon"
				class={active ? item.media : 'bg-muted text-muted-foreground dark:bg-muted/60'}
			>
				<item.icon class="size-4" />
			</Item.Media>

			<Item.Content class="min-w-0">
				<div class="mb-1 flex items-center justify-between gap-2">
					<Item.Title class="text-sm font-medium">
						{item.title}
						{#if item.wip}
							<Badge variant="outline" class="rounded-sm">
								Work In Progress
								<Hammer />
							</Badge>
						{/if}
					</Item.Title>
					<span class="text-[11px] font-medium {active ? 'text-current' : 'text-muted-foreground'}">
						{value}/5
					</span>
				</div>

				<Item.Description class="text-xs leading-relaxed">
					{item.description}
				</Item.Description>

				<div class="mt-3">
					<Slider
						type="single"
						disabled={item.wip}
						{value}
						onValueChange={(val) => set(item.kind, val)}
						min={0}
						max={5}
						step={1}
						class={active ? 'opacity-100' : 'opacity-80'}
					/>
				</div>
			</Item.Content>
		</Item.Root>
	{/each}
</Item.Group>
