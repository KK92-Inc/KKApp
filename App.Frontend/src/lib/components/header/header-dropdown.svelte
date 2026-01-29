<script lang="ts">
		import {
		Bell,
		CircleQuestionMark,
		Cog,
		Ellipsis,
		LogOut,
		Rocket,
	} from '@lucide/svelte';
	import { Button } from '../button';
	import * as Avatar from '$lib/components/avatar/';
	import * as ButtonGroup from '$lib/components/button-group/';
	import * as DropdownMenu from '$lib/components/dropdown-menu/';
	import { page } from '$app/state';
	import { logout } from '../../../routes/auth/auth.remote';

</script>

<ButtonGroup.Root>
	<Button variant="outline" href="/users/{page.data.session.userId}">
		<Avatar.Root class="size-6">
			<Avatar.Image src="https://github.com/w2wizard.png" alt="@evilrabbit" />
			<Avatar.Fallback>ER</Avatar.Fallback>
		</Avatar.Root>
		Account
	</Button>
	<DropdownMenu.Root>
		<DropdownMenu.Trigger>
			{#snippet child({ props })}
				<Button variant="outline" size="icon" aria-label="More Options" {...props}>
					<Ellipsis />
				</Button>
			{/snippet}
		</DropdownMenu.Trigger>
		<DropdownMenu.Content align="end" class="w-52">
			<DropdownMenu.Group>
				<DropdownMenu.Item href="/settings">
					<Cog />
					Settings
				</DropdownMenu.Item>
				<DropdownMenu.Item href="/notifications">
					<Bell />
					Notifications
				</DropdownMenu.Item>
			</DropdownMenu.Group>
			<DropdownMenu.Separator />
			<DropdownMenu.Group>
				<DropdownMenu.Item href="/">
					<CircleQuestionMark />
					FAQ
				</DropdownMenu.Item>
				{#if page.data.session.roles.includes('staff')}
					<DropdownMenu.Item href="/admin">
						<Rocket />
						Dashboard
					</DropdownMenu.Item>
				{/if}
			</DropdownMenu.Group>
			<DropdownMenu.Separator />
			<DropdownMenu.Group>
				<DropdownMenu.Item remote={logout} variant="destructive">
					<LogOut />
					Logout
				</DropdownMenu.Item>
			</DropdownMenu.Group>
		</DropdownMenu.Content>
	</DropdownMenu.Root>
</ButtonGroup.Root>
