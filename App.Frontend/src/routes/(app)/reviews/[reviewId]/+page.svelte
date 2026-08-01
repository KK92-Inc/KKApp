<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import { Separator } from '$lib/components/separator';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Failed from '$lib/components/empty/failed.svelte';
	import * as Avatar from '$lib/components/avatar';
	import * as Review from '$lib/remotes/review.remote';
	import { Problem } from '$lib/api';
	import {
		Clock,
		Loader,
		ClipboardCheck,
		ClipboardList,
		XCircle,
		Play,
		CheckCircle,
		ArrowLeft,
		Users,
		Globe,
		Bot,
		UserRound
	} from '@lucide/svelte';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();

	// -- Static lookup tables -------------------------------------------------

	/** Bitflag values — keep in sync with the backend's ReviewKinds. */
	const ReviewKind = {
		Self: 1 << 0,
		Peer: 1 << 1,
		Async: 1 << 2,
		Auto: 1 << 3
	} as const;

	const KIND_INFO = [
		{
			flag: ReviewKind.Self,
			label: 'Self Review',
			icon: ClipboardCheck,
			description: 'A reflection on your own work'
		},
		{
			flag: ReviewKind.Peer,
			label: 'Peer Review',
			icon: Users,
			description: 'An in-person review by a peer'
		},
		{
			flag: ReviewKind.Async,
			label: 'Async Review',
			icon: Globe,
			description: 'A remote review by another user'
		},
		{
			flag: ReviewKind.Auto,
			label: 'Auto Review',
			icon: Bot,
			description: 'An automated review'
		}
	];

	function kindInfo(kind: number) {
		return (
			KIND_INFO.find((k) => (kind & k.flag) !== 0) ?? {
				label: 'Review',
				icon: ClipboardList,
				description: 'A project review'
			}
		);
	}

	const STATE_CONFIG = {
		Pending: {
			icon: Clock,
			variant: 'outline' as const,
			class: 'text-muted-foreground',
			label: 'Pending'
		},
		InProgress: {
			icon: Loader,
			variant: 'secondary' as const,
			class: 'text-blue-600 dark:text-blue-400',
			label: 'In Progress'
		},
		Finished: {
			icon: ClipboardCheck,
			variant: 'default' as const,
			class: 'text-green-600 dark:text-green-400',
			label: 'Finished'
		},
		Cancelled: {
			icon: XCircle,
			variant: 'destructive' as const,
			class: 'text-destructive',
			label: 'Cancelled'
		}
	};

	function initials(name: string | null | undefined, login: string) {
		const source = (name?.trim() || login) ?? '?';
		return source
			.split(/\s+/)
			.map((part) => part[0])
			.join('')
			.slice(0, 2)
			.toUpperCase();
	}

	// -- Actions ----------------------------------------------------------------

	let acting = $state(false);

	async function pickUp(reviewId: string) {
		acting = true;
		const result = await Problem.try(() => Review.start(reviewId));
		if (result) await Review.get(reviewId).refresh();
		acting = false;
	}

	async function markComplete(reviewId: string) {
		acting = true;
		const result = await Problem.try(() => Review.complete(reviewId));
		if (result) await Review.get(reviewId).refresh();
		acting = false;
	}
</script>

<div class="mx-auto max-w-2xl p-6">
	<Button variant="ghost" size="sm" href="/reviews" class="mb-4 gap-1 text-muted-foreground">
		<ArrowLeft size={14} />
		Back to reviews
	</Button>

	<svelte:boundary>
		{#snippet pending()}
			<Card.Root class="shadow-none">
				<Card.Content class="space-y-4 p-6">
					<Skeleton class="h-8 w-48" />
					<Skeleton class="h-4 w-full" />
					<Skeleton class="h-4 w-3/4" />
					<Skeleton class="h-32 w-full" />
				</Card.Content>
			</Card.Root>
		{/snippet}

		{#snippet failed(error, reset)}
			<Failed {error} {reset} />
		{/snippet}

		{@const review = await Review.get(params.reviewId)}
		{@const state = STATE_CONFIG[review.state]}
		{@const kind = kindInfo(review.kind)}

		<Card.Root class="shadow-none">
			<Card.Header>
				{@const KindIcon = kind.icon}
				{@const StateIcon = state.icon}
				<div class="flex items-center justify-between">
					<Card.Title class="flex items-center gap-2 text-xl">
						<KindIcon size={20} />
						{kind.label}
					</Card.Title>
					<Badge variant={state.variant} class="gap-1">
						<StateIcon size={12} class={state.class} />
						{state.label}
					</Badge>
				</div>
				<Card.Description>{kind.description}</Card.Description>
			</Card.Header>

			<Card.Content class="space-y-4">
				<Separator />

				<!-- Review details -->
				<div class="grid grid-cols-2 gap-4 text-sm">
					<div>
						<p class="font-medium text-muted-foreground">Reviewer</p>
						{#if review.reviewer}
							<div class="mt-1 flex items-center gap-2">
								<Avatar.Root class="size-6">
									{#if review.reviewer.avatarUrl}
										<Avatar.Image src={review.reviewer.avatarUrl} alt={review.reviewer.login} />
									{/if}
									<Avatar.Fallback class="text-[10px]">
										{initials(review.reviewer.displayName, review.reviewer.login)}
									</Avatar.Fallback>
								</Avatar.Root>
								<span>{review.reviewer.displayName ?? review.reviewer.login}</span>
							</div>
						{:else}
							<p class="flex items-center gap-1 text-muted-foreground italic">
								<UserRound size={14} />
								Not assigned
							</p>
						{/if}
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Rubric</p>
						<p>{review.rubric?.name ?? 'Unknown'}</p>
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Project</p>
						<p>{review.userProject.project?.name ?? 'Unknown'}</p>
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Created</p>
						<p>{new Date(review.createdAt).toLocaleString()}</p>
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Last Updated</p>
						<p>{new Date(review.updatedAt).toLocaleString()}</p>
					</div>
				</div>

				<Separator />

				<!-- Actions -->
<div class="flex gap-2">
	{#if review.state === 'Pending'}
		<Button class="w-full gap-2" loading={acting} onclick={() => pickUp(review.id)}>
			<Play size={16} />
			{review.reviewer ? 'Start Review' : 'Pick Up Review'}
		</Button>
	{:else if review.state === 'InProgress'}
		<!-- TODO: restrict this to review.reviewer once we have the current user in scope -->
		<Button
			class="w-full gap-2"
			variant="default"
			loading={acting}
			onclick={() => markComplete(review.id)}
		>
			<CheckCircle size={16} />
			Complete Review
		</Button>
	{:else if review.state === 'Finished'}
		<div
			class="flex w-full items-center justify-center gap-2 rounded-md border border-green-200 bg-green-50 p-4 text-sm text-green-700 dark:border-green-800 dark:bg-green-950 dark:text-green-300"
		>
			<CheckCircle size={16} />
			This review has been completed.
		</div>
	{:else if review.state === 'Cancelled'}
		<div
			class="flex w-full items-center justify-center gap-2 rounded-md border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive"
		>
			<XCircle size={16} />
			This review was cancelled.
		</div>
	{/if}
</div>
			</Card.Content>
		</Card.Root>
	</svelte:boundary>
</div>
