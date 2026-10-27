<script lang="ts">
	import UserPlus from '@lucide/svelte/icons/user-plus';
	import * as Tooltip from '$lib/components/tooltip';
	import type { components } from '$lib/api/api';
	import { formatDate, formatRelative } from '$lib/datetime';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { Problem } from '$lib/api';
	import * as Item from '../item';
	import Button from '$lib/components/button/button.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import { useDialog } from '../dialog';

	interface Props {
		notification: components['schemas']['NotificationDO'] & {
			data: components['schemas']['NotificationDataProjectInviteData'];
		};
	}

	const dialog = useDialog();
	const { notification }: Props = $props();
</script>

<Item.Root
	variant="outline"
	size="sm"
	class="mb-2 items-start border-blue-200/70 bg-blue-50/40 hover:bg-blue-100/50 dark:border-blue-900/50 dark:bg-blue-950/20 dark:hover:bg-blue-950/35"
>
	<Item.Media
		variant="icon"
		class="border-blue-200 bg-blue-100 text-blue-600 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-400"
	>
		<UserPlus size={16} />
	</Item.Media>

	<Item.Content class="min-w-0 items-start gap-1.5">
		<Item.Title class="w-full justify-between gap-3">
			<span>Project invitation</span>
			<Tooltip.Root>
				<Tooltip.Trigger class="shrink-0 text-xs font-normal text-muted-foreground">
					{formatRelative(notification.createdAt)}
				</Tooltip.Trigger>
				<Tooltip.Content>
					{formatDate(notification.createdAt)}
				</Tooltip.Content>
			</Tooltip.Root>
		</Item.Title>
		<Item.Description>
			You’ve been invited to join a project. Review the project before accepting.
		</Item.Description>
	</Item.Content>

	<Item.Actions class="w-full justify-end sm:w-auto">
		<ButtonGroup.Root>
			<ButtonGroup.Root>
				<Button
					variant="outline"
					size="sm"
					href="/users/{notification.data.inviterUserId}/projects/{notification.data.projectId}"
				>
					View project
				</Button>
			</ButtonGroup.Root>
			<ButtonGroup.Root>
				<Button
					size="sm"
					class="border"
					variant="secondary"
					loading={UserProjects.accept.pending > 0}
					onclick={async () => {
						if (!(await dialog.confirm())) return;
						Problem.try(async () => {
							await UserProjects.accept(notification.data.userProjectId);
						});
					}}
				>
					Accept
				</Button>
				<Button
					variant="destructive"
					size="sm"
					class="border"
					loading={UserProjects.decline.pending > 0}
					onclick={async () => {
						if (!(await dialog.confirm())) return;
						Problem.try(async () => {
							await UserProjects.decline(notification.data.userProjectId);
						});
					}}
				>
					Decline
				</Button>
			</ButtonGroup.Root>
		</ButtonGroup.Root>
	</Item.Actions>
</Item.Root>
