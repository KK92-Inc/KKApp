<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import Separator from '$lib/components/separator/separator.svelte';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import Slider from '$lib/components/slider/slider.svelte';
</script>

<Card.Root class="relative mx-auto mt-8 w-full max-w-lg pt-0">
	<div class="absolute inset-0 z-30 aspect-video bg-black/35"></div>
	<img
		src="https://avatar.vercel.sh/shadcn1"
		alt="Event cover"
		class="relative z-20 aspect-video w-full object-cover brightness-60 grayscale dark:brightness-40"
	/>
	<Card.Header>
		<Card.Action>
			<Badge variant="secondary">Proposal</Badge>
		</Card.Action>
		<Card.Title>Propose an Event</Card.Title>
		<Card.Description>
			If you have a fun event idea you would like to organize, you can propose it here.
		</Card.Description>
	</Card.Header>

	<Separator />

	<Card.Content>
		<Field.Set>
			<Field.Group>
				<Field.Field>
					<Field.Label for="street">Event Name</Field.Label>
					<Input id="street" type="text" placeholder="Karaoke Night" />
				</Field.Field>
				<Field.Field>
					<Field.Label for="description">
						Description
						<span class="ml-auto text-xs font-normal">
							{0}/255
						</span>
					</Field.Label>

					<Textarea id="description" rows={3} class="max-h-52 resize-y" maxlength={255} />
					<Field.Description>Short and readable description about the cursus.</Field.Description>
				</Field.Field>
				<div class="grid grid-cols-2 gap-4">
					<Field.Field>
						<Field.Label for="city">City</Field.Label>
						<Input id="city" type="datetime-local" placeholder="New York" />
					</Field.Field>
					<Field.Field>
						<Field.Label for="zip">Postal Code</Field.Label>
						<Input id="zip" type="datetime-local" placeholder="90502" />
					</Field.Field>

					<Field.Field>
						<Field.Label for="city">Capacity</Field.Label>
						<Input id="zip" max={255} min={10} type="number" placeholder="25" />
					</Field.Field>
					<Field.Field>
						<Field.Label for="zip">Closes At</Field.Label>
						<Input id="zip" type="datetime-local" placeholder="90502" />
					</Field.Field>
				</div>
			</Field.Group>
		</Field.Set>
	</Card.Content>

	<Separator />

	<Card.Content>
		<Field.Set>
			<Field.Field>
				<Field.Label for="city">Event Details</Field.Label>
				<MarkdownTextarea />
				<Field.Description
					>Here you can line out what is going to happen in the event in detail.</Field.Description
				>
			</Field.Field>
		</Field.Set>
	</Card.Content>
	<Separator />

	<Card.Footer>
		<Button class="w-full">View Event</Button>
	</Card.Footer>
</Card.Root>
