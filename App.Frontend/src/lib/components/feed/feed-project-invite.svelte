<script lang="ts">
import UserPlus from '@lucide/svelte/icons/user-plus';
import * as Tooltip from '$lib/components/tooltip';
import { Badge } from '$lib/components/badge';
import type { components } from '$lib/api/api';
// import { getLocalTimeZone, parseAbsolute } from '@internationalized/date';
	// import { page } from '$app/state';

interface Props {
  notification: components["schemas"]["NotificationDO"] & {
    data: components["schemas"]["NotificationDataProjectInviteData"]
  }
}

const { notification }: Props = $props();
// const createdDate = $derived(parseAbsolute(notification.createdAt, page.data.tz));
// const formattedDate = $derived()
// const formattedDate = formatDateTime(createdDate, { timeZone: getLocalTimeZone(), dateStyle: 'medium', timeStyle: 'short' });
</script>

<div class="mb-3 rounded-lg border bg-card p-4 transition-colors hover:bg-accent/50">
  <div class="flex items-start gap-3">
    <div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-blue-100 text-blue-600 dark:bg-blue-950 dark:text-blue-400">
      <UserPlus class="size-4" />
    </div>
    <div class="min-w-0 flex-1">
      <div class="flex items-center justify-between gap-2">
        <Badge class="border-0 bg-blue-500 text-xs text-white dark:bg-blue-700">
          <UserPlus class="size-3" />
          Project Invite
        </Badge>
        <Tooltip.Root>
          <Tooltip.Trigger class="text-muted-foreground text-xs">
            <!-- {formattedDate} -->
          </Tooltip.Trigger>
          <Tooltip.Content>
            <!-- <p>{formattedDate}</p> -->
          </Tooltip.Content>
        </Tooltip.Root>
      </div>
      <div class="mt-2 text-sm">
        You have been invited to a project by user <span class="font-semibold">{notification.data.inviterUserId}</span>.
      </div>
    </div>
  </div>
</div>
