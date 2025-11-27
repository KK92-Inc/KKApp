<script lang="ts">
	import { cn } from '$lib/utils';
	import { Upload } from '@lucide/svelte';
	import type { Attachment } from 'svelte/attachments';
	import type { ClassValue } from 'svelte/elements';

	interface Props {
		src: string;
		maxSize?: number;
		allowed?: string[];
		name?: string;
		class?: ClassValue;
	}

	const {
		src,
		maxSize = 5,
		name = 'image',
		class: klass,
		allowed = ['image/png', 'image/jpeg', 'image/gif']
	}: Props = $props();

	let input: HTMLInputElement = $state(null!);
	const preview: Attachment<HTMLImageElement> = (node) => {
		if (!input || input.type !== 'file') {
			throw new Error('Invalid input element');
		}

		function onChange(e: Event) {
			if (!e.target || !(e.target instanceof HTMLInputElement) || !e.target.files) {
				return console.error('Failed to show preview');
			}

			const file = e.target.files[0];
			if (!file || file.size > maxSize * 1024 * 1024) {
				e.target.value = '';
				return console.error(`File too large! Maximum size is ${maxSize}MB`);
			}

			if (!allowed.includes(file.type)) {
				e.target.value = '';
				return console.error(`Invalid file type!`);
			}

			const reader = new FileReader();
			reader.addEventListener('load', () => (node.src = reader.result as string));
			reader.readAsDataURL(file);
		}

		// Prep input
		const ctrl = new AbortController();
		input.title = 'Upload a image';
		input.accept = allowed.join(',');
		input.addEventListener('change', onChange, {
			signal: ctrl.signal
		});

		return () => ctrl.abort();
	};
</script>

<div class={cn('group relative max-w-52', klass)}>
	<input
		bind:this={input}
		type="file"
		{name}
		value=""
		class="absolute inset-0 z-10 cursor-pointer opacity-0"
	/>
	<div class="relative">
		<!-- NOTE(W2): avatarUrl, will always be a string here. -->
		<img {@attach preview} alt="logo" class="max-h-52 w-full rounded border object-cover" {src} />
		<div
			class="absolute inset-0 rounded bg-black/50 opacity-0 transition-opacity group-hover:opacity-100"
		>
			<Upload class="absolute inset-0 z-1 m-auto size-8 text-white" />
		</div>
	</div>
</div>
