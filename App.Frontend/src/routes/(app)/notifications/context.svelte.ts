// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import * as Account from "$lib/remotes/account.remote";
import { createContext } from "svelte";

// ============================================================================

type Notification = components['schemas']['NotificationDO'];
export class Context<T extends Notification = Notification> {
	public mask = $state(0);
	public page = $state(1);
	public size = $state(20);
	public notifications = $state.raw<T[]>([])
	public selected = $state<T>()

	public async hydrate() {

	}
}

export const [ get, init ] = createContext<Context>();
