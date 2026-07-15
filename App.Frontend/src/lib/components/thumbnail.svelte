<script lang="ts">
	import { cn } from '$lib/utils';
	import { Upload, User as UserIcon } from '@lucide/svelte';
	import type { ClassValue } from 'svelte/elements';

	interface Props {
		value?: File | string | null;
		size?: number;
		maxSize?: number;
		allowed?: string[];
		name?: string;
		alt?: string;
		class?: ClassValue;
		readonly?: boolean;
	}

	let {
		value = $bindable(null),
		size = 128,
		maxSize = 5,
		name = 'image',
		alt = 'Preview',
		class: klass,
		readonly = false,
		allowed = ['image/png', 'image/jpeg', 'image/gif']
	}: Props = $props();

	let error = $state<string | null>(null);
	let objectUrl = $state<string | null>(null);

	const maxBytes = $derived(maxSize * 1024 * 1024);

	$effect(() => {
		if (!(value instanceof File)) {
			objectUrl = null;
			return;
		}
		const url = URL.createObjectURL(value);
		objectUrl = url;
		return () => URL.revokeObjectURL(url);
	});

	const src = $derived(value instanceof File ? objectUrl : typeof value === 'string' ? value : null);

	function validate(file: File): string | null {
		if (file.size > maxBytes) return `File too large — max ${maxSize}MB`;
		if (!allowed.includes(file.type)) return 'Unsupported file type';
		return null;
	}

	function onChange(e: Event & { currentTarget: HTMLInputElement }) {
		const file = e.currentTarget.files?.[0];
		if (!file) return;

		const issue = validate(file);
		if (issue) {
			error = issue;
			e.currentTarget.value = '';
			return;
		}

		error = null;
		value = file;
	}
</script>

<div class={cn('inline-flex flex-col gap-1.5', klass)}>
	<label
		class={cn(
			'group relative block shrink-0 overflow-hidden rounded-lg bg-muted',
			readonly ? 'cursor-default' : 'cursor-pointer'
		)}
		style:width={`${size}px`}
		style:height={`${size}px`}
	>
		<input
			type="file"
			{name}
			disabled={readonly}
			accept={allowed.join(',')}
			onchange={onChange}
			class="sr-only"
		/>

		{#if src}
			<img {src} {alt} class="size-full object-cover" />
		{:else}
			<div class="flex size-full items-center justify-center text-muted-foreground">
				<UserIcon class="size-1/2" />
			</div>
		{/if}

		{#if !readonly}
			<div
				class="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 transition-opacity group-focus-within:opacity-100 group-hover:opacity-100"
			>
				<Upload class="size-1/4 text-white" />
			</div>
		{/if}
	</label>

	{#if error}
		<p class="text-xs text-destructive">{error}</p>
	{/if}
</div>
