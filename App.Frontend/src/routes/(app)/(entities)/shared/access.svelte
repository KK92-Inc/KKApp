<script lang="ts">
	import { Zap, Unlock, Lock } from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import { Switch } from '$lib/components/switch';
	import { cn } from '$lib/utils';

	interface Props {
		visible: boolean;
		enabled: boolean;
	}

	let { visible = $bindable(), enabled = $bindable() }: Props = $props();
</script>

<Card.Root class="gap-2 py-4">
	<Card.Header class="px-4">
		<Card.Title class="text-sm font-medium text-muted-foreground">Access Modifiers</Card.Title>
	</Card.Header>
	<Card.Content class="px-4">
		<Field.Set>
			<Field.Group>
				<Field.Field orientation="horizontal" class="items-center">
					<Field.Content>
						<Field.Label for="cursus-public" class="flex items-center gap-2">
							{#if visible}
								<Unlock size={16} class="text-emerald-500" />
							{:else}
								<Lock size={16} class="text-muted-foreground" />
							{/if}
							Public
						</Field.Label>
						<Field.Description>
							{#if visible}
								Visible to all users on the platform.
							{:else}
								Visible only to you and staff,
							{/if}
						</Field.Description>
					</Field.Content>
					<Switch id="cursus-public" bind:checked={visible} />
				</Field.Field>

				<Field.Field orientation="horizontal" class="items-center">
					<Field.Content>
						<Field.Label for="cursus-enabled" class="flex items-center gap-2">
							<Zap size={16} class={cn(enabled ? 'text-amber-500' : 'text-muted-foreground')} />
							Enabled
						</Field.Label>
						<Field.Description>
							{#if enabled}
								Other users can subscribe to this entity.
							{:else}
								Subscriptions to this entity are disabled
							{/if}
						</Field.Description>
					</Field.Content>
					<Switch id="cursus-enabled" bind:checked={enabled} />
				</Field.Field>
			</Field.Group>
		</Field.Set>
	</Card.Content>
</Card.Root>
