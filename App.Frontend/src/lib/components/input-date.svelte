<!-- src/lib/components/datetime-local-input.svelte -->
<script lang="ts">
	import { Input } from '$lib/components/input';
	import { parseAbsolute, parseDateTime, toCalendarDateTime, toZoned } from '@internationalized/date';
	import { page } from '$app/state';
	import type { ComponentProps } from 'svelte';

	interface Props extends Omit<ComponentProps<typeof Input>, 'type' | 'value' | 'files' | 'min' | 'max'> {
		/**
		 * UTC ISO string (e.g. "2026-09-18T09:02:46.459Z").
		 * `undefined` means "nothing picked", never null and never "".
		 */
		value?: string;
		/** Earliest selectable moment, as a UTC ISO string. Optional. */
		min?: string;
		/** Latest selectable moment, as a UTC ISO string. Optional. */
		max?: string;
		/** IANA timezone the value is displayed/interpreted in. Defaults to the instance tz. */
		tz?: string;
	}

	let { value = $bindable(), min, max, tz = page.data.tz, ...rest }: Props = $props();

	function toLocalInputValue(utcIso: string | undefined, timeZone: string): string {
		if (!utcIso) return '';
		try {
			const zoned = parseAbsolute(utcIso, timeZone);
			return toCalendarDateTime(zoned).toString().slice(0, 16); // "YYYY-MM-DDTHH:mm"
		} catch {
			return '';
		}
	}

	function toUtcIso(local: string, timeZone: string): string | undefined {
		if (!local) return undefined;
		try {
			// datetime-local's .value has no seconds; parseDateTime wants them.
			const calendar = parseDateTime(local.length === 16 ? `${local}:00` : local);
			return toZoned(calendar, timeZone).toAbsoluteString();
		} catch {
			return undefined;
		}
	}

	function onInput(e: Event & { currentTarget: HTMLInputElement }) {
		value = toUtcIso(e.currentTarget.value, tz);
	}
</script>

<Input
	type="datetime-local"
	value={toLocalInputValue(value, tz)}
	min={toLocalInputValue(min, tz) || undefined}
	max={toLocalInputValue(max, tz) || undefined}
	oninput={onInput}
	{...rest}
/>
