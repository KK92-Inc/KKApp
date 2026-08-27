<script lang="ts">
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import * as Avatar from '$lib/components/avatar';
	import { Camera, RefreshCw, Trash2, X } from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import * as Tabs from '$lib/components/tabs';
	import * as ButtonGroup from '$lib/components/button-group';
	import type { PageProps } from './$types';

	let { data }: PageProps = $props();

	// --- avatar capture state ---
	let error = $state<string | null>(null);
	let avatar = $state<string | null>(null);
	let stream = $state<MediaStream | null>(null);
	let videoEl = $state<HTMLVideoElement | null>(null);

	$effect(() => {
		if (videoEl && stream) {
			videoEl.srcObject = stream;
		}
	});

	async function start() {
		error = null;
		try {
			stream = await navigator.mediaDevices.getUserMedia({
				video: { width: 400, height: 400, facingMode: 'user' },
				audio: false
			});
		} catch (e) {
			console.error(e);
			error = 'Could not access camera. Please allow camera permissions.';
		}
	}

	function stop() {
		stream?.getTracks().forEach((track) => track.stop());
		stream = null;
	}

	function capture() {
		if (!videoEl) return;

		const canvas = document.createElement('canvas');
		canvas.width = videoEl.videoWidth;
		canvas.height = videoEl.videoHeight;
		const ctx = canvas.getContext('2d');
		if (ctx) {
			ctx.drawImage(videoEl, 0, 0, canvas.width, canvas.height);
			avatar = canvas.toDataURL('image/png');
		}

		stop();
	}

	let name = $state('');

	const userTypes = [
		{ value: 'applicant', label: 'Applicant' },
		{ value: 'student', label: 'Student' },
		{ value: 'staff', label: 'Staff' }
	] as const;

	let userType = $state<(typeof userTypes)[number]['value']>('applicant');
</script>

<div class="mx-auto mt-8 flex max-w-sm flex-col gap-6 lg:sticky">
	<Card.Root class="gap-1 overflow-hidden p-0">
		<div
			class="relative flex flex-col items-center gap-3 border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
			style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
		>
			{#if stream}
				<video
					bind:this={videoEl}
					autoplay
					playsinline
					muted
					class="size-64 transform-[scaleX(-1)] rounded-md border-2 border-muted object-cover"
				></video>

				<ButtonGroup.Root>
					<Button size="sm" onclick={capture}>
						<Camera class="size-4" />
						Capture
					</Button>
					<Button size="sm" variant="outline" class="bg-muted" onclick={stop}>
						<X class="size-4" />
						Cancel
					</Button>
				</ButtonGroup.Root>
			{:else}
				<Avatar.Root class="size-64 rounded-md border-2 border-muted">
					{#if avatar}
						<Avatar.Image src={avatar} alt="Avatar preview" class="object-cover" />
					{:else}
						<Avatar.Fallback class="size-64 rounded-md">
							<Button variant="outline" onclick={start}>
								<Camera class="size-4" />
								Take photo
							</Button>
						</Avatar.Fallback>
					{/if}
				</Avatar.Root>

				{#if avatar}
					<Button size="sm" variant="outline" class="bg-muted" onclick={start}>
						<RefreshCw class="size-4" />
						Retake
					</Button>
				{/if}
			{/if}

			{#if error}
				<p class="text-xs text-destructive">{error}</p>
			{/if}
		</div>

		<Card.Content class="flex flex-col gap-4 p-4">
			<Field.Field>
				<Field.Label for="name">Name</Field.Label>
				<Input id="name" bind:value={name} maxlength={255} placeholder="Full name" />
				<Field.Error errors={[]} class="justify-center" />
			</Field.Field>

			<Field.Field>
				<Field.Label for="user-type">User type</Field.Label>
				<Tabs.Root bind:value={userType} class="w-full">
					<Tabs.List class="grid w-full grid-cols-3">
						{#each userTypes as type (type.value)}
							<Tabs.Trigger value={type.value}>{type.label}</Tabs.Trigger>
						{/each}
					</Tabs.List>
				</Tabs.Root>
			</Field.Field>
		</Card.Content>
	</Card.Root>
</div>
