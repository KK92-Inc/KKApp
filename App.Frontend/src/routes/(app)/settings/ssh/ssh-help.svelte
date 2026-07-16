<script lang="ts">
	import { buttonVariants } from '$lib/components/button/index.js';
	import { CircleQuestionMark, KeyRound, Lock, ShieldCheck } from '@lucide/svelte';
	import * as Dialog from '$lib/components/dialog/index.js';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Item from '$lib/components/item';

	const { title = 'About SSH' }: { title?: string } = $props();
</script>

<Dialog.Root>
	<Dialog.Trigger class={buttonVariants({ variant: 'outline', size: 'sm' })}>
		{title}
		<CircleQuestionMark />
	</Dialog.Trigger>

	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>What's an SSH key?</Dialog.Title>
			<Dialog.Description></Dialog.Description>
		</Dialog.Header>

		<Item.Group class="grid grid-rows-3 gap-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><KeyRound class="size-4"/></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">It's a pair of files, not a password</Item.Title>
					<Item.Description class="line-clamp-none">
						Generating a key creates two files: a private one that never leaves your computer, and a public
						one that's safe to hand out. They're mathematically linked, anything locked with the public key
						can only be opened with the matching private one.
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Lock class="size-4"/></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">We only ever want the public one</Item.Title>
					<Item.Description class="line-clamp-none">
						The file ending in <code class="text-xs">.pub</code> is the one you need. The other file (no
						extension, or ending in nothing) is your private key. Treat it like a house key,
						if a site ever asks you to paste that one, don't.
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><ShieldCheck class="size-4"/></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Why bother, what for ?</Item.Title>
					<Item.Description class="line-clamp-none">
						Once it's set up, your computer proves who you are automatically. No password typed or stored
						anywhere, and it's not something that can be guessed or phished the way a password can. As a result
						we can authenticate certain actions directly such as reading a repository.
					</Item.Description>
				</Item.Content>
			</Item.Root>
		</Item.Group>

		<!-- <div class="space-y-5 text-sm">
			<div class="flex gap-3">
				<KeyRound class="mt-0.5 h-5 w-5 shrink-0 text-muted-foreground" />
				<div>
					<p class="font-medium">It's a pair of files, not a password</p>
					<p class="mt-1 text-muted-foreground">
						Generating a key creates two files: a private one that never leaves your computer, and a public
						one that's safe to hand out. They're mathematically linked — anything locked with the public key
						can only be opened with the matching private one.
					</p>
				</div>
			</div>

			<div class="flex gap-3">
				<Lock class="mt-0.5 h-5 w-5 shrink-0 text-muted-foreground" />
				<div>
					<p class="font-medium">We only ever want the public one</p>
					<p class="mt-1 text-muted-foreground">
						The file ending in <code class="text-xs">.pub</code> is the one you paste in here. The other file (no
						extension, or ending in nothing) is your private key — treat it like a house key, not a business card.
						If a site ever asks you to paste that one, don't.
					</p>
				</div>
			</div>

			<div class="flex gap-3">
				<ShieldCheck class="mt-0.5 h-5 w-5 shrink-0 text-muted-foreground" />
				<div>
					<p class="font-medium">Why bother</p>
					<p class="mt-1 text-muted-foreground">
						Once it's set up, your computer proves who you are automatically — no password typed or stored
						anywhere, and it's not something that can be guessed or phished the way a password can.
					</p>
				</div>
			</div>
		</div> -->

		<Separator />

		<p class="text-xs text-muted-foreground">
			Ready to make one? Close this and use the <span class="font-medium">Add Key</span> button.
		</p>
	</Dialog.Content>
</Dialog.Root>
