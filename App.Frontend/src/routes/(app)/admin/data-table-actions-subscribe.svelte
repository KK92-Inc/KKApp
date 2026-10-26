<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button';
	import type { components } from '$lib/api/api';
	import { Plus, PlusCircle, Snowflake } from '@lucide/svelte';
	import { useDialog } from '$lib/components/dialog';
	import * as Dialog from '$lib/components/dialog';
	import * as Cursus from '$lib/remotes/cursus.remote';
	import * as Goals from '$lib/remotes/goals.remote';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Subscriptions from '$lib/remotes/subscription.remote';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Select from '$lib/components/select';
	import { Problem } from '$lib/api';
	import { toast } from 'svelte-sonner';
	import * as Field from '$lib/components/field';

	interface Props {
		open: boolean;
		user: components['schemas']['UserDO'];
	}

	const dialog = useDialog();
	let { user, open = $bindable(false) }: Props = $props();

	let value = $state<string>('cursus');
	let target = $state<string>();
	let name = $state<string>();
	const entities = [
		{
			label: 'Entities',
			options: [
				{ value: 'cursus', label: 'Cursus' },
				{ value: 'goals', label: 'Goals' },
				{ value: 'projects', label: 'Projects' }
			]
		}
	];

	const items = entities.flatMap((group) => group.options);

	async function submit() {
		open = false;

		const confirm = await dialog.confirm();
		if (!confirm) return;

		await Problem.try(async () => {
			if (value === 'cursus') {
				await Subscriptions.subscribeToCursus({ userId: user.id, cursusId: target! });
			} else if (value === 'goals') {
				await Subscriptions.subscribeToGoal({ userId: user.id, goalId: target! });
			} else if (value === 'projects') {
				await Subscriptions.subscribeToProject({ userId: user.id, projectId: target! });
			}

			toast.success('User has been subscribed!');
		});
	}
</script>

{#snippet cursi()}
	{@const paginated = await Cursus.getPage({ size: 100 })}
	<Select.Root type="single" {items} bind:value={target}>
		<Select.Trigger class="w-full capitalize">{name ?? 'Select...'}</Select.Trigger>
		<Select.Content>
			<Select.Group>
				<Select.Label>Cursi</Select.Label>
				{#each paginated.data as option (option.id)}
					<Select.Item
						value={option.id}
						onclick={() => {
							name = option.name;
						}}>{option.name}</Select.Item
					>
				{/each}
			</Select.Group>
		</Select.Content>
	</Select.Root>
{/snippet}

{#snippet goals()}
	{@const paginated = await Goals.getPage({ size: 100 })}
	<Select.Root type="single" {items} bind:value={target}>
		<Select.Trigger class="w-full capitalize">{name ?? 'Select...'}</Select.Trigger>
		<Select.Content>
			<Select.Group>
				<Select.Label>Goals</Select.Label>
				{#each paginated.data as option (option.id)}
					<Select.Item
						value={option.id}
						onclick={() => {
							name = option.name;
						}}>{option.name}</Select.Item
					>
				{/each}
			</Select.Group>
		</Select.Content>
	</Select.Root>
{/snippet}

{#snippet projects()}
	{@const paginated = await Projects.getPage({ size: 100 })}
	<Select.Root type="single" {items} bind:value={target}>
		<Select.Trigger class="w-full capitalize">{name ?? 'Select...'}</Select.Trigger>
		<Select.Content>
			<Select.Group>
				<Select.Label>Projects</Select.Label>
				{#each paginated.data as option (option.id)}
					<Select.Item
						value={option.id}
						onclick={() => {
							name = option.name;
						}}>{option.name}</Select.Item
					>
				{/each}
			</Select.Group>
		</Select.Content>
	</Select.Root>
{/snippet}

<Dialog.Root bind:open>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Subscribe {user.login} to an entity.</Dialog.Title>
			<Dialog.Description>
				Forcefully subscribe a user to a given entity.
			</Dialog.Description>
		</Dialog.Header>

		<Select.Root type="single" {items} bind:value>
			<Select.Trigger class="w-full capitalize">
				{value}
			</Select.Trigger>
			<Select.Content>
				{#each entities as group (group.label)}
					<Select.Group>
						<Select.Label>{group.label}</Select.Label>
						{#each group.options as option (option.value)}
							<Select.Item value={option.value} label={option.label}>{option.label}</Select.Item>
						{/each}
					</Select.Group>
				{/each}
			</Select.Content>
		</Select.Root>

		<Field.Set>
			<Field.Group class="gap-3">
				<Field.Field>
					<Field.Label for="reason">Entity</Field.Label>
					<svelte:boundary>
						{#if value === 'cursus'}
							{@render cursi()}
						{:else if value === 'goals'}
							{@render goals()}
						{:else if value === 'projects'}
							{@render projects()}
						{/if}
					</svelte:boundary>
				</Field.Field>
			</Field.Group>
		</Field.Set>

		<Separator />
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" onclick={() => submit()}>
				Subscribe <PlusCircle />
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
