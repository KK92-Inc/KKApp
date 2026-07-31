<script lang="ts">
	import type { Snippet } from 'svelte';
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as Tooltip from '$lib/components/tooltip';
	import { CalendarDays } from '@lucide/svelte';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';

	interface Props {
		user: components['schemas']['UserDO'];
		actions?: Snippet;
	}

	let { user, actions }: Props = $props();

	const formatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric'
	});
</script>

<Item.Root variant="outline">
	{#snippet child({ props })}
		<a
			href="/users/{user.id}"
			{...props}
			class="grid rounded border transition-all hover:border-ring hover:ring-2 hover:ring-ring/50"
		>
			<Avatar.Root class="h-40 w-full rounded-none border-b">
				<Avatar.Image src={user.avatarUrl} alt={user.login} class="object-cover" />
				<Avatar.Fallback class="min-w-40 rounded-none text-xl font-medium">
					{user.displayName?.slice(0, 2)}
				</Avatar.Fallback>
			</Avatar.Root>

			<Item.Content class="border-b p-2">
				<Item.Title class="text-md items-center font-semibold">
					{user.displayName} <span class="text-xs text-muted-foreground">@{user.login}</span>
				</Item.Title>
				<Item.Description class="text-xs">
					<Tooltip.Root delayDuration={100}>
						<Tooltip.Trigger class="flex items-center text-xs text-muted-foreground">
							<CalendarDays class="me-1.5 size-3.5 opacity-70" />
							<span>{formatter.format(new Date(user.createdAt))}</span>
						</Tooltip.Trigger>
						<Tooltip.Content>
							<p>User joined {formatter.format(new Date(user.createdAt))}</p>
						</Tooltip.Content>
					</Tooltip.Root>
				</Item.Description>
			</Item.Content>

			{#if actions}
				<Item.Actions class="p-2">
					{@render actions()}
				</Item.Actions>
			{/if}
		</a>
	{/snippet}
</Item.Root>
