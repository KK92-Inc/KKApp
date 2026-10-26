<script lang="ts">
	import { Check, Search, Trash2, Undo2 } from '@lucide/svelte';
	import { refreshAll } from '$app/navigation';
	import { Button } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import * as Dialog from '$lib/components/dialog';
	import { useDialog } from '$lib/components/dialog';
	import * as InputGroup from '$lib/components/input-group';
	import * as Table from '$lib/components/table';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Problem, type Paginated } from '$lib/api';
	import type { components } from '$lib/api/api';
	import { formatDate } from '$lib/datetime';

	type Registration = components['schemas']['ApplyRegistrationDO'];

	interface Props {
		open: boolean;
		title: string;
		load: (params: { email: string; page: number; size: number }) => PromiseLike<Paginated<Registration>>;
		verify: (registration: Registration, attended: boolean) => Promise<unknown>;
		remove: (registration: Registration) => Promise<unknown>;
	}

	let { open = $bindable(false), title, load, verify, remove }: Props = $props();

	const dialog = useDialog();

	let email = $state('');
	let page = $state(0);
	const size = 10;

	// Start over every time the dialog opens.
	$effect(() => {
		if (!open) return;
		email = '';
		page = 0;
	});

	async function toggle(registration: Registration) {
		const done = await Problem.try(async () => {
			await verify(registration, !registration.attendedAt);
			return true;
		});
		if (done) await refreshAll({ includeLoadFunctions: false });
	}

	async function unregister(registration: Registration) {
		await dialog
			.confirm(`Remove '${registration.email}'`, 'This gives their seat back. Someone who was verified as present cannot be removed.')
			.ok(async () => {
				const done = await Problem.try(async () => {
					await remove(registration);
					return true;
				});
				if (done) await refreshAll({ includeLoadFunctions: false });
			});
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-3xl">
		<Dialog.Header>
			<Dialog.Title>Attendees of {title}</Dialog.Title>
			<Dialog.Description>
				Verify only people you can see in person. The time and who verified it are recorded and can't be changed by the applicant.
			</Dialog.Description>
		</Dialog.Header>

		<InputGroup.Root>
			<InputGroup.Input
				value={email}
				placeholder="Find by email..."
				oninput={(e) => {
					email = e.currentTarget.value;
					page = 0;
				}}
			/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		{#if open}
			<svelte:boundary>
				{@const registrations = await load({ email, page, size })}

				{#snippet pending()}
					<Skeleton class="h-40" />
				{/snippet}

				{#snippet failed()}
					<div class="p-4 text-red-500">Error loading attendees</div>
				{/snippet}

				<div class="rounded-md border">
					<Table.Root>
						<Table.Header>
							<Table.Row>
								<Table.Head>Email</Table.Head>
								<Table.Head>Name</Table.Head>
								<Table.Head>Status</Table.Head>
								<Table.Head align="right"></Table.Head>
							</Table.Row>
						</Table.Header>
						<Table.Body>
							{#each registrations.data as registration (registration.id)}
								<Table.Row>
									<Table.Cell>{registration.email}</Table.Cell>
									<Table.Cell>
										{[registration.firstName, registration.lastName].filter(Boolean).join(' ') || '-'}
									</Table.Cell>
									<Table.Cell>
										{#if registration.attendedAt}
											<Badge variant="success">Verified {formatDate(registration.attendedAt)}</Badge>
										{:else}
											<Badge variant="outline">Signed up</Badge>
										{/if}
									</Table.Cell>
									<Table.Cell align="right">
										<div class="flex justify-end gap-1">
											<Button
												size="sm"
												variant={registration.attendedAt ? 'outline' : 'default'}
												onclick={() => toggle(registration)}
											>
												{#if registration.attendedAt}
													<Undo2 />
													Undo
												{:else}
													<Check />
													Verify
												{/if}
											</Button>
											<Button
												size="icon"
												variant="ghost"
												class="size-8"
												disabled={!!registration.attendedAt}
												onclick={() => unregister(registration)}
											>
												<span class="sr-only">Remove</span>
												<Trash2 />
											</Button>
										</div>
									</Table.Cell>
								</Table.Row>
							{:else}
								<Table.Row>
									<Table.Cell colspan={4} class="h-24 text-center">Nobody signed up.</Table.Cell>
								</Table.Row>
							{/each}
						</Table.Body>
					</Table.Root>
				</div>

				<div class="flex items-center justify-end gap-2">
					<div class="text-sm text-muted-foreground">
						Page {page + 1} of {Math.max(registrations.pages, 1)}
					</div>
					<Button variant="outline" size="sm" disabled={page <= 0} onclick={() => page--}>Previous</Button>
					<Button
						variant="outline"
						size="sm"
						disabled={page + 1 >= registrations.pages}
						onclick={() => page++}
					>
						Next
					</Button>
				</div>
			</svelte:boundary>
		{/if}
	</Dialog.Content>
</Dialog.Root>
