<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button';
	import {
		CircleQuestionMark,
		Clipboard,
		KeyRound,
		Plus,
		Save
	} from '@lucide/svelte';
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

	let open = $state(false);
	let os = $state<'nt' | 'unix'>(navigator.userAgent.includes('Windows') ? 'nt' : 'unix');
	let errors = $state<ValidationErrors>({});
	let fields = $state({
		title: '',
		publicKey: ''
	});

	const keygen = $derived(`ssh-keygen -t ed25519 -C "${page.data.session.email}"`);
	const cat = $derived(
		os === 'nt' ? String.raw`Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub` : 'cat ~/.ssh/id_ed25519.pub'
	);

	async function submit() {
		try {
			await Account.addKey(fields);
			open = false;
		} catch (e) {
			const resolved = Problem.resolve(e);
			if (resolved.kind === 'validation') {
				errors = resolved.fields;
			} else {
				toast.error(resolved.message);
			}
		}
	}
</script>

<Dialog.Root bind:open onOpenChange={(v) => { if (!v) errors = {}}}>
	<Dialog.Trigger type="button" class={buttonVariants({ variant: 'outline', size: 'sm' })}>
		Add Key
		<Plus />
	</Dialog.Trigger>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title class="flex items-center justify-between">Add SSH Key</Dialog.Title>
		</Dialog.Header>

		<Tabs.Root bind:value={os} class="mt-4">
			<Tabs.List class="h-8 w-full">
				<Tabs.Trigger class="h-6" value="unix">macOS / Linux</Tabs.Trigger>
				<Tabs.Trigger class="h-6" value="nt">Windows</Tabs.Trigger>
			</Tabs.List>
		</Tabs.Root>

		<Separator class=" absolute top-28 left-0" />

		<Item.Group class="mt-2 grid grid-rows-[auto_auto] gap-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><KeyRound class="size-4 text-primary" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">
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
								<Tooltip.Content>
									{#if os === 'nt'}
										Open the Start menu, type Windows PowerShell, select Windows PowerShell, then Open.
									{:else}
									<p>On macOS, press

									<Kbd.Group>
										<Kbd.Root>⌘</Kbd.Root>
										<span class="text-foreground">+</span>
										<Kbd.Root>Space</Kbd.Root>
									</Kbd.Group>

										, type Terminal, and press Return.</p>
									<p>On Linux, open your Terminal app from the applications menu.</p>
									{/if}
								</Tooltip.Content>
							</Tooltip.Root>
							<span>, paste the following:</span>
						</span>
					</Item.Title>
					<InputGroup.Root>
						<InputGroup.Input readonly value={keygen} />
						<InputGroup.Addon align="inline-end">
							<InputGroup.Copy value={keygen} />
						</InputGroup.Addon>
					</InputGroup.Root>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Clipboard class="size-4 text-primary" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">2. Copy your public key</Item.Title>
					<InputGroup.Root>
						<InputGroup.Input readonly value={cat} />
						<InputGroup.Addon align="inline-end">
							<InputGroup.Copy value={cat} />
						</InputGroup.Addon>
					</InputGroup.Root>
				</Item.Content>
			</Item.Root>
		</Item.Group>

		<Separator class=" absolute top-82 left-0" />

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
					<Field.Description>A label to identify which device this key belongs to.</Field.Description>
					<Field.Error errors={errors.publicKey} />
				</Field.Field>
			</Field.Group>
		</Field.Set>
		<Dialog.Footer>
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button type="submit" loading={Account.addKey.pending > 0} onclick={submit}>Add <Save/></Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
