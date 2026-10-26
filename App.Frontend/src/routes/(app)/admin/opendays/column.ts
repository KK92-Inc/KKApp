// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from '$lib/api/api.js';
import { createRawSnippet } from 'svelte';
import { createColumnHelper, renderComponent } from '@tanstack/svelte-table';
import { Badge, type BadgeVariant } from '$lib/components/badge';
import type { DataTableFeatures } from '../features';
import { formatDate, past } from '$lib/datetime';
import DataTableActions from './data-table-actions.svelte';

// ============================================================================

type OpenDay = components['schemas']['ApplyEventDO'];

const helper = createColumnHelper<DataTableFeatures, OpenDay>();

function status(openDay: OpenDay): { label: string; variant: BadgeVariant } {
	if (past(openDay.startsAt)) return { label: 'Started', variant: 'secondary' };
	if (past(openDay.deadlineAt)) return { label: 'Sign-up closed', variant: 'outline' };
	return { label: 'Open', variant: 'success' };
}

// ============================================================================

export const columns = helper.columns([
	helper.accessor('name', {
		header: 'Name',
		cell: ({ row }) => row.original.name
	}),
	helper.accessor('location', {
		header: 'Location',
		cell: ({ row }) => row.original.location
	}),
	helper.display({
		header: 'Sign-ups',
		cell: ({ row }) => `${row.original.registered} / ${row.original.capacity ?? '∞'}`
	}),
	helper.accessor('attended', {
		header: 'Verified',
		cell: ({ row }) => row.original.attended
	}),
	helper.accessor('startsAt', {
		header: 'Starts',
		cell: ({ row }) => formatDate(row.original.startsAt)
	}),
	helper.accessor('deadlineAt', {
		header: 'Sign-up deadline',
		cell: ({ row }) => formatDate(row.original.deadlineAt)
	}),
	helper.display({
		header: 'Status',
		cell: ({ row }) => {
			const { label, variant } = status(row.original);
			const children = createRawSnippet(() => ({ render: () => `<span>${label}</span>` }));
			return renderComponent(Badge, { variant, children });
		}
	}),
	helper.display({
		id: 'actions',
		cell: ({ row }) => renderComponent(DataTableActions, { openDay: row.original })
	})
]);
