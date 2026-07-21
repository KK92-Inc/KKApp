<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button';
	import { CircleQuestionMark, Clipboard, KeyRound, Plus, Save, TriangleAlert } from '@lucide/svelte';
	import * as Account from '$lib/remotes/account.remote';
	import * as Tabs from '$lib/components/tabs';
	import * as Dialog from '$lib/components/dialog';
	import { Input } from '$lib/components/input';
	import * as Field from '$lib/components/field';
	import * as Item from '$lib/components/item';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import { toast } from 'svelte-sonner';
	import { Problem, type ValidationErrors } from '$lib/api';
	import SshHelp from './ssh-help.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { page } from '$app/state';
	import * as Kbd from '$lib/components/kbd';
	import * as Alert from '$lib/components/alert';
	import { env } from '$env/dynamic/public';
	import { ensure } from '$lib/utils';

	let open = $state(false);
	let agent = navigator.userAgent;
	let os = $state<'nt' | 'macos' | 'linux'>(agent.includes('Windows') ? 'nt' : agent.includes('Mac') ? 'macos' : 'linux');

	let errors = $state<ValidationErrors>({});
	let fields = $state({ title: '', publicKey: '' });
	// svelte-ignore state_referenced_locally
	const original = $state.snapshot(fields);

	const keygen = $derived(
		os !== 'nt'
			? `curl -fsSL ${env.PUBLIC_DOMAIN}/key | bash`
			: `powershell -c "irm ${env.PUBLIC_DOMAIN}/key.ps1|iex"`
	);

	async function submit() {
		try {
			await Account.addKey(fields);
			fields = original;
			open = false;
		} catch (error) {
			const resolved = Problem.resolve(error);
			if (resolved.kind === 'validation') {
				errors = resolved.fields;
			} else {
				toast.error(resolved.message);
			}
		}
	}
</script>

<Dialog.Root
	bind:open
	onOpenChange={(v) => {
		if (!v) errors = {};
	}}
>
	<Dialog.Trigger type="button" class={buttonVariants({ variant: 'outline', size: 'sm' })}>
		Add
		<Plus />
	</Dialog.Trigger>
	<Dialog.Content class="gap-2">
		<Dialog.Header>
			<Dialog.Title class="flex items-center justify-between">Add Key</Dialog.Title>
		</Dialog.Header>

		<Tabs.Root bind:value={os} class="mt-6">
			<Tabs.List class="h-8 w-full">
				<Tabs.Trigger class="h-6" value="macos">macOS</Tabs.Trigger>
				<Tabs.Trigger class="h-6" value="linux">Linux</Tabs.Trigger>
				<Tabs.Trigger class="h-6" value="nt">Windows</Tabs.Trigger>
			</Tabs.List>
		</Tabs.Root>

		<Separator />

		<Item.Group class="mt-2 grid grid-rows-[auto_auto] gap-3">
			<!-- <Alert.Root variant="default">
				<TriangleAlert class="h-4 w-4" />
				<Alert.Title>Please try and read the instructions</Alert.Title>
				<Alert.Description class="font-normal text-wrap">
					If you're unfamiliar with terminals or shells this might be confusing. Make sure to read everything
					so you understand whats going on.
				</Alert.Description>
			</Alert.Root> -->

			<Item.Root variant="muted" size="sm" class="items-start">
				<Item.Media variant="icon"><KeyRound class="size-4 text-primary" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-xs">
						<span class="inline-flex flex-wrap items-center gap-1">
							<span>1. Open your</span>
							<Tooltip.Root delayDuration={100}>
								<Tooltip.Trigger>
									{#snippet child({ props })}
										<span {...props} class="inline-flex items-center gap-1 underline">
											terminal
											<CircleQuestionMark size={16} />
										</span>
									{/snippet}
								</Tooltip.Trigger>
								<Tooltip.Content class="block">
									{#if os === 'nt'}
										Open the Start menu, type Windows PowerShell, select Windows PowerShell, open it.
									{:else}
										<p>
											On <span class="font-extrabold">macOs</span>, press
											<Kbd.Group>
												<Kbd.Root>⌘</Kbd.Root>
												<span class="text-foreground">+</span>
												<Kbd.Root>Space</Kbd.Root>
											</Kbd.Group>
											, type Terminal, and press Return.
										</p>
										<Separator class="my-1" />
										<p>
											On <span class="font-extrabold">Linux</span>, open your Terminal app from the
											applications menu.
										</p>
									{/if}
								</Tooltip.Content>
							</Tooltip.Root>
							, paste the following and press enter:
						</span>
					</Item.Title>
					<InputGroup.Root>
						<InputGroup.Input readonly value={keygen} />
						<InputGroup.Addon align="inline-end">
							<InputGroup.Copy value={keygen} />
						</InputGroup.Addon>
					</InputGroup.Root>
					<p class="text-xs">
						Once you've run the command your key should be in your clipboard, paste it into the Public Key
						field below.
					</p>
				</Item.Content>
			</Item.Root>
		</Item.Group>

		<Separator />

		<Field.Set class="mt-4">
			<Field.Group>
				<Field.Field>
					<Field.Label for="key-title">Title</Field.Label>
					<Input
						id="key-title"
						autocomplete="off"
						autocorrect="off"
						placeholder="My Personal Computer"
						bind:value={fields.title}
					/>
					<Field.Description>A label to identify which device this key belongs to.</Field.Description>
					<Field.Error errors={errors.title} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="key-value">
						<span class="flex-1">Public Key</span>
						<SshHelp title="What is this" />
					</Field.Label>
					<Input
						id="key-value"
						autocomplete="off"
						autocorrect="off"
						placeholder="ssh-ed25519 AAAAC3..."
						bind:value={fields.publicKey}
					/>
					<Field.Description>A public key you generated.</Field.Description>
					<Field.Error errors={errors.publicKey} />
				</Field.Field>
			</Field.Group>
		</Field.Set>
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" loading={Account.addKey.pending > 0} onclick={submit}>Add <Save /></Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
