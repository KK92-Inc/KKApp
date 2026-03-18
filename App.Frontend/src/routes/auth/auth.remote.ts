// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Keycloak } from '$lib/auth';
import { form } from '$app/server';

// ============================================================================

/** Remote to sign-in */
export const login = form(() => Keycloak.signIn());
/** Remote to sign-out */
export const logout = form(async () => await Keycloak.signOut());
