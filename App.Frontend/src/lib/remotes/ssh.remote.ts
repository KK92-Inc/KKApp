// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';

// ============================================================================
// Get
// ============================================================================

export const get = Remote.GET('/account/ssh-keys').declare();

// ============================================================================
// Create
// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });
export const create = Remote.POST('/account/ssh-keys')
	.extend(addSchema, data => ({ body: data }))
	.after(() => get({}).refresh())
	.declare();

// ============================================================================
// Delete
// ============================================================================

export const remove = Remote.DELETE('/account/ssh-keys/{fingerprint}')
	.after(() => get({}).refresh())
	.declare();
