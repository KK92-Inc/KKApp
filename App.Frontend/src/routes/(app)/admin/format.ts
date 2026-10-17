// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as Date from '@internationalized/date';
import { page } from '$app/state';

// Note: the namespace import above shadows the global `Date` in this file on
// purpose, everything about dates goes through the library.

// ============================================================================

/** Moments are compared in UTC, the instance's timezone only matters when showing them. */
const UTC = 'UTC';

/** An ISO string from the API, with whatever offset it came with, as an absolute moment. */
const parse = (iso: string) => Date.parseAbsolute(iso, UTC);

let formatter: { key: string; instance: Date.DateFormatter } | undefined;

/** A UTC ISO string in the instance's locale and timezone, same as the date inputs use. */
export function formatDate(iso?: string | null): string {
	if (!iso) return '-';

	const { locale, tz } = page.data;
	if (formatter?.key !== `${locale}|${tz}`) {
		formatter = {
			key: `${locale}|${tz}`,
			instance: new Date.DateFormatter(locale, { dateStyle: 'medium', timeStyle: 'short', timeZone: tz })
		};
	}

	return formatter.instance.format(parse(iso).toDate());
}

/** Whether a moment is behind us (or right now). */
export function hasPassed(iso: string): boolean {
	return parse(iso).compare(Date.now(UTC)) <= 0;
}

/** Whether two ISO strings are the same moment, regardless of how they are written. */
export function sameMoment(a?: string | null, b?: string | null): boolean {
	if (!a || !b) return !a && !b;
	return parse(a).compare(parse(b)) === 0;
}

const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
	['year', 31_536_000],
	['month', 2_592_000],
	['day', 86_400],
	['hour', 3_600],
	['minute', 60]
];

/** "in 3 days", "yesterday", "15 days ago"... relative to now, in the instance's locale. */
export function formatRelative(iso: string): string {
	// The library has no relative formatter, so only the distance comes from it.
	const seconds = (parse(iso).toDate().getTime() - Date.now(UTC).toDate().getTime()) / 1000;
	const relative = new Intl.RelativeTimeFormat(page.data.locale, { numeric: 'auto' });

	for (const [unit, size] of UNITS) {
		if (Math.abs(seconds) >= size) return relative.format(Math.round(seconds / size), unit);
	}

	return relative.format(Math.round(seconds), 'second');
}
