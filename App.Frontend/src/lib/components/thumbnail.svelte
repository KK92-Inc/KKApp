<script lang="ts">
	import { cn } from '$lib/utils';
	import { Upload, User as UserIcon, Image as ImageIcon } from '@lucide/svelte';
	import type { ClassValue } from 'svelte/elements';
	import { ALLOWED_IMAGE_TYPES, MAX_IMAGE_MB } from '$lib/s3';
	import { toast } from 'svelte-sonner';

	interface Props {
		value?: string | null;
		variant?: 'avatar' | 'cover';
		size?: number;
		maxSize?: number;
		allowed?: readonly string[];
		name?: string;
		alt?: string;
		class?: ClassValue;
		readonly?: boolean;
	}

	let {
		value = $bindable(null),
		variant = 'avatar',
		size = 128,
		maxSize = MAX_IMAGE_MB,
		name = 'image',
		alt = 'Preview',
		class: klass,
		readonly = false,
		allowed = ALLOWED_IMAGE_TYPES
	}: Props = $props();

	const maxBytes = $derived(maxSize * 1024 * 1024);
	function validate(file: File): string | null {
		if (file.size > maxBytes) return `File too large, max ${maxSize}MB`;
		if (!allowed.includes(file.type)) return 'Unsupported file type';
		return null;
	}

	function readAsDataUrl(file: File): Promise<string> {
		return new Promise((resolve, reject) => {
			const reader = new FileReader();
			reader.onload = () => resolve(reader.result as string);
			reader.onerror = () => reject(reader.error ?? new Error('Failed to read file'));
			reader.readAsDataURL(file);
		});
	}

	async function onChange(e: Event & { currentTarget: HTMLInputElement }) {
		const file = e.currentTarget.files?.[0];
		e.currentTarget.value = '';
		if (!file) return;

		const issue = validate(file);
		if (issue) {
			toast.error(issue)
			return;
		}

		try {
			value = await readAsDataUrl(file);
		} catch {
			toast.error('Failed to read file');
		}
	}
</script>

<div class={cn('inline-flex flex-col gap-1.5', variant === 'cover' && 'w-full', klass)}>
	<label
		class={cn(
			'group relative block overflow-hidden bg-muted',
			variant === 'avatar' ? 'shrink-0 rounded-lg' : 'w-full rounded-t-lg',
			readonly ? 'cursor-default' : 'cursor-pointer'
		)}
		style:width={variant === 'avatar' ? `${size}px` : undefined}
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

		{#if value}
			<img src={value} {alt} class="size-full object-cover" />
		{:else}
			<div class="flex size-full items-center justify-center text-muted-foreground">
				{#if variant === 'cover'}
					<ImageIcon class="size-10" />
				{:else}
					<UserIcon class="size-1/2" />
				{/if}
			</div>
		{/if}

		{#if !readonly}
			<div
				class="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 transition-opacity group-focus-within:opacity-100 group-hover:opacity-100"
			>
				<Upload class={cn('text-white', variant === 'cover' ? 'size-8' : 'size-1/4')} />
			</div>
		{/if}
	</label>
</div>
