<script lang="ts">
	import { tick } from 'svelte';
	import { page } from '$app/state';
	import * as Agent from '$lib/contexts/agent.svelte';
	import * as Avatar from '$lib/components/avatar';
	import * as Bubble from '$lib/components/bubble';
	import * as Message from '$lib/components/message';
	import * as Marker from '$lib/components/marker';
	import * as Empty from '$lib/components/empty/index.js';
	import * as InputGroup from '$lib/components/input-group/index.js';
	import Button from '$lib/components/button/button.svelte';
	import Markdown from '$lib/components/markdown/markdown.svelte';

	import ArrowUpIcon from '@lucide/svelte/icons/arrow-up';
	import SparklesIcon from '@lucide/svelte/icons/sparkles';
	import WrenchIcon from '@lucide/svelte/icons/wrench';
	import LoaderCircleIcon from '@lucide/svelte/icons/loader-circle';
	import TriangleAlertIcon from '@lucide/svelte/icons/triangle-alert';

	const chat = Agent.get();

	let input = $state('');
	let bottom = $state<HTMLElement>();

	const initials = $derived(page.data.session.username.slice(0, 2).toUpperCase());
	const suggestions = ['Who am I?', 'List the users', 'Who is currently frozen?'];
	const busy = $derived(chat.loading || chat.pending !== null);

	function submit(event: SubmitEvent) {
		event.preventDefault();
		const text = input;
		if (!text.trim() || busy) return;
		input = '';
		chat.send(text);
	}

	/** Tool results are JSON, show them readable. */
	function pretty(raw: string) {
		try {
			return JSON.stringify(JSON.parse(raw), null, 2);
		} catch {
			return raw;
		}
	}

	/** Short one-liner of a call's arguments for the trace, e.g. login: "jd". */
	function summary(args: Record<string, unknown>) {
		const entries = Object.entries(args);
		if (entries.length === 0) return '';
		return entries.map(([key, value]) => `${key}: ${JSON.stringify(value)}`).join(', ');
	}

	// Keep the newest thing in view.
	$effect(() => {
		void [chat.messages.length, chat.loading, chat.pending, chat.error];
		tick().then(() => bottom?.scrollIntoView({ behavior: 'smooth', block: 'end' }));
	});
</script>

{#snippet assistant()}
	<Message.Avatar>
		<Avatar.Root>
			<Avatar.Fallback><SparklesIcon class="size-4" /></Avatar.Fallback>
		</Avatar.Root>
	</Message.Avatar>
{/snippet}

<div class="flex w-full flex-col gap-4 pb-4">
	{#if chat.messages.length === 0}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><SparklesIcon /></Empty.Media>
				<Empty.Title>What can I help with?</Empty.Title>
				<Empty.Description>
					Ask about users and freezes. Anything that changes data asks for your approval first.
				</Empty.Description>
			</Empty.Header>
			<Empty.Content>
				<div class="flex flex-wrap justify-center gap-2">
					{#each suggestions as suggestion (suggestion)}
						<Button size="sm" variant="outline" disabled={busy} onclick={() => chat.send(suggestion)}>
							{suggestion}
						</Button>
					{/each}
				</div>
			</Empty.Content>
		</Empty.Root>
	{/if}

	{#each chat.messages as message, i (i)}
		{#if message.role === 'user'}
			<Message.Root align="end">
				<Message.Avatar>
					<Avatar.Root>
						<Avatar.Fallback>{initials}</Avatar.Fallback>
					</Avatar.Root>
				</Message.Avatar>
				<Message.Content>
					<Bubble.Root>
						<Bubble.Content class="whitespace-pre-wrap">{message.content}</Bubble.Content>
					</Bubble.Root>
				</Message.Content>
			</Message.Root>
		{:else if message.role === 'assistant'}
			{#if message.content}
				<Message.Root>
					{@render assistant()}
					<Message.Content>
						<Bubble.Root variant="muted">
							<Bubble.Content>
								<Markdown value={message.content} />
							</Bubble.Content>
						</Bubble.Root>
					</Message.Content>
				</Message.Root>
			{/if}

			{#each message.toolCalls ?? [] as call, c (c)}
				<Marker.Root>
					<Marker.Icon><WrenchIcon /></Marker.Icon>
					<Marker.Content>
						Called <code class="font-mono text-foreground">{call.name}</code>
						<span class="font-mono text-xs">{summary(call.arguments)}</span>
					</Marker.Content>
				</Marker.Root>
			{/each}
		{:else}
			<details class="text-xs text-muted-foreground">
				<summary class="cursor-pointer select-none">Result of {message.toolName}</summary>
				<pre class="mt-1 max-h-64 overflow-auto rounded-md bg-muted p-2">{pretty(message.content)}</pre>
			</details>
		{/if}
	{/each}

	{#if chat.pending}
		<Message.Root>
			{@render assistant()}
			<Message.Content>
				<Bubble.Root variant="outline">
					<Bubble.Content class="space-y-3">
						<p class="font-medium">The assistant wants to make a change. Approve it?</p>

						{#each chat.pending.calls as call, c (c)}
							<div class="rounded-md border p-2 text-xs">
								<div class="flex items-center gap-2">
									<code class="font-mono font-semibold">{call.name}</code>
									{#if !call.requiresApproval}
										<span class="text-muted-foreground">(read only)</span>
									{/if}
								</div>
								<p class="text-muted-foreground">{call.description}</p>
								<pre class="mt-1 overflow-auto rounded bg-muted p-2">{JSON.stringify(call.arguments, null, 2)}</pre>
							</div>
						{/each}

						<div class="flex gap-2">
							<Button size="sm" disabled={chat.loading} onclick={() => chat.decide('Approve')}>Approve</Button>
							<Button size="sm" variant="outline" disabled={chat.loading} onclick={() => chat.decide('Deny')}>
								Deny
							</Button>
						</div>
					</Bubble.Content>
				</Bubble.Root>
			</Message.Content>
		</Message.Root>
	{/if}

	{#if chat.loading}
		<Marker.Root role="status">
			<Marker.Icon><LoaderCircleIcon class="animate-spin" /></Marker.Icon>
			<Marker.Content class="animate-pulse">Thinking...</Marker.Content>
		</Marker.Root>
	{/if}

	{#if chat.error}
		<Bubble.Root variant="destructive" class="max-w-full">
			<Bubble.Content class="flex flex-wrap items-center gap-2" role="alert">
				<TriangleAlertIcon class="size-4 shrink-0" />
				<span>{chat.error}</span>
				{#if !chat.pending}
					<Button size="sm" variant="outline" disabled={chat.loading} onclick={() => chat.retry()}>
						Try again
					</Button>
				{/if}
			</Bubble.Content>
		</Bubble.Root>
	{/if}

	<div bind:this={bottom} class="scroll-mb-28"></div>
</div>

<form class="sticky bottom-0 w-full bg-background pt-2 pb-2" onsubmit={submit}>
	<InputGroup.Root>
		<div class="h-14 w-full py-2.5">
			<InputGroup.Input
				bind:value={input}
				disabled={busy}
				autocomplete="off"
				placeholder={chat.pending ? 'Approve or deny the change above first' : 'Ask the agent...'}
			/>
		</div>
		<InputGroup.Addon align="block-end" class="pt-1">
			<InputGroup.Button
				type="submit"
				variant="default"
				size="icon-sm"
				class="ml-auto"
				disabled={busy || !input.trim()}
			>
				<ArrowUpIcon />
				<span class="sr-only">Send</span>
			</InputGroup.Button>
		</InputGroup.Addon>
	</InputGroup.Root>
</form>
