<script lang="ts">
	import { page } from '$app/state';
	import * as Explorer from '$lib/components/explorer';
	import * as Page from '../../../context.svelte';
	import * as Remote from '../../../page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';

	import * as Avatar from '$lib/components/avatar';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import Label from '$lib/components/label/label.svelte';
	import { MessageSquarePlus, RotateCcw, Trash2, X } from '@lucide/svelte';
	import { onNavigate } from '$app/navigation';
	import Loader from '$lib/components/loader.svelte';
	import Badge from '$lib/components/badge/badge.svelte';

	const context = Page.getContext();
	const { params }: PageProps = $props();
	const { review } = $derived(await Remote.getData(context.reviewId()));
	const isReviewer = $derived(review.reviewer?.id === page.data.session.userId);

	let draft = $state('');
	let focused = $state<number>();

	function add(line: number) {
		if (!draft.trim()) return;
		context.addAnnotation(params.path, {
			$type: 'Comment',
			filepath: params.path,
			body: draft,
			rowStart: line,
			// TODO: Support multi line annotations later
			rowEnd: line
		});

		draft = '';
	}

	function remove(line: number) {
		context.removeAnnotation(params.path, line);
		focused = undefined;
	}

	const onLineClick = $derived.by(() => {
		if (isReviewer && review.state !== 'InProgress') {
			return (line: number) => {
				focused = line === focused ? undefined : line;
				draft = ''; // Reset input when switching lines
			};
		}
	});

	onNavigate(() => {
		// Otherwise we're stuck with it...
		focused = undefined;
	});
</script>

<svelte:boundary>
	{@const content = await Git.getBlob({
		id: review.userProject.gitInfoId,
		ref: review.ref,
		path: params.path
	})}

	{#snippet pending()}
		<div class="flex min-h-100 items-center justify-center">
			<Badge variant="outline" class="rounded-md p-4">
				<Loader />
				<span class="text-xs font-medium text-muted-foreground">Fetching file from repository...</span>
			</Badge>
		</div>
	{/snippet}

	{#snippet failed(error, reset)}
		<div class="flex min-h-100 items-center justify-center">
			<Badge variant="outline" class="flex flex-col rounded-md bg-destructive/10 p-4">
				<span class="animate-pulse text-xs font-medium text-destructive">
					Failed to retrieve file from repository...
				</span>
				<Button variant="destructive" size="sm" onclick={() => reset()}>
					Try Again
					<RotateCcw />
				</Button>
			</Badge>
		</div>
	{/snippet}

	<Explorer.File name={params.path.split('/').pop() ?? ''} {content} backHref="/reviews/{page.params.id}">
		{#snippet text({ text, language })}
			<Explorer.Code {text} {language} {onLineClick} class="border-b">
				{#snippet extra({ line })}
					{@const existing = context.annotations.get(params.path)?.find((a) => a.rowStart === line)}

					{#if existing || line === focused}
						<Item.Root
							variant="outline"
							class="my-2 mr-4 ml-(--code-gutter) max-w-md bg-background shadow-xl"
						>
							{#if existing}
								<Item.Media>
									<Avatar.Root class="size-10 border-2">
										<Avatar.Image src={review.reviewer?.avatarUrl} />
										<Avatar.Fallback>{review.reviewer?.login.slice(0,2).toUpperCase()}</Avatar.Fallback>
									</Avatar.Root>
								</Item.Media>
							{/if}
							<Item.Content>
								{#if existing}
									<Item.Title class="w-full text-foreground">
										{review.reviewer?.displayName ?? review.reviewer?.login}
										<span class="text-xs text-muted-foreground">on #{line}</span>
										<Separator class="flex-1" />
										{#if review.state !== 'InProgress'}
											<Button size="sm" variant="destructive" onclick={() => remove(line)}>
												Remove
												<Trash2 />
											</Button>
										{/if}
									</Item.Title>
								{/if}
								<Item.Description class="p-1 line-clamp-none">
									{#if existing}
										{existing.body}
									{:else}
										<div class="flex items-center gap-2 pb-1">
											<Label>Comment on #{focused}</Label>
											<Separator class="flex-1" />
											<span class="text-xs">{draft.length}/256</span>
										</div>
										<Textarea class="max-h-48 resize-y wrap-anywhere" bind:value={draft} rows={3} maxlength={256} />
										<ButtonGroup.Root class="ml-auto">
											<Button size="sm" variant="outline" onclick={() => (focused = undefined)}>
												Cancel
												<X />
											</Button>
											<Button size="sm" variant="default" class="border" onclick={() => add(line)}>
												Add
												<MessageSquarePlus />
											</Button>
										</ButtonGroup.Root>
									{/if}
								</Item.Description>
							</Item.Content>
						</Item.Root>
					{/if}
				{/snippet}
			</Explorer.Code>
		{/snippet}
	</Explorer.File>
</svelte:boundary>
