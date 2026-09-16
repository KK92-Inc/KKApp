// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createColumnHelper, renderComponent, renderSnippet } from "@tanstack/svelte-table";
import type { DataTableFeatures } from "./data-table-features.js";
import type { components } from "$lib/api/api.js";
import DataTableActions from "./data-table-actions.svelte";
import DataTableStudent from "./data-table-student.svelte";
import { createRawSnippet } from "svelte";

// ============================================================================

// Use `accessor` for data columns and `display` for columns without one.
const helper = createColumnHelper<DataTableFeatures, components['schemas']['UserDO']>();

// ============================================================================

export const columns = helper.columns([
	helper.display({
		header: "Student",
		cell: ({ row }) => {
			return renderComponent(DataTableStudent, { user: row.original });
		},
	}),
	helper.accessor("login", {
		header: "Login",
	}),
	helper.accessor("details.email", {
		header: "Email",
		cell: ({ row }) => {
			return renderSnippet(createRawSnippet(() => ({
				render: () => `<span>${row.original.details?.email ?? '-'}</span>`,
			})));
		},
	}),
	helper.display({
		id: "actions",
		cell: ({ row }) => {
			// You can pass whatever you need from `row.original` to the component
			return renderComponent(DataTableActions, { user: row.original });
		},
	}),
]);
