<script lang="ts">
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { RotateCcw, HeartHandshake, FlaskConical } from '@lucide/svelte';
	import type { LayoutProps } from './$types';
	import * as Alert from '$lib/components/alert';
	import * as Agent from '$lib/contexts/agent.svelte';

	let { children }: LayoutProps = $props();
	const chat = Agent.init(new Agent.AgentChat());
</script>

<div class="flex items-center gap-2">
	<h1 class="text-lg font-bold text-muted-foreground">Agent</h1>
	<Separator class="flex-1" />
	<Button
		size="sm"
		variant="outline"
		disabled={chat.loading || (chat.messages.length === 0 && !chat.error)}
		onclick={() => chat.reset()}
	>
		Restart
		<RotateCcw />
	</Button>
</div>

<Alert.Root class="my-2" variant="warning">
	<FlaskConical />
	<Alert.Title>Experimental Feature</Alert.Title>
	<Alert.Description class="text-xs">
		<p>
			Agentic Administration lets you use a LLM to directly manage the environment. The LLM has no direct
			database access and will always ask for your approval to run destructive actions.
		</p>
		<Separator />
		<Button size="sm" variant="secondary" class="border" href="https://github.com/KK92-Inc/KKApp/issues">
			Provide Feedback
			<HeartHandshake />
		</Button>
	</Alert.Description>
</Alert.Root>

{@render children()}
