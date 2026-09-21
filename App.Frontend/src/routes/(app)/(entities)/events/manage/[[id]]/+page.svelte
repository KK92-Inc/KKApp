<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import Separator from '$lib/components/separator/separator.svelte';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import * as Page from './context.svelte';
	import type { PageProps } from './$types';
	import InputDate from '$lib/components/input-date.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { page } from '$app/state';
	import { Checkbox } from '$lib/components/checkbox';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	$effect(() => {
		context.hydrate();
	});
</script>

{JSON.stringify(context.fields)}
<svelte:boundary>
	{#snippet pending()}
		<Skeleton class="mx-auto mt-4 h-100 w-full max-w-lg" />
	{/snippet}
	<Card.Root class="relative mx-auto mt-4 w-full max-w-lg pt-0">
		<Thumbnail
			variant="cover"
			size={192}
			class="border-b"
			bind:value={context.fields.thumbnail}
			name="thumbnail"
			alt="Event thumbnail"
		/>
		<Card.Content>
			<Field.Set>
				<Field.Group class="gap-y-2">
					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Event Name</Field.Label>
						<Input
							id="name"
							type="text"
							maxlength={255}
							bind:value={context.fields.name}
							placeholder="Karaoke Night"
						/>
						<Field.Error errors={context.errors.name} />
					</Field.Field>
					<Field.Field data-invalid={!!context.errors.description}>
						<Field.Label for="description">
							Description
							<span class="ml-auto text-xs font-normal">
								{context.fields.description.length}/255
							</span>
						</Field.Label>

						<Textarea
							id="description"
							rows={3}
							class="max-h-52 resize-y"
							maxlength={255}
							bind:value={context.fields.description}
						/>
						<Field.Description>Short and readable description about the event.</Field.Description>
						<Field.Error errors={context.errors.description} />
					</Field.Field>

					<div class="grid grid-cols-2 gap-4">
						<Field.Field data-invalid={!!context.errors.startsAt}>
							<Field.Label for="starts-at">Starts At</Field.Label>
							<InputDate id="starts-at" bind:value={context.fields.startsAt} />
							<Field.Error errors={context.errors.startsAt} />
						</Field.Field>
						<Field.Field data-invalid={!!context.errors.endsAt}>
							<Field.Label for="ends-at">Ends At</Field.Label>
							<InputDate id="ends-at" bind:value={context.fields.endsAt} />
							<Field.Error errors={context.errors.endsAt} />
						</Field.Field>

						<Field.Field data-invalid={!!context.errors.capacity}>
							<Field.Label for="capacity">Capacity</Field.Label>
							<Input
								id="capacity"
								max={255}
								min={10}
								type="number"
								bind:value={context.fields.capacity}
								placeholder="25"
							/>
							<Field.Description>How many people may join this event.</Field.Description>
							<Field.Error errors={context.errors.capacity} />
						</Field.Field>

						<Field.Field>
							<Field.Label for="closes-at">Closes At</Field.Label>
							<InputDate id="closes-at" bind:value={context.fields.closesAt} class="w-full" />
							<Field.Description>When registrations close for this event.</Field.Description>
							<Field.Error errors={context.errors.closesAt} />
						</Field.Field>
					</div>

					<Separator />

					<Field.Field>
						<Field.Label for="threshold">Threshold</Field.Label>
						{#if page.data.session.roles.includes('staff')}
							<div class="flex items-center gap-2">
								<Checkbox
									id="threshold-optional"
									checked={context.fields.threshold === null}
									onCheckedChange={(checked: boolean) => {
										context.fields.threshold = checked ? null : context.fields.capacity;
									}}
								/>
								<label for="threshold-optional" class="text-sm font-normal text-muted-foreground">
									No minimum required
								</label>
							</div>

							{#if context.fields.threshold !== null}
								<Input
									id="threshold"
									max={255}
									min={10}
									type="number"
									bind:value={context.fields.threshold}
									placeholder="25"
								/>
							{/if}
						{:else}
							<Input
								id="threshold"
								max={255}
								min={10}
								type="number"
								required
								bind:value={context.fields.threshold}
								placeholder="25"
							/>
						{/if}

						<Field.Description>
							{#if page.data.session.roles.includes('staff')}
								Minimum required occupancy for the event to switch state. If not defined the event will skip proposal and become an upcoming event.
							{:else}
								Your event will start off as a proposal and if you meet the threshold it will be allowed to happen. Otherwise it will be rejected automatically.
							{/if}
						</Field.Description>
						<Field.Error errors={context.errors.threshold} />
					</Field.Field>
				</Field.Group>
			</Field.Set>
		</Card.Content>

		<Separator />

		<Card.Content>
			<Field.Set>
				<Field.Group class="w-full">
					<Field.Field class="w-full min-w-0" data-invalid={!!context.errors.markdown}>
						<Field.Label for="markdown">Event Details</Field.Label>
						<MarkdownTextarea id="markdown" bind:value={context.fields.markdown} />
						<Field.Description>
							Here you can line out what is going to happen in the event in detail.
						</Field.Description>
						<Field.Error errors={context.errors.markdown} />
					</Field.Field>
				</Field.Group>
			</Field.Set>
		</Card.Content>
		<Separator />

		<Card.Footer>
			<Button class="w-full" onclick={() => context.submit()}>
				{params.id ? 'Save Event' : 'Create Event'}
			</Button>
		</Card.Footer>
	</Card.Root>
</svelte:boundary>
