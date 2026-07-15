<script lang="ts">
	import { cn } from '$lib/utils';
	import { Upload } from '@lucide/svelte';
	import type { ClassValue } from 'svelte/elements';

	interface Props {
		value?: File | string | null;
		maxSize?: number;
		allowed?: string[];
		name?: string;
		class?: ClassValue;
		readonly?: boolean;
	}

	let {
		value = $bindable(null),
		maxSize = 5,
		name = 'image',
		class: klass,
		readonly = false,
		allowed = ['image/png', 'image/jpeg', 'image/gif']
	}: Props = $props();

	let error = $state<string | null>(null);
	let objectUrl = $state<string | null>(null);

	$effect(() => {
		if (value instanceof File) {
			const url = URL.createObjectURL(value);
			objectUrl = url;
			return () => URL.revokeObjectURL(url);
		}
		objectUrl = null;
	});

	const displaySrc = $derived(
		value instanceof File ? objectUrl : (typeof value === 'string' ? value : null)
	);

	function onChange(e: Event) {
		error = null;
		const target = e.target as HTMLInputElement;
		const file = target.files?.[0];
		if (!file) return;

		if (file.size > maxSize * 1024 * 1024) {
			target.value = '';
			error = `File too large! Maximum size is ${maxSize}MB`;
			return;
		}
		if (!allowed.includes(file.type)) {
			target.value = '';
			error = 'Invalid file type!';
			return;
		}

		value = file; // this is the bit that was missing — push it back up to the parent
	}
</script>

<div class={cn('group relative max-w-52', klass)}>
	<input
		type="file"
		{name}
		{readonly}
		disabled={readonly}
		accept={allowed.join()}
		onchange={onChange}
		class="absolute inset-0 z-10 cursor-pointer opacity-0"
	/>
	<div class="relative">
		<img
			alt="logo"
			class="max-h-52 w-full rounded object-cover"
			src={displaySrc ?? 'https://placehold.co/400'}
		/>
		{#if !readonly}
			<div class="absolute inset-0 rounded bg-black/50 opacity-0 transition-opacity group-hover:opacity-100">
				<Upload class="absolute inset-0 z-1 m-auto size-8 text-white" />
			</div>
		{/if}
	</div>
	{#if error}
		<p class="mt-1 text-xs text-destructive">{error}</p>
	{/if}
</div>
