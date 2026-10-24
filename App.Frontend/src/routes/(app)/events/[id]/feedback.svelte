<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import * as Field from '$lib/components/field';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import { Label } from '$lib/components/label';
	import { Slider } from '$lib/components/slider';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import * as Events from '$lib/remotes/events.remote';
	import { Problem } from '$lib/api';
	import { formatDate } from '$lib/datetime';
	import type { components } from '$lib/api/api';
	import { page } from '$app/state';
	import { refreshAll } from '$app/navigation';
	import { MessageSquareHeart, Send, Star } from '@lucide/svelte';
	import { toast } from 'svelte-sonner';

	type Props = { event: components['schemas']['EventDO'] };
	const { event }: Props = $props();

	const MAX_COMMENT = 2048;
	const userId = $derived(page.data.session.userId);
	const participant = $derived(event.participants.some((u) => u.id === userId));

	let index = $state(0);
	let rating = $state(8);
	let comment = $state('');
	let submitting = $state(false);
	let submitted = $state(false);

	const valid = $derived(comment.trim().length > 0 && comment.length <= MAX_COMMENT);

	async function submit() {
		if (submitting || !valid) return;
		submitting = true;
		try {
			const feedback = await Problem.try(() =>
				Events.addFeedback({ id: event.id, rating, comment })
			);

			if (!feedback) return;
			submitted = true;
			comment = '';
			index = 0;
			toast.success('Thanks for your feedback!');
			// The feedback is cached per page, refresh whatever is on screen.
			await refreshAll({ includeLoadFunctions: false });
		} finally {
			submitting = false;
		}
	}

	const authorName = (user?: components['schemas']['UserLightDO']) =>
		user ? (user.displayName ?? user.login) : 'Anonymous';
</script>

<Card.Root id="feedback" class="scroll-mt-4">
	<Card.Header>
		<Card.Title class="flex items-center gap-2">
			<MessageSquareHeart size={18} />
			Feedback
		</Card.Title>
		<Card.Description>What the participants thought of this event.</Card.Description>
	</Card.Header>
	<Separator />

	<svelte:boundary>
		{@const feedback = await Events.getFeedback({
			id: event.id,
			page: index,
			sortBy: 'CreatedAt',
			sort: 'Descending'
		})}
		{@const mine = submitted || feedback.data.some((f) => f.comment.user?.id === userId)}

		{#if participant && !mine}
			<Card.Content>
				<Field.Set class="gap-3">
					<Field.Group>
						<Field.Field>
							<div class="flex items-center gap-2 pb-1">
								<Label for="feedback-rating">Rating</Label>
								<Separator class="flex-1" />
								<span class="flex items-center gap-1 text-sm font-medium">
									<Star size={14} />
									{rating}/10
								</span>
							</div>
							<Slider
								id="feedback-rating"
								type="single"
								bind:value={rating}
								min={0}
								max={10}
								step={1}
							/>
						</Field.Field>

						<Field.Field>
							<div class="flex items-center gap-2 pb-1">
								<Label for="feedback-comment">Comment</Label>
								<Separator class="flex-1" />
								<span class="text-xs text-muted-foreground">{comment.length}/{MAX_COMMENT}</span>
							</div>
							<Textarea
								id="feedback-comment"
								bind:value={comment}
								class="max-h-42 resize-y"
								rows={3}
								maxlength={MAX_COMMENT}
								placeholder="What went well, what could be better..."
							/>
						</Field.Field>
					</Field.Group>

					<div class="flex justify-end">
						<Button disabled={!valid || submitting} onclick={submit}>
							{submitting ? 'Submitting...' : 'Submit Feedback'}
							<Send />
						</Button>
					</div>
				</Field.Set>
			</Card.Content>
			<Separator />
		{:else if participant}
			<Card.Content class="text-sm text-muted-foreground">
				You've already left feedback for this event, thank you!
			</Card.Content>
			<Separator />
		{/if}

		<Card.Content class="space-y-4">
			{#each feedback.data as entry (entry.id)}
				{@const author = entry.comment.user}
				<div class="flex gap-3">
					<Avatar.Root class="size-10">
						<Avatar.Image class="border-2" src={author?.avatarUrl} />
						<Avatar.Fallback class="border-2">
							{authorName(author).slice(0, 2).toUpperCase()}
						</Avatar.Fallback>
					</Avatar.Root>
					<div class="min-w-0 flex-1 space-y-1">
						<div class="flex flex-wrap items-center gap-2">
							<span class="font-medium">{authorName(author)}</span>
							{#if author?.id === userId}
								<Badge class="rounded-sm" variant="default">You</Badge>
							{/if}
							<Badge class="rounded-sm" variant="outline">
								<Star />
								{entry.rating}/10
							</Badge>
							<span class="text-xs text-muted-foreground">{formatDate(entry.createdAt)}</span>
						</div>
						<p class="text-sm break-words whitespace-pre-wrap">{entry.comment.body}</p>
					</div>
				</div>
			{:else}
				<p class="text-sm text-muted-foreground">
					{participant ? 'Be the first to leave feedback.' : 'No feedback has been left yet.'}
				</p>
			{/each}

			{#if feedback.pages > 1}
				<Separator />
				<Paginate
					page={index}
					onPageChange={(p) => (index = p)}
					perPage={feedback.perPage}
					count={feedback.count}
				/>
			{/if}
		</Card.Content>

		{#snippet pending()}
			<Card.Content class="text-sm text-muted-foreground">Loading feedback...</Card.Content>
		{/snippet}
	</svelte:boundary>
</Card.Root>
