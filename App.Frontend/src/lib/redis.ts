// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { VALKEY_HOST, VALKEY_PASSWORD, VALKEY_PORT } from './config';
import { building } from '$app/environment';

// ============================================================================

export const redis = building
	? new Bun.RedisClient()
	: new Bun.RedisClient(`redis://:${VALKEY_PASSWORD}@${VALKEY_HOST}:${VALKEY_PORT}`);
