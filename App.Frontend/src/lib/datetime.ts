// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Thin wrapper for handling, parsing, and readin ISO 8601 dates.
// ============================================================================

import {
	DateFormatter,
	now,
	parseAbsolute,
} from '@internationalized/date';
import { page } from '$app/state';

// ============================================================================

const UTC = 'UTC';
export function parse(value: string) {
	return parseAbsolute(value, UTC);
}
export function nowUTC() {
	return now(UTC).toAbsoluteString();
}
export function past(value: string) {
	return parse(value).compare(now(UTC)) <= 0;
}
export function future(iso: string) {
	return !past(iso);
}
export function equal(a?: string | null, b?: string | null) {
	if (!a || !b) return !a && !b;
	return parse(a).compare(parse(b)) === 0;
}

// ============================================================================
// Formatting
// ============================================================================

const STYLES = {
	/** Jan 5, 2026, 2:30 PM */
	datetime: { dateStyle: 'medium', timeStyle: 'short' },
	/** January 5, 2026 at 2:30 PM */
	datetimeLong: { dateStyle: 'long', timeStyle: 'short' },
	/** Jan 5, 2026 */
	date: { dateStyle: 'medium' },
	/** January 5, 2026 */
	dateLong: { dateStyle: 'long' },
	/** 2:30 PM */
	time: { timeStyle: 'short' }
} as const satisfies Record<string, Intl.DateTimeFormatOptions>;

export type DateStyle = keyof typeof STYLES;

const formatters = new Map<string, DateFormatter>();
function formatterFor(style: DateStyle): DateFormatter {
	const { locale, tz } = page.data;
	const key = `${locale}|${tz}|${style}`;

	let formatter = formatters.get(key);
	if (!formatter) {
		formatter = new DateFormatter(locale, { ...STYLES[style], timeZone: tz });
		formatters.set(key, formatter);
	}

	return formatter;
}

// ============================================================================

/**
 * A moment in the instance's locale and timezone.
 * Missing values come out as `fallback`, so callers don't need their own guard.
 * @param value The date-string to format.
 * @param style The style of formatting
 * @param fallback What to provide as fallback.
 * @returns Either fallback or the formatted date-string.
 */
export function formatDate(value?: string | null, style: DateStyle = 'datetime', fallback = '-') {
	if (!value) return fallback;
	return formatterFor(style).format(parse(value).toDate());
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

	// The library has no relative formatter, so only the distance (in ms) comes from it.
	const seconds = parse(iso).compare(now(UTC)) / 1000;
	const relative = new Intl.RelativeTimeFormat(page.data.locale, { numeric: 'auto' });

	for (const [unit, size] of UNITS) {
		if (Math.abs(seconds) >= size)
			return relative.format(Math.round(seconds / size), unit);
	}

	return relative.format(Math.round(seconds), 'second');
}
