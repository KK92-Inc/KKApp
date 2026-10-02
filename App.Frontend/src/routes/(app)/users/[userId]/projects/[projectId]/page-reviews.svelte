<script lang="ts">
	import * as Item from '$lib/components/item/';
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import { HeartHandshake, Plus, SortAsc, SortDesc } from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { page } from '$app/state';
	import * as Card from '$lib/components/card';
	import * as Page from './context.svelte';
	import Failed from '$lib/components/empty/failed.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import type { components } from '$lib/api/api';
	import * as Select from '$lib/components/select';
	import { Order, Problem } from '$lib/api';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Git from '$lib/remotes/git.remote';
	import { toast } from 'svelte-sonner';

	const context = Page.getContext();
	let sort = $state<components['schemas']['Order']>('Descending');
	const session = $derived(
		await UserProjects.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const git = $derived(session?.gitInfo ? await Git.getBranches(session.gitInfo.id) : []);
	async function requestReview() {
		const head = git.find((b) => b.head);
		if (!session) toast.error('Session in');
		else if (!head) toast.error('You have not submitted anything yet.');
		else {
			await Problem.try(async () => {
				await Reviews.create({ userProjectId: session.id, ref: head.name });
			});
		}
	}
</script>

{#if session}
	<Card.Root class="gap-2 py-3">
		<Card.Header class="flex items-center  justify-between px-4">
			<Card.Title
				class="flex items-center gap-2 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
			>
				<HeartHandshake size={16} />
				Reviews
			</Card.Title>
		</Card.Header>

		<!-- Show Review state -->
		 <!-- Basically compare the  -->
		<Card.Content>

		</Card.Content>

		<!-- Show Recent Reviews -->
		<Card.Content>

		</Card.Content>
	</Card.Root>
{/if}

<!-- {#if session}
	<svelte:boundary>
		{@const members = await UserProjects.getMembersPage({ id: session.id })}
		{@const membership = members.data.find((v) => v.userId === page.data.session.userId && !v.leftAt)}
		{@const reviews = await Reviews.getPage({
			sort,
			userProjectId: session.id,
			sortBy: 'CreatedAt',
			size: 4
		})}

		{#snippet failed(error, reset)}
			<Failed {error} {reset} />
		{/snippet}

		{#snippet pending()}
			<Skeleton class="h-16 w-full" />
			<Skeleton class="h-16 w-full" />
			<Skeleton class="h-16 w-full" />
		{/snippet}

		<Card.Root class="gap-2 py-3">
			<Card.Header class="flex items-center  justify-between px-4">
				<Card.Title
					class="flex items-center gap-2 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
				>
					<HeartHandshake size={16} />
					Reviews
				</Card.Title>
				<Card.Action>
					<ButtonGroup.Root>
						{#if membership?.role === 'Leader' && session?.state !== 'Inactive'}
							<Button size="sm" variant="outline" onclick={requestReview}>
								Request <Plus />
							</Button>
						{:else if !membership}
							<Button size="sm" variant="outline" onclick={provideReview}>
								Review <TextSearch />
							</Button>
						{/if}
						<Button
							size="sm"
							variant="outline"
							href="/reviews/user/{context.userId()}/project/{context.projectId()}"
						>
							View All <HeartHandshake />
						</Button>
						<Select.Root type="single" bind:value={sort}>
							<Select.Trigger
								icon={sort === 'Ascending' ? SortAsc : SortDesc}
								class={buttonVariants({ variant: 'outline', size: 'sm', class: 'h-6! py-0' })}
							>
								Sort
							</Select.Trigger>
							<Select.Content>
								<Select.Group>
									<Select.Label>Sort Order</Select.Label>
									{#each Order.options as order (order)}
										{@const label = order === 'Descending' ? 'Newest' : 'Oldest'}
										<Select.Item value={order} {label}>
											{label}
										</Select.Item>
									{/each}
								</Select.Group>
							</Select.Content>
						</Select.Root>
					</ButtonGroup.Root>
				</Card.Action>
			</Card.Header>
			<Card.Content class="px-3">
				<Item.Group class="gap-2">
					{#each reviews.data as item (item.id)}
						<Item.Review review={item} />
					{:else}
						<span class="text-xs text-muted-foreground">No Reviews yet.</span>
					{/each}
				</Item.Group>
			</Card.Content>
		</Card.Root>
	</svelte:boundary>
{/if} -->
