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
import CursusName from './cursus-name.svelte';
import DataTableActions from './data-table-actions.svelte';

// ============================================================================

type Trial = components['schemas']['TrialDO'];

const helper = createColumnHelper<DataTableFeatures, Trial>();

/** Where a trial is in its life. A passed date without the matching timestamp means the job hasn't caught up yet. */
function status(trial: Trial): { label: string; variant: BadgeVariant } {
	if (trial.endedAt) return { label: 'Ended', variant: 'secondary' };
	if (trial.startedAt) {
		return past(trial.endsAt)
			? { label: 'Ending', variant: 'secondary' }
			: { label: 'Running', variant: 'success' };
	}
	return past(trial.startsAt)
		? { label: 'Starting', variant: 'secondary' }
		: { label: 'Upcoming', variant: 'outline' };
}

// ============================================================================

export const columns = helper.columns([
	helper.accessor('name', {
		header: 'Name',
		cell: ({ row }) => row.original.name
	}),
	helper.display({
		header: 'Cursus',
		cell: ({ row }) => renderComponent(CursusName, { id: row.original.cursusId })
	}),
	helper.display({
		header: 'Participants',
		cell: ({ row }) => `${row.original.participants} / ${row.original.capacity}`
	}),
	helper.accessor('startsAt', {
		header: 'Starts',
		cell: ({ row }) => formatDate(row.original.startsAt)
	}),
	helper.accessor('endsAt', {
		header: 'Ends',
		cell: ({ row }) => formatDate(row.original.endsAt)
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
		cell: ({ row }) => renderComponent(DataTableActions, { trial: row.original })
	})
]);
