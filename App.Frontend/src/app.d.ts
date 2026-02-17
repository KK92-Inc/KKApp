// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces

import type { Client } from 'openapi-fetch';
import type { Session } from '$lib/oauth';
import type { paths } from '$lib/api/api';

declare global {
	namespace App {
		// interface Error {}
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
		// interface PageState {}
		// interface Platform {}
	}
}

export {};
