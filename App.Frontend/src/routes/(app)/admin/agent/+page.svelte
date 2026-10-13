<script lang="ts">
	import { tick } from 'svelte';
	import { slide } from 'svelte/transition';
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
	import Loader from '$lib/components/loader.svelte';

	import { ShieldAlert, TriangleAlert, ArrowUp, Sparkles, Wrench } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import * as ButtonGroup from '$lib/components/button-group';
	import Separator from '$lib/components/separator/separator.svelte';

	const chat = Agent.get();

	let query = $state('');
	let bottom = $state<HTMLElement>();

	const busy = $derived(chat.loading || chat.pending !== null);
	const suggestions = ['Who am I?', 'List the users', 'Who is currently frozen?', 'What can you do?'];
	const quotes = [
		'Scraping for nuclear launch codes...',
		'Finding solid investment advise...',
		'Figuring out the meaning of life...',
		'Bribing the database...',
		'Assembling skynet...',
		'Rethinking life choices...',
		'Asking the rubber duck...',
		'Searching for cheap ram...',
		'Unshredding old logs...',
		'Sending tech support tickets...',
	];

	let index = $state(0);
	let qoute = $derived(quotes[index]);
	$effect(() => {
		let interval = setInterval(() => {
			index = Math.floor(Math.random() * quotes.length);
		}, 4000);

		return () => {
			clearInterval(interval);
		};
	});

	function submit(event: SubmitEvent) {
		event.preventDefault();
		const text = query;

		if (text.trim() && !busy) {
			query = '';
			chat.send(text);
		}
	}

	function stringify(raw: string) {
		try {
			return JSON.stringify(JSON.parse(raw), null, 2);
		} catch {
			return raw;
		}
	}

	$effect(() => {
		void [chat.messages.length, chat.loading, chat.pending, chat.error];
		tick().then(() => bottom?.scrollIntoView({ behavior: 'smooth', block: 'end' }));
	});
</script>

{#snippet assistant()}
	<Message.Avatar>
		<Avatar.Root>
			<Avatar.Fallback><Sparkles class="size-4" /></Avatar.Fallback>
		</Avatar.Root>
	</Message.Avatar>
{/snippet}

<div class="flex w-full flex-col gap-4 pb-4">
	{#if chat.messages.length === 0}
		<Empty.Root>
			<Empty.Header>
				<Empty.Media variant="icon"><Sparkles /></Empty.Media>
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
						<Avatar.Fallback class="uppercase">
							{page.data.session.username.slice(0, 2)}
						</Avatar.Fallback>
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
					<Marker.Icon><Wrench /></Marker.Icon>
					<Marker.Content>
						Called <code class="font-mono text-foreground">{call.name}</code>
					</Marker.Content>
				</Marker.Root>
			{/each}
		{:else if message.content}
			<details class="text-xs text-muted-foreground">
				<summary class="cursor-pointer select-none">Result of {message.toolName}</summary>
				<pre class="mt-1 max-h-64 overflow-auto rounded-md bg-muted p-2">{stringify(message.content)}</pre>
			</details>
		{/if}
	{/each}

	{#if chat.pending}
		<Message.Root>
			{@render assistant()}
			<Message.Content>
				<Bubble.Root variant="ghost">
					<Bubble.Content class="space-y-3 p-0">
						<Alert.Root variant="destructive">
							<ShieldAlert size={18} />
							<Alert.Title>The assistant wants to make a changes.</Alert.Title>
							<Alert.Description>
								<span class="text-xs">
									If you feel unsure on what to do, deny these changes and ask for help.
								</span>
								<Separator />
								{#each chat.pending.calls as call, c (c)}
									<div class="rounded-md border p-2 text-xs text-muted-foreground">
										<div class="flex items-center gap-2">
											<code class="font-mono font-semibold">{call.name}</code>
											{#if !call.requiresApproval}
												<span class="text-muted-foreground">(read only)</span>
											{/if}
										</div>
										<p class="text-muted-foreground">{call.description}</p>
										<pre class="mt-1 overflow-auto rounded bg-muted p-2">{JSON.stringify(
												call.arguments,
												null,
												2
											)}
											</pre>
									</div>
								{/each}
								<ButtonGroup.Root class="text-muted-foreground">
									<Button
										size="sm"
										variant="destructive"
										class="border"
										disabled={chat.loading}
										onclick={() => chat.decide('Approve')}>Approve</Button
									>
									<Button
										size="sm"
										variant="outline"
										disabled={chat.loading}
										onclick={() => chat.decide('Deny')}
									>
										Deny
									</Button>
								</ButtonGroup.Root>
							</Alert.Description>
						</Alert.Root>
					</Bubble.Content>
				</Bubble.Root>
			</Message.Content>
		</Message.Root>
	{/if}

	{#if chat.loading}
		<Marker.Root role="status">
			<Marker.Icon><Loader /></Marker.Icon>
			<Marker.Content>
				{#key index}
					<p class="text-xs" transition:slide>
						{qoute}
					</p>
				{/key}
			</Marker.Content>
		</Marker.Root>
	{/if}

	{#if chat.error}
		<Bubble.Root variant="destructive" class="max-w-full">
			<Bubble.Content class="flex flex-wrap items-center gap-2" role="alert">
				<TriangleAlert class="size-4 shrink-0" />
				<p>{chat.error}</p>
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

<form class="sticky bottom-0 w-full space-y-2 bg-background pb-2" onsubmit={submit}>
	<InputGroup.Root>
		<InputGroup.Input
			bind:value={query}
			disabled={busy}
			autocomplete="off"
			placeholder={chat.pending ? 'Approve or deny the change above first' : 'Ask the agent...'}
		/>
		<InputGroup.Addon align="block-end" class="pt-1">
			<InputGroup.Button
				type="submit"
				variant="default"
				size="icon-sm"
				class="ml-auto"
				disabled={busy || !query.trim()}
			>
				<ArrowUp />
				<span class="sr-only">Send</span>
			</InputGroup.Button>
		</InputGroup.Addon>
	</InputGroup.Root>

	<p class="flex items-center gap-1 text-xs text-muted-foreground">
		<TriangleAlert size={14} />
		Agentic Administration is AI and can make mistakes. Please double-check responses.
	</p>
</form>
