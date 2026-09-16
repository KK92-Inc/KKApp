<script lang="ts">
	import EllipsisIcon from '@lucide/svelte/icons/ellipsis';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import type { components } from '$lib/api/api';
	import { Hammer, HatGlasses, IdCard, Snowflake, User } from '@lucide/svelte';
	interface Props {
		user: components['schemas']['UserDO'];
	}

	let { user }: Props = $props();
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger>
		{#snippet child({ props })}
			<Button {...props} variant="ghost" size="icon" class="relative size-8 p-0">
				<span class="sr-only">Open menu</span>
				<EllipsisIcon />
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content>
		<DropdownMenu.Group>
			<DropdownMenu.Label>Account</DropdownMenu.Label>
			<DropdownMenu.Item onclick={() => navigator.clipboard.writeText(user.id)}>
				<IdCard />
				Copy User ID
			</DropdownMenu.Item>
		<DropdownMenu.Separator />

			<DropdownMenu.Item href="/users/{user.id}">
				<User />
				View Profile
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Sub>
				<DropdownMenu.SubTrigger>
					<Hammer />
					Actions
				</DropdownMenu.SubTrigger>
				<DropdownMenu.SubContent>
					<DropdownMenu.Item variant="destructive">
						<Snowflake />
						Freeze
					</DropdownMenu.Item>
					<DropdownMenu.Item variant="destructive">
						<HatGlasses />
						Anonymize
					</DropdownMenu.Item>
				</DropdownMenu.SubContent>
			</DropdownMenu.Sub>
		</DropdownMenu.Group>
	</DropdownMenu.Content>
</DropdownMenu.Root>
