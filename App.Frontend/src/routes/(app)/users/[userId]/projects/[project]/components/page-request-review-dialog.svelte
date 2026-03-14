<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import { Button, buttonVariants } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Checkbox } from '$lib/components/checkbox';
	import { Label } from '$lib/components/label';
	import { getRubricsForProject, requestReviews } from '$lib/remotes/reviews.remote';
	import { ClipboardCheck, Users, Globe, Bot } from '@lucide/svelte';
	import type { components } from '$lib/api/api';
	import { Input } from '$lib/components/input';

	type Rubric = components['schemas']['RubricDO'];
	type ReviewVariant = components['schemas']['ReviewVariant'];

	interface Props {
		userProjectId: string;
		open?: boolean;
	}

	let { userProjectId, open = $bindable(false) }: Props = $props();

	let rubrics = $state<Rubric[]>([]);
	let loading = $state(true);
	let selectedRubric = $state<Rubric | null>(null);
	let selectedKinds = $state(new Set<ReviewVariant>());
	let step = $state('rubric' as 'rubric' | 'kinds');

	const reviewKindInfo = {
		Self: {
			label: 'Self Review',
			description: 'Reflect on your own work',
			icon: ClipboardCheck
		},
		Peer: {
			label: 'Peer Review',
			description: 'A person physically next to you reviews your work',
			icon: Users
		},
		Async: {
			label: 'Async Review',
			description: 'A remote reviewer evaluates your work',
			icon: Globe
		},
		Auto: {
			label: 'Auto Review',
			description: 'Automated review (not yet supported)',
			icon: Bot
		}
	};

	/** Parse the supported review kinds from the flags enum string */
	function getSupportedKinds(rubric: Rubric): ReviewVariant[] {
		const kindsStr = String(rubric.supportedReviewKinds);
		const all: ReviewVariant[] = ['Self', 'Peer', 'Async', 'Auto'];

		// The backend sends this as a comma-separated flags string or individual value
		if (kindsStr.includes(',')) {
			return all.filter((k) => kindsStr.includes(k));
		}

		// Check for exact matches or "All"
		if (kindsStr === 'All') return all.filter((k) => k !== 'Auto');
		return all.filter((k) => kindsStr.includes(k));
	}

	function toggleKind(kind: ReviewVariant) {
		const next = new Set(selectedKinds);
		if (next.has(kind)) {
			next.delete(kind);
		} else {
			next.add(kind);
		}
		selectedKinds = next;
	}

	async function loadRubrics() {
		loading = true;
		try {
			rubrics = await getRubricsForProject(userProjectId);
		} catch {
			rubrics = [];
		} finally {
			loading = false;
		}
	}

	function selectRubric(rubric: Rubric) {
		selectedRubric = rubric;
		selectedKinds = new Set();
		step = 'kinds';
	}

	function goBack() {
		step = 'rubric';
		selectedRubric = null;
		selectedKinds = new Set();
	}

	$effect(() => {
		if (open) {
			loadRubrics();
			step = 'rubric';
			selectedRubric = null;
			selectedKinds = new Set();
		}
	});
</script>

<Dialog.Root bind:open>
	<Dialog.Trigger
		type="button"
		class={buttonVariants({ variant: 'outline', size: 'sm', class: 'h-5 px-1.5 text-[10px]' })}
	>
		Request Review
	</Dialog.Trigger>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>
				{step === 'rubric' ? 'Request Review' : 'Select Review Types'}
			</Dialog.Title>
			<Dialog.Description>
				{#if step === 'rubric'}
					Choose a rubric to evaluate your project with.
				{:else}
					Select which types of reviews you'd like to request. You can select multiple.
				{/if}
			</Dialog.Description>
		</Dialog.Header>

		{#if step === 'rubric'}
			<div class="max-h-64 space-y-2 overflow-y-auto">
				{#if loading}
					<Skeleton class="h-16 w-full rounded-md" />
					<Skeleton class="h-16 w-full rounded-md" />
				{:else if rubrics.length === 0}
					<p class="py-6 text-center text-sm text-muted-foreground">No rubrics available for this project.</p>
				{:else}
					{#each rubrics as rubric (rubric.id)}
						<button
							type="button"
							class="w-full rounded-md border bg-card p-3 text-left transition-colors hover:bg-accent/50"
							onclick={() => selectRubric(rubric)}
						>
							<div class="flex items-center justify-between">
								<span class="font-medium">{rubric.name}</span>
								<Badge variant="outline" class="text-[10px]">
									{rubric.supportedReviewKinds}
								</Badge>
							</div>
							{#if rubric.slug}
								<p class="mt-1 text-xs text-muted-foreground">{rubric.slug}</p>
							{/if}
						</button>
					{/each}
				{/if}
			</div>
		{:else if selectedRubric}
			<div class="space-y-3">
				<div class="rounded-md border bg-muted/30 px-3 py-2">
					<p class="text-xs text-muted-foreground">
						Rubric: <strong class="text-foreground">{selectedRubric.name}</strong>
					</p>
				</div>

				{#each getSupportedKinds(selectedRubric) as kind (kind)}
					{@const info = reviewKindInfo[kind]}
					{@const disabled = kind === 'Auto'}
					{@const KindIcon = info.icon}
					<button
						type="button"
						class="flex w-full items-center gap-3 rounded-md border p-3 text-left transition-colors
							{disabled ? 'cursor-not-allowed opacity-50' : 'hover:bg-accent/50'}
							{selectedKinds.has(kind) ? 'border-primary bg-primary/5' : ''}"
						onclick={() => !disabled && toggleKind(kind)}
						{disabled}
					>
						<Checkbox
							checked={selectedKinds.has(kind)}
							{disabled}
							onCheckedChange={() => !disabled && toggleKind(kind)}
						/>
						<KindIcon size={16} class="shrink-0 text-muted-foreground" />
						<div class="min-w-0 flex-1">
							<p class="text-sm font-medium">{info.label}</p>
							<p class="text-xs text-muted-foreground">{info.description}</p>
						</div>
						{#if disabled}
							<Badge variant="outline" class="text-[10px]">Soon</Badge>
						{/if}
					</button>
				{/each}
			</div>

			<Dialog.Footer class="flex gap-2">
				<Button variant="outline" size="sm" onclick={goBack}>Back</Button>
				<form {...requestReviews} class="flex-1">
					<input hidden {...requestReviews.fields.userProjectId.as('text')} value={userProjectId} />
					<input hidden {...requestReviews.fields.rubricId.as('text')} value={selectedRubric.id} />
					<input hidden {...requestReviews.fields.kinds.as('text')} value={[...selectedKinds].join(',')} />
					<Button
						type="submit"
						size="sm"
						class="w-full"
						disabled={selectedKinds.size === 0}
						loading={requestReviews.pending > 0}
					>
						Request {selectedKinds.size} Review{selectedKinds.size !== 1 ? 's' : ''}
					</Button>
				</form>
			</Dialog.Footer>
		{/if}
	</Dialog.Content>
</Dialog.Root>
