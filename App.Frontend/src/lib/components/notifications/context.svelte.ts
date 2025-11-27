// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { getNotifications } from "$lib/remotes/notification.remote";
import { createContext } from "svelte";

// ============================================================================

type Notification = components['schemas']['NotificationDO'];
export class Context<T extends Notification = Notification> {
	public mask = $state(0);
	public page = $state(0);
	public size = $state(20);
	public notifications = $state.raw<T[]>([])
	public selected = $state<T>()

	public async fetch() {
		this.notifications =  await getNotifications({
			page: this.page,
			size: this.size
		})
	}
}

export const [ get, init ] = createContext<Context>();
