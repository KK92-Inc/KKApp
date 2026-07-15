// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { S3Client } from "bun";

// ============================================================================

export const avatars = new S3Client({ bucket: "avatars" });
