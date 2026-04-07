// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { Remote } from "./index.svelte";

// ============================================================================

export const branches = Remote.GET('/git/{id}/branches').declare('text');
export const tree = Remote.GET('/git/{id}/tree/{branch}/{path}').declare('text');
export const blob = Remote.GET('/git/{id}/blob/{branch}/{path}').declare('text');

export const treeViaUser = Remote.GET('/users/{id}/projects/{projectId}/tree/{branch}/{path}').declare('text');
export const blobViaUser = Remote.GET('/users/{id}/projects/{projectId}/blob/{branch}/{path}').declare('text');

export const createBranch = Remote.POST('/git/{id}/branches/{ref}/{child}')
	.required(false)
	.declare();

export const deleteBranch = Remote.DELETE('/git/{id}/branches/{branch}')
	.required(false)
	.declare();
