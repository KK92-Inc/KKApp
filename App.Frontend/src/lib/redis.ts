// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { RedisClient } from "bun";
import { VALKEY_HOST, VALKEY_PASSWORD, VALKEY_PORT } from "./config";

// ============================================================================

export const redis = new RedisClient(`valkey://:${VALKEY_PASSWORD}@${VALKEY_HOST}:${VALKEY_PORT}`);
