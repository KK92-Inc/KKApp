<script lang="ts">
	import { cn } from '$lib/utils';
	import { Camera, Upload, User as UserIcon, Image as ImageIcon, X } from '@lucide/svelte';
	import type { ClassValue } from 'svelte/elements';
	import { ALLOWED_IMAGE_TYPES, MAX_IMAGE_MB } from '$lib/s3';
	import { toast } from 'svelte-sonner';
	import { Button } from '$lib/components/button';
	import * as ButtonGroup from '$lib/components/button-group';

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
		/** Also offer the device camera next to the file picker. */
		capture?: boolean;
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
		allowed = ALLOWED_IMAGE_TYPES,
		capture = false
	}: Props = $props();

	let videoEl = $state<HTMLVideoElement | null>(null);
	let stream = $state<MediaStream | null>(null);
	let starting = false;

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

	// Shared by the file picker and the camera so both go through the same checks.
	async function setFromFile(file: File) {
		const issue = validate(file);
		if (issue) {
			toast.error(issue);
			return;
		}

		try {
			value = await readAsDataUrl(file);
		} catch {
			toast.error('Failed to read file');
		}
	}

	async function onChange(e: Event & { currentTarget: HTMLInputElement }) {
		const file = e.currentTarget.files?.[0];
		e.currentTarget.value = '';
		if (file) await setFromFile(file);
	}

	// Release the camera if the component is destroyed while streaming.
	$effect(() => () => stop());
	$effect(() => {
		if (videoEl && stream) videoEl.srcObject = stream;
	});

	async function start() {
		if (starting || stream) return;
		starting = true;
		try {
			stream = await navigator.mediaDevices.getUserMedia({
				video: { width: 400, height: 400, facingMode: 'user' },
				audio: false
			});
		} catch (e) {
			console.error(e);
			toast.error('Could not access camera. Please allow camera permissions.');
		} finally {
			starting = false;
		}
	}

	function stop() {
		stream?.getTracks().forEach((track) => track.stop());
		stream = null;
	}

	async function takePhoto() {
		if (!videoEl) return;
		const { videoWidth: vw, videoHeight: vh, clientWidth: cw, clientHeight: ch } = videoEl;
		if (!vw || !vh) return; // first frame not ready yet

		// Crop to exactly what the preview shows (object-cover), so the saved image matches it.
		const scale = Math.max(cw / vw, ch / vh);
		const sw = cw / scale;
		const sh = ch / scale;

		const canvas = document.createElement('canvas');
		canvas.width = Math.round(sw);
		canvas.height = Math.round(sh);
		canvas
			.getContext('2d')
			?.drawImage(videoEl, (vw - sw) / 2, (vh - sh) / 2, sw, sh, 0, 0, canvas.width, canvas.height);
		stop();

		const jpeg = allowed.includes('image/jpeg');
		const type = jpeg ? 'image/jpeg' : 'image/png';
		const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, type, 0.92));
		if (!blob) {
			toast.error('Failed to capture photo');
			return;
		}

		await setFromFile(new File([blob], jpeg ? 'capture.jpg' : 'capture.png', { type }));
	}
</script>

<div class={cn('inline-flex flex-col gap-1.5', variant === 'cover' && 'w-full', klass)}>
	<label
		class={cn(
			'group relative block overflow-hidden bg-muted',
			variant === 'avatar' ? 'shrink-0 rounded-lg' : 'w-full rounded-t-lg',
			readonly || stream ? 'cursor-default' : 'cursor-pointer'
		)}
		style:width={variant === 'avatar' ? `${size}px` : undefined}
		style:height={`${size}px`}
	>
		<input
			type="file"
			{name}
			disabled={readonly || !!stream}
			accept={allowed.join(',')}
			onchange={onChange}
			class="sr-only"
		/>

		{#if stream}
			<video bind:this={videoEl} autoplay playsinline muted class="size-full -scale-x-100 object-cover"
			></video>
		{:else if value}
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

		{#if !readonly && !stream}
			<div
				class="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 transition-opacity group-focus-within:opacity-100 group-hover:opacity-100"
			>
				<Upload class={cn('text-white', variant === 'cover' ? 'size-8' : 'size-1/4')} />
			</div>
		{/if}
	</label>

	{#if capture && !readonly}
		<ButtonGroup.Root class="w-full">
			{#if stream}
				<Button
					type="button"
					size="sm"
					class="flex-1"
					aria-label="Capture photo"
					title="Capture photo"
					onclick={takePhoto}
				>
					<Camera class="size-4" />
				</Button>
				<Button
					type="button"
					size="sm"
					variant="outline"
					class="flex-1"
					aria-label="Cancel"
					title="Cancel"
					onclick={stop}
				>
					<X class="size-4" />
				</Button>
			{:else}
				<Button
					type="button"
					size="sm"
					variant="outline"
					class="flex-1"
					aria-label="Take photo"
					title="Take photo"
					onclick={start}
				>
					<Camera class="size-4" />
				</Button>
			{/if}
		</ButtonGroup.Root>
	{/if}
</div>
