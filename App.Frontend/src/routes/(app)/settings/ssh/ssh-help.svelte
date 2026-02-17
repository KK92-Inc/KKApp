<script lang="ts">
	import { buttonVariants } from '$lib/components/button/index.js';
	import { CircleQuestionMark, Copy, CopyCheck } from '@lucide/svelte';
	import * as Dialog from '$lib/components/dialog/index.js';
	import * as InputGroup from '$lib/components/input-group';
	import Separator from '$lib/components/separator/separator.svelte';
	import { page } from '$app/state';

	const cmd = `ssh-keygen -t ed25519 -C "${page.data.session.email}"`;
</script>

<Dialog.Root>
	<Dialog.Trigger class={buttonVariants({ variant: 'outline' })}>
		About SSH
		<CircleQuestionMark />
	</Dialog.Trigger>

	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Add SSH Key for Git Access</Dialog.Title>
			<Dialog.Description>
				<div class="space-y-4">
					<p class="text-sm text-muted-foreground">
						SSH keys are an alternate way to identify yourself that doesn't require you to enter
						your username and password every time. They are commonly used for secure communication
						with Git repositories.
					</p>
					<p class="text-sm text-muted-foreground">
						To generate a new SSH key, open your terminal and paste the command below.
					</p>
					<p class="text-sm text-muted-foreground">
						The value returned can then be pasted into the "Add Key" dialog.
					</p>
				</div>
			</Dialog.Description>
		</Dialog.Header>
		<Separator />

		<InputGroup.Root>
			<InputGroup.Input
				id="title"
				autocomplete="off"
				autocorrect="off"
				autosave="off"
				readonly
				value={cmd}
			/>
			<InputGroup.Addon align="inline-end">
				<InputGroup.Copy value={cmd} />
			</InputGroup.Addon>
		</InputGroup.Root>
	</Dialog.Content>
</Dialog.Root>
