// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from '$lib/api/api.js';
import { createRawSnippet } from 'svelte';
import { createColumnHelper, renderComponent } from '@tanstack/svelte-table';
import { Badge, type BadgeVariant } from '$lib/components/badge';
import type { DataTableFeatures } from '../features';
import { formatDate, hasPassed } from '../format';
import DataTableActions from './data-table-actions.svelte';

// ============================================================================

type Kickoff = components['schemas']['KickoffDO'];

const helper = createColumnHelper<DataTableFeatures, Kickoff>();

/** Where a kickoff is in its life. Past its date but not started means the job hasn't picked it up yet. */
function status(kickoff: Kickoff): { label: string; variant: BadgeVariant } {
	if (kickoff.startedAt) return { label: 'Started', variant: 'success' };
	if (hasPassed(kickoff.startsAt)) return { label: 'Starting', variant: 'secondary' };
	return { label: 'Upcoming', variant: 'outline' };
}

// ============================================================================

export const columns = helper.columns([
	helper.accessor('name', {
		header: 'Name',
		cell: ({ row }) => row.original.name
	}),
	helper.display({
		header: 'Members',
		cell: ({ row }) => `${row.original.members} / ${row.original.capacity}`
	}),
	helper.accessor('startsAt', {
		header: 'Starts',
		cell: ({ row }) => formatDate(row.original.startsAt)
	}),
	helper.display({
		header: 'Status',
		cell: ({ row }) => {
			const { label, variant } = status(row.original);
			// The label is one of our own constants, never API data, so it is safe as markup.
			const children = createRawSnippet(() => ({ render: () => `<span>${label}</span>` }));
			return renderComponent(Badge, { variant, children });
		}
	}),
	helper.display({
		id: 'actions',
		cell: ({ row }) => renderComponent(DataTableActions, { kickoff: row.original })
	})
]);
