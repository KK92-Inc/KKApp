<!-- src/lib/components/datetime-local-input.svelte -->
<script lang="ts">
	import { Input } from '$lib/components/input';
	import { parseAbsolute, parseDateTime, toCalendarDateTime, toZoned } from '@internationalized/date';
	import { page } from '$app/state';
	import type { ComponentProps } from 'svelte';

	interface Props extends Omit<ComponentProps<typeof Input>, 'type' | 'value' | 'files'> {
		/** UTC ISO string (e.g. "2026-09-18T09:02:46.459Z"), or null. */
		value?: string | null;
		/** IANA timezone the value is displayed/interpreted in. Defaults to the instance tz. */
		tz?: string;
	}

	let { value = $bindable(null), tz = page.data.tz, ...rest }: Props = $props();

	function toLocalInputValue(utcIso: string | null | undefined, timeZone: string): string {
		if (!utcIso) return '';
		try {
			const zoned = parseAbsolute(utcIso, timeZone);
			return toCalendarDateTime(zoned).toString().slice(0, 16); // "YYYY-MM-DDTHH:mm"
		} catch {
			return '';
		}
	}

	function toUtcIso(local: string, timeZone: string): string | null {
		if (!local) return null;
		try {
			// datetime-local's .value has no seconds; parseDateTime wants them.
			const calendar = parseDateTime(local.length === 16 ? `${local}:00` : local);
			return toZoned(calendar, timeZone).toAbsoluteString();
		} catch {
			return null;
		}
	}

	// Re-synced whenever the underlying UTC value or tz changes from outside
	// (e.g. context.hydrate() loading fresh data from the server).
	let local = $derived(toLocalInputValue(value, tz));
	$effect(() => { local = toLocalInputValue(value, tz) });

	function onInput(e: Event & { currentTarget: HTMLInputElement }) {
		local = e.currentTarget.value;
		value = toUtcIso(local, tz);
	}
</script>

<Input type="datetime-local" value={local} oninput={onInput} {...rest} />
