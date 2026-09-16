<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import type { components } from '$lib/api/api';

	interface Props {
		user: components['schemas']['UserDO'];
	}

	let { user }: Props = $props();
</script>

<div class="grid grid-cols-[auto_1fr] gap-x-4">
	<Avatar.Root>
		<Avatar.Image src={user.avatarUrl} alt="@{user.login}" />
		<Avatar.Fallback>{user.login.slice(0,2).toUpperCase()}</Avatar.Fallback>
	</Avatar.Root>

	<div>
		<a class="font-medium hover:underline" href="/users/174824">
			{#if user.details?.firstName && user.details.lastName}
				{user.details?.firstName} {user.details?.lastName}
			{:else}
				{user.displayName ?? user.login}
			{/if}
		</a>
		<p class="text-xs text-muted-foreground">@{user.login}</p>
	</div>
</div>

