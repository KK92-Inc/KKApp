<script lang="ts">
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { RotateCcw, CircleAlert, HeartHandshake } from '@lucide/svelte';
	import type { LayoutProps } from './$types';
	import * as Alert from '$lib/components/alert';
	let { children }: LayoutProps = $props();
</script>

<div class="flex items-center gap-2">
	<h1 class="text-lg font-bold text-muted-foreground">Agent</h1>
	<Separator class="flex-1" />
	<Button size="sm" variant="outline">
		Restart
		<RotateCcw />
	</Button>
</div>

<Alert.Root class="my-2">
	<CircleAlert />
	<Alert.Title>Agentic Administration</Alert.Title>
	<Alert.Description class="text-xs">
		<p>
			This is a experimental feature that lets you directly use a LLM Agent to manage users, investigate state
			and much more.
		</p>

		<p>The usual rules around AI therefor apply, it may hallucinate and provide incorrect data.</p>
		<Button size="sm" variant="secondary" href="https://github.com/KK92-Inc/KKApp/issues">
			Provide Feedback
			<HeartHandshake />
		</Button>
	</Alert.Description>
</Alert.Root>

{@render children()}
