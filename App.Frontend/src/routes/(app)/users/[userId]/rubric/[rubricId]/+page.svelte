<script lang="ts">
	import * as Card from '$lib/components/card';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Button } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { RuleBuilder } from '$lib/components/rule-builder';
	import { get, hasMarkdown } from '$lib/remotes/rubric.remote';
	import { FileText, Check, X, Edit, GitBranch, Users, FileCheck } from '@lucide/svelte';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();

	function formatTimestamp(iso: string): string {
		return new Date(iso).toLocaleDateString('en-US', {
			month: 'short',
			day: 'numeric',
			year: 'numeric'
		});
	}
</script>

<Layout>
	{#snippet left()}
		<div class="mt-4 flex flex-col gap-3">
			<!-- Rubric info card -->
			<svelte:boundary>
				{#snippet pending()}
					<Skeleton class="h-24 rounded-lg" />
				{/snippet}

				{@const rubric = await get({ id: params.rubricId })}

				<Card.Root class="shadow-none py-0">
					<Card.Content class="flex items-center gap-3 p-3">
						<div class="flex size-16 items-center justify-center rounded-lg bg-muted">
							<FileText class="size-8 text-muted-foreground" />
						</div>
						<div class="min-w-0 flex-1">
							<h1 class="truncate text-sm font-semibold leading-tight">{rubric.name}</h1>
							<div class="mt-1 flex flex-wrap gap-1">
								{#if rubric.enabled}
									<Badge variant="default" class="text-[10px]">Enabled</Badge>
								{:else}
									<Badge variant="secondary" class="text-[10px]">Disabled</Badge>
								{/if}
								{#if rubric.public}
									<Badge variant="outline" class="text-[10px]">Public</Badge>
								{/if}
							</div>
						</div>
					</Card.Content>
				</Card.Root>
			</svelte:boundary>

			<!-- Git status card -->
			<svelte:boundary>
				{#snippet pending()}
					<Skeleton class="h-16 rounded-lg" />
				{/snippet}

				{@const markdownExists = await hasMarkdown({ id: params.rubricId })}

				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-3">
						<h3 class="mb-1.5 flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
							<GitBranch size={12} />
							Repository Status
						</h3>
						<div class="flex items-center gap-2">
							{#if markdownExists}
								<Check class="size-4 text-green-500" />
								<span class="text-xs text-foreground">RUBRIC.md found</span>
							{:else}
								<X class="size-4 text-red-500" />
								<span class="text-xs text-foreground">RUBRIC.md missing</span>
							{/if}
						</div>
						{#if !markdownExists}
							<p class="mt-1 text-[10px] text-muted-foreground">
								Add a RUBRIC.md file to the git repository
							</p>
						{/if}
					</Card.Content>
				</Card.Root>
			</svelte:boundary>

			<!-- Creator card -->
			<svelte:boundary>
				{@const rubric = await get({ id: params.rubricId })}

				{#if rubric.creator}
					<Card.Root class="shadow-none py-0">
						<Card.Content class="p-3">
							<h3 class="mb-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
								Created By
							</h3>
							<div class="flex items-center gap-2">
								{#if rubric.creator.avatarUrl}
									<img
										src={rubric.creator.avatarUrl}
										alt={rubric.creator.displayName ?? 'User'}
										class="size-5 rounded-full object-cover"
									/>
								{/if}
								<span class="text-xs text-foreground">
									{rubric.creator.displayName ?? 'Unknown'}
								</span>
							</div>
							<p class="mt-1 text-[10px] text-muted-foreground">
								Created {formatTimestamp(rubric.createdAt)}
							</p>
						</Card.Content>
					</Card.Root>
				{/if}
			</svelte:boundary>

			<!-- Actions card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-3">
					<div class="space-y-2">
						<Button href="./edit" size="sm" variant="outline" class="w-full">
							<Edit class="size-3 mr-1" />
							Edit Rubric
						</Button>
					</div>
				</Card.Content>
			</Card.Root>
		</div>
	{/snippet}

	{#snippet right()}
		<div class="my-4 grid gap-4">
			<!-- Description section -->
			<svelte:boundary>
				{@const rubric = await get({ id: params.rubricId })}

				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-6">
						<div class="mb-4 flex items-center gap-2">
							<FileCheck class="size-4 text-muted-foreground" />
							<h2 class="text-lg font-semibold">Evaluation Criteria</h2>
						</div>

						{#if rubric.markdown}
							<MarkdownTextarea value={rubric.markdown} readonly />
						{:else}
							<div class="flex flex-col items-center justify-center py-12 text-center">
								<FileCheck class="mb-3 size-8 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									No evaluation criteria have been added yet.
								</p>
							</div>
						{/if}
					</Card.Content>
				</Card.Root>
			</svelte:boundary>

			<!-- Reviewer rules section -->
			<svelte:boundary>
				{@const rubric = await get({ id: params.rubricId })}

				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-6">
						<div class="mb-4 flex items-center gap-2">
							<Users class="size-4 text-muted-foreground" />
							<h2 class="text-lg font-semibold">Reviewer Eligibility Rules</h2>
						</div>

						{#if rubric.reviewerRules && rubric.reviewerRules.length > 0}
							<div class="rounded-lg border p-4">
								<p class="text-sm text-muted-foreground mb-2">Rules visualization would go here</p>
							</div>
						{:else}
							<div class="flex flex-col items-center justify-center py-12 text-center">
								<Users class="mb-3 size-8 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									No reviewer eligibility rules defined.
								</p>
							</div>
						{/if}
					</Card.Content>
				</Card.Root>
			</svelte:boundary>

			<!-- Reviewee rules section -->
			<svelte:boundary>
				{@const rubric = await get({ id: params.rubricId })}

				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-6">
						<div class="mb-4 flex items-center gap-2">
							<FileText class="size-4 text-muted-foreground" />
							<h2 class="text-lg font-semibold">Reviewee Eligibility Rules</h2>
						</div>

						{#if rubric.revieweeRules && rubric.revieweeRules.length > 0}
							<div class="rounded-lg border p-4">
								<p class="text-sm text-muted-foreground mb-2">Rules visualization would go here</p>
							</div>
						{:else}
							<div class="flex flex-col items-center justify-center py-12 text-center">
								<FileText class="mb-3 size-8 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									No reviewee eligibility rules defined.
								</p>
							</div>
						{/if}
					</Card.Content>
				</Card.Root>
			</svelte:boundary>
		</div>
	{/snippet}
</Layout>
