// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createColumnHelper, renderComponent } from '@tanstack/svelte-table';
import type { DataTableFeatures } from '../features';
import { formatDate } from '../format';
import DataTableStudent from '../data-table-student.svelte';
import DataTableActions from './data-table-actions.svelte';
import FreezeStatus from './freeze-status.svelte';
import type { Freeze } from './state';

// ============================================================================

const helper = createColumnHelper<DataTableFeatures, Freeze>();

/** The full reason lives in the dialog, the table only needs enough to recognise it. */
function excerpt(reason: string, max = 60): string {
	const flat = reason.replace(/\s+/g, ' ').trim();
	return flat.length > max ? `${flat.slice(0, max - 1)}...` : flat;
}

// ============================================================================

export const columns = helper.columns([
	helper.display({
		header: 'Student',
		cell: ({ row }) => renderComponent(DataTableStudent, { user: row.original.user })
	}),
	helper.display({
		header: 'Status',
		cell: ({ row }) => renderComponent(FreezeStatus, { freeze: row.original })
	}),
	helper.accessor('startsAt', {
		header: 'From',
		cell: ({ row }) => formatDate(row.original.startsAt)
	}),
	helper.accessor('endsAt', {
		header: 'Until',
		cell: ({ row }) => formatDate(row.original.endsAt)
	}),
	helper.accessor('reason', {
		header: 'Reason',
		cell: ({ row }) => excerpt(row.original.reason)
	}),
	helper.display({
		id: 'actions',
		cell: ({ row }) => renderComponent(DataTableActions, { freeze: row.original })
	})
]);
