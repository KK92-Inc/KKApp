<script lang="ts">
	import * as Explorer from '$lib/components/explorer';
	import * as Git from '$lib/remotes/git.remote';
	import * as Alert from '$lib/components/alert';
	import { Button } from '$lib/components/button';
	import { Skeleton } from '$lib/components/skeleton';
	import { ArrowLeft, CircleAlert, RefreshCcw } from '@lucide/svelte';
	import * as Page from '../../../../context.svelte';
	import * as Remote from '../../../../page.remote';
	import type { PageProps } from './$types';
	import type { HttpError } from '@sveltejs/kit';

	const { params }: PageProps = $props();
	const context = Page.getContext();
	const parentPath = $derived(params.path.split('/').slice(0, -1).join('/'));
	const backHref = $derived(context.href({ kind: 'tree', branch: params.branch, path: parentPath }));
</script>

<!-- Key the boundary so that on a slow connection we show the pending state for the new directory -->
{#key `${context.view}:${params.branch}:${params.path}`}
	<svelte:boundary>
		{@const repo = await Remote.repo({
			userId: context.userId(),
			projectId: context.projectId(),
			view: context.view
		})}
		{@const entries = params.path
			? await Git.getTreePath({ id: repo.id, ref: params.branch, path: params.path })
			: await Git.getTree({ id: repo.id, ref: params.branch })}

		{#snippet pending()}
			<Skeleton class="h-60 w-full" />
		{/snippet}

		{#snippet failed(e, reset)}
			{@const err = e as HttpError}
			<Alert.Root variant="destructive">
				<CircleAlert />
				<Alert.Title>{err.body?.message ?? 'Failed to load directory'}</Alert.Title>
				<Alert.Description>
					This could resolve itself or may be a bug.
					<div class="flex items-center gap-1">
						<Button variant="outline" class="text-foreground" size="sm" href={backHref}>
							<ArrowLeft class="size-3" />
							Back
						</Button>
						<Button variant="outline" class="text-foreground" size="sm" onclick={reset}>
							<RefreshCcw class="size-3" />
							Try again
						</Button>
					</div>
				</Alert.Description>
			</Alert.Root>
		{/snippet}

		<Explorer.Tree
			{entries}
			parentHref={params.path
				? context.href({ view: repo.view, kind: 'tree', branch: params.branch, path: parentPath })
				: undefined}
			href={(entry) =>
				context.href({
					view: repo.view,
					kind: entry.directory ? 'tree' : 'blob',
					branch: params.branch,
					path: entry.path
				})}
		/>
	</svelte:boundary>
{/key}
