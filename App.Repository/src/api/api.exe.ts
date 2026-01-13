// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import type { BunRequest } from "bun";

// ============================================================================

Bun.serve({
	routes: {
		"/repo/add": (req) => {
			return new Response("Repo add");
		},
		"/repo/:name/delete": (req) => {
			return new Response("Repo delete");
		},
		"/repo/:old/rename/:new": (req) => {
			return new Response("Repo rename");
		},
		"/repo/:name/tree/:branch/*": (req) => {
			return new Response(
				`Repo tree for branch: ${req.params.branch}, path: ${req.params["*"]}`
			);
		},
		"/repo/:name/blob/:branch/*": (req) => {
			return new Response(
				`Repo blob for branch: ${req.params.branch}, path: ${req.params["*"]}`
			);
		},
	},
});
