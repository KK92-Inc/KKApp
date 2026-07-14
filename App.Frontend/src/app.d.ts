// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
// ============================================================================

import type { Session } from '$lib/auth';
import type { Client } from 'openapi-fetch';
import type { paths } from '$lib/api/api';

declare global {
	namespace App {
		interface Locals {
			api: Client<paths>;
			session: Session;
			tz: string;
			locale: string;
		}
		interface PageData {
			session: Session;
			tz: string;
			locale: string;
		}
		interface Error {
			message: string;
			status: number;
			errors?: Record<string, string[]>;
		}
		// interface PageState {}
		// interface Platform {}
		// interface Error {}
	}
}

export { };
