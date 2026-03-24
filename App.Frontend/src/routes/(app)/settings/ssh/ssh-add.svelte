<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button/index.js';
	import { CircleAlert, ExternalLink, Info, Plus } from '@lucide/svelte';
	import * as SSH from '$lib/remotes/ssh.remote';
	import * as Dialog from '$lib/components/dialog/index.js';
	import { Input } from '$lib/components/input/index.js';
	import * as Field from '$lib/components/field';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import { toast } from 'svelte-sonner';

	let open = $state(false);
	let title = $state('');
	let publicKey = $state('');
	let loading = $derived(SSH.create.pending > 0);
	let disabled = $derived(title.trim() === '' || publicKey.trim() === '');

	async function handleAdd() {
		try {
			await SSH.create({ title, publicKey });
			toast.success('SSH key added', {
				description: `"${title}" has been added to your account.`
			});
		} catch (error) {
			const message = error instanceof Error ? error.message : 'Something went wrong.';
			toast.error('Failed to add SSH key', { description: message });
		}
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Trigger class={buttonVariants({ variant: 'outline' })}>
		Add Key
		<Plus />
	</Dialog.Trigger>

	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Add SSH Key for Git Access</Dialog.Title>
			<Dialog.Description>
				Add a new SSH key, give it a name you recognize to which device it belongs to.
			</Dialog.Description>
		</Dialog.Header>

		<Field.Set>
			<Field.Group>
				<!-- Title -->
				<Field.Field>
					<Field.Label for="title">Title</Field.Label>
					<InputGroup.Root>
						<InputGroup.Input
							id="title"
							autocomplete="off"
							autocorrect="off"
							placeholder="Arch Linux Home"
							bind:value={title}
						/>
						<InputGroup.Addon align="inline-end">
							<Tooltip.Root>
								<Tooltip.Trigger>
									{#snippet child({ props })}
										<InputGroup.Button {...props} class="rounded-full" size="icon-xs">
											<CircleAlert />
										</InputGroup.Button>
									{/snippet}
								</Tooltip.Trigger>
								<Tooltip.Content>Key name so you can identify it at a glance.</Tooltip.Content>
							</Tooltip.Root>
						</InputGroup.Addon>
					</InputGroup.Root>
					<Field.Description>A label to identify which device this key belongs to.</Field.Description>
				</Field.Field>

				<!-- Public Key -->
				<Field.Field>
					<Field.Label for="key">Public Key</Field.Label>
					<Input
						id="key"
						autocomplete="off"
						autocorrect="off"
						placeholder="ssh-ed25519 AAAAC3..."
						bind:value={publicKey}
					/>
				</Field.Field>
			</Field.Group>
		</Field.Set>

		<Dialog.Footer class="pt-4">
			<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
			<Button variant="default" {loading} {disabled} onclick={handleAdd}>
				{loading ? 'Adding...' : 'Add'}
			</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>
