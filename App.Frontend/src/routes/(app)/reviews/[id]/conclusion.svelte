<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import {
		CircleAlert,
		CircleCheck,
		CircleX,
		ExternalLink,
		MessageSquareHeart,
		Users,
		X
	} from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import { Button, buttonVariants } from '$lib/components/button';
	import { Label } from '$lib/components/label';
	import { Check } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Textarea from '$lib/components/textarea/textarea.svelte';

	const context = Page.getContext();
	const { review } = $derived(await Remote.load(context.reviewId()));

	let open = $state(false);
	let submitting = $state(false);

	async function submit() {
		if (submitting) return;
		submitting = true;
		try {
			if (await context.finish(review))
				open = false;
		} finally {
			submitting = false;
		}
	}

	const fileCount = $derived(context.annotations.size);
	const totalAnnotations = $derived.by(() => {
		const values = Array.from(context.annotations.values());
		return values.reduce((acc, list) => acc + list.length, 0);
	});
</script>

<Dialog.Root bind:open>
	<Dialog.Trigger
		type="button"
		id="action-finish"
		class={buttonVariants({ variant: 'outline', class: 'rounded-l-none' })}
		disabled={review.state !== 'InProgress'}
	>
		Finish <Check />
	</Dialog.Trigger>
	<Dialog.Content class="sm:max-w-md">
		<svelte:boundary>
			<Dialog.Header>
				<Dialog.Title>Finalize Review for {review.userProject.name}</Dialog.Title>
				<Dialog.Description class="space-y-3 pt-1">
					Choose whether this project passes or fails, and summarize your final decision below.
				</Dialog.Description>
			</Dialog.Header>
			<Separator />

			{#if totalAnnotations === 0}
				<Alert.Root variant="warning">
					<CircleAlert />
					<Alert.Title class="flex w-full items-center justify-between">
						<span>No Annotations Added</span>
						<span class="text-xs font-normal text-muted-foreground">{totalAnnotations} / 256</span>
					</Alert.Title>
					<Alert.Description class="text-xs">
						You haven't left any comments yet. Consider adding inline annotations on specific lines to provide
						clearer feedback.
					</Alert.Description>
				</Alert.Root>
			{:else}
				<Alert.Root>
					<MessageSquareHeart />
					<Alert.Title class="flex w-full items-center justify-between">
						<span>Inline Annotations</span>
						<span class="text-xs font-normal text-muted-foreground">{totalAnnotations} / 256</span>
					</Alert.Title>
					<Alert.Description>
						<p class="text-xs">
							You've added <strong>{totalAnnotations}</strong> inline {totalAnnotations === 1
								? 'comment'
								: 'comments'} across <strong>{fileCount}</strong>
							{fileCount === 1 ? 'file' : 'files'}.
						</p>
					</Alert.Description>
				</Alert.Root>
			{/if}
			{#if review.roundId}
				<Button
					variant="secondary"
					href="/reviews/round/{review.roundId}"
					target="_blank"
					rel="noopener noreferrer"
					class="w-full justify-between gap-2 border text-muted-foreground shadow-xs"
				>
					<span class="flex items-center gap-2">
						<Users size={16} />
						<span>View existing feedback in this round</span>
					</span>
					<ExternalLink size={16} />
				</Button>
			{/if}
			<Separator />

			<Field.Set class="gap-3">
				<Tabs.Root
					value={context.pass ? 'pass' : 'fail'}
					onValueChange={(v) => (context.pass = v === 'pass')}
				>
					<Tabs.List class="w-full">
						<Tabs.Trigger
							class="data-active:bg-green-600! data-active:text-white! dark:data-active:bg-green-500/50! dark:data-active:text-white!"
							value="pass"
						>
							Pass <Check />
						</Tabs.Trigger>
						<Tabs.Trigger
							class="data-active:bg-destructive! data-active:text-white! dark:data-active:bg-destructive/50! dark:data-active:text-white!"
							value="fail"
						>
							Fail <X />
						</Tabs.Trigger>
					</Tabs.List>
					<Tabs.Content value="pass">
						<Alert.Root variant="success">
							<CircleCheck />
							<Alert.Title>Project passed this review</Alert.Title>
							<Alert.Description>
								{#if review.roundId === null}
									This is an informal review, so passing doesn't count toward official project completion.
								{:else}
									Passing this review brings the project one step closer to official completion.
								{/if}
							</Alert.Description>
						</Alert.Root>
					</Tabs.Content>
					<Tabs.Content value="fail">
						<Alert.Root variant="destructive">
							<CircleX />
							<Alert.Title>Project failed this review</Alert.Title>
							<Alert.Description>
								{#if review.roundId === null}
									This is an informal review, so failing won't affect the project's official status.
								{:else}
									Failing means the project needs a few changes before it can be resubmitted and passed.
								{/if}
							</Alert.Description>
						</Alert.Root>
					</Tabs.Content>
				</Tabs.Root>

				<Field.Group>
					<Field.Field>
						<div class="flex items-center gap-2 pb-1">
							<Label for="conclusion">Conclusion</Label>
							<Separator class="flex-1" />
							<span class="text-xs text-muted-foreground">{context.conclusion.length}/2048</span>
						</div>

						<Textarea
							id="conclusion"
							bind:value={context.conclusion}
							class="max-h-42 resize-y"
							rows={3}
							maxlength={2048}
							placeholder="Explain your decision and provide feedback..."
						/>
					</Field.Field>
				</Field.Group>
			</Field.Set>

			<Dialog.Footer>
				<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
				<Button type="button" disabled={submitting} onclick={submit}>
					{submitting ? 'Submitting...' : 'Finish Review'}
				</Button>
			</Dialog.Footer>
		</svelte:boundary>
	</Dialog.Content>
</Dialog.Root>
