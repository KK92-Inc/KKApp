<script lang="ts">
	import { Button, buttonVariants } from '$lib/components/button/index.js';
	import { CircleAlert, ExternalLink, Info, Plus } from '@lucide/svelte';
	import { addKey } from '$lib/remotes/ssh.remote';
	import * as Dialog from '$lib/components/dialog/index.js';
	import { Input } from '$lib/components/input/index.js';
	import * as Field from '$lib/components/field';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
</script>

<Dialog.Root>
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
			{#if addKey.result?.success === false}
				<Alert.Root variant="destructive">
					<Info class="h-4 w-4" />
					<Alert.Title>{addKey.result.message}</Alert.Title>
				</Alert.Root>
			{/if}
		</Dialog.Header>
		<form {...addKey}>
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
								{...addKey.fields.title.as('text')}
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
						<Field.Description>Choose a unique username for your account.</Field.Description>
						<Field.Error errors={addKey.fields.title.issues()} />
					</Field.Field>

					<!-- Key -->
					<Field.Field>
						<Field.Label for="key">Public Key</Field.Label>
						<Input
							id="key"
							autocomplete="off"
							autocorrect="off"
							placeholder="ssh-ed25519 AAAAC3..."
							{...addKey.fields.publicKey.as('text')}
						/>
						<Field.Error errors={addKey.fields.publicKey.issues()} />
					</Field.Field>
				</Field.Group>
			</Field.Set>
			<Dialog.Footer class="pt-4">
				<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
				<Button type="submit" class={buttonVariants({ variant: 'default' })}>Add</Button>
			</Dialog.Footer>
		</form>
	</Dialog.Content>
</Dialog.Root>
