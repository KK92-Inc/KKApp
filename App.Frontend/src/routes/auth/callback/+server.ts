// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Keycloak } from "$lib/auth";
import type { RequestHandler } from './$types';

// ============================================================================

export const GET: RequestHandler = Keycloak.callback;
