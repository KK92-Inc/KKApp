<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import * as Empty from '$lib/components/empty';
	import * as Pagination from '$lib/components/pagination';
	import { Button } from '$lib/components/button';
	import { FolderOpen, FolderCode, Users } from '@lucide/svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import { getUsers } from '$lib/remotes/user.remote';

	const currentPage = $derived(Number(page.url.searchParams.get('page') ?? 1));
	const users = $derived(await getUsers({ page: currentPage }));

	function getInitials(user: typeof users.data[number]): string {
		const first = user.details?.firstName?.[0] ?? '';
		const last = user.details?.lastName?.[0] ?? '';
		return (first + last).toUpperCase() || user.login[0].toUpperCase();
	}

	function getDisplayName(user: typeof users.data[number]): string {
		const { details } = user;
		if (details?.firstName || details?.lastName) {
			return [details.firstName, details.lastName].filter(Boolean).join(' ');
		}
		return user.displayName ?? user.login;
	}

	function buildPageUrl(p: number): string {
		const params = new URLSearchParams(page.url.searchParams);
		params.set('page', String(p));
		return `?${params}`;
	}
</script>

<svelte:boundary>
	<Layout>
		{#snippet left()}
			<div class="my-4 grid gap-2">
				<Card.Root class="py-0 shadow-none">
					<Card.Content class="flex items-center gap-3 p-3">
						<div class="bg-muted flex size-10 shrink-0 items-center justify-center rounded-md">
							<Users class="text-muted-foreground size-5" />
						</div>
						<div class="min-w-0 flex-1">
							<h1 class="text-sm font-semibold leading-tight">Users</h1>
							<p class="text-muted-foreground text-xs">
								{users.count} user{users.count === 1 ? '' : 's'} total
							</p>
						</div>
					</Card.Content>
				</Card.Root>

				{#if users.pages > 1}
					<Card.Root class="py-0 shadow-none">
						<Card.Content class="p-3">
							<p class="text-muted-foreground mb-2 text-xs font-medium tracking-wide uppercase">
								Page
							</p>
							<p class="text-sm">{currentPage} of {users.pages}</p>
						</Card.Content>
					</Card.Root>
				{/if}
			</div>
		{/snippet}

		{#snippet right()}
			<div class="my-4 grid gap-2">
				<Card.Root class="py-0 shadow-none">
					<Card.Content class="p-0">
						{#if users.data.length > 0}
							<Item.Group>
								{#each users.data as user, index (user.id)}
									<Item.Root
										class="cursor-pointer rounded-none px-3 transition-colors first:rounded-t-xl last:rounded-b-xl hover:bg-accent/50"
										onclick={() => goto(`/users/${user.id}`)}
									>
										<Item.Media>
											<Avatar.Root class="size-9 shrink-0">
												{#if user.avatarUrl}
													<Avatar.Image src={user.avatarUrl} alt={getDisplayName(user)} />
												{/if}
												<Avatar.Fallback class="text-xs font-medium">
													{getInitials(user)}
												</Avatar.Fallback>
											</Avatar.Root>
										</Item.Media>

										<Item.Content>
											<Item.Title class="text-sm font-semibold">
												{getDisplayName(user)}
											</Item.Title>
											<Item.Description class="text-xs">@{user.login}</Item.Description>
										</Item.Content>

										<Item.Actions
											class="gap-1.5"
											onclick={(e) => e.stopPropagation()}
										>
											<Button
												href="/users/{user.id}/projects"
												variant="outline"
												size="sm"
												class="h-7 gap-1.5 text-xs"
											>
												<FolderOpen class="size-3" />
												Projects
											</Button>
										</Item.Actions>
									</Item.Root>

									{#if index !== users.data.length - 1}
										<Item.Separator />
									{/if}
								{/each}
							</Item.Group>
						{:else}
							<div class="p-6">
								<Empty.Root>
									<Empty.Header>
										<Empty.Media variant="icon">
											<FolderCode />
										</Empty.Media>
										<Empty.Title>No users yet</Empty.Title>
										<Empty.Description>There are no users, strange...</Empty.Description>
									</Empty.Header>
								</Empty.Root>
							</div>
						{/if}
					</Card.Content>
				</Card.Root>

				{#if users.pages > 1}
					<div class="flex justify-center">
						<Pagination.Root count={users.count} perPage={users.perPage} page={currentPage}>
							{#snippet children({ pages, currentPage: cp })}
								<Pagination.Content>
									<Pagination.Item>
										<Pagination.PrevButton
											href={cp > 1 ? buildPageUrl(cp - 1) : undefined}
											disabled={cp <= 1}
										/>
									</Pagination.Item>

									{#each pages as p (p.key)}
										{#if p.type === 'ellipsis'}
											<Pagination.Item>
												<Pagination.Ellipsis />
											</Pagination.Item>
										{:else}
											<Pagination.Item>
												<Pagination.Link
													href={buildPageUrl(p.value)}
													page={p}
													isActive={cp === p.value}
												>
													{p.value}
												</Pagination.Link>
											</Pagination.Item>
										{/if}
									{/each}

									<Pagination.Item>
										<Pagination.NextButton
											href={cp < users.pages ? buildPageUrl(cp + 1) : undefined}
											disabled={cp >= users.pages}
										/>
									</Pagination.Item>
								</Pagination.Content>
							{/snippet}
						</Pagination.Root>
					</div>
				{/if}
			</div>
		{/snippet}
	</Layout>
</svelte:boundary>
