<script lang="ts">
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import * as Page from './context.svelte';
	import * as Card from '$lib/components/card';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import * as Alert from '$lib/components/alert';
	import * as InputGroup from '$lib/components/input-group';
	import { AtSign, GraduationCap, Mail, RotateCcw, ShieldUser, User } from '@lucide/svelte';
	import type { PageProps } from './$types';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	$effect(() => {
		context.hydrate();
	});

	const kebab = (s: string) =>
		s
			.normalize('NFD')
			.replace(/\p{M}/gu, '')
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, '-')
			.replace(/^-+|-+$/g, '');

	// First initial + last name in kebab-case, capped at 8 characters: "Jane" "Van der Berg" -> "jvan-der"
	const generated = $derived(
		(kebab(context.fields.firstName ?? '')[0] ?? '') +
			kebab(context.fields.lastName ?? '')
				.slice(0, 7)
				.replace(/-+$/, '')
	);

	// The login lives in the context now, so "follow the generated value until the user types"
	// is done with an effect. It only re-runs when the names change (it never reads `login`),
	// which gives the same behavior as before: manual edits stick until first/last name change again.
	// Skipped when editing, so an existing user's login is never overwritten.
	$effect(() => {
		if (params.id) return;
		context.fields.login = generated;
	});

	// Tabs hands back a plain string, the context wants the role union.
	const setRole = (value: string) => (context.role = value as Page.Context['role']);
</script>

<Card.Root class="relative mx-auto mt-8 w-full max-w-md gap-4 pt-0">
	<div
		class="relative flex flex-col items-center gap-3 border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
	>
		<Thumbnail capture size={256} bind:value={context.fields.avatarUrl} />
	</div>

	<Card.Content>
		<Field.Group>
			<Field.Field>
				<Field.Label for="email">Email</Field.Label>
				<InputGroup.Root>
					<InputGroup.Input
						id="email"
						type="email"
						bind:value={context.fields.email}
						maxlength={100}
						placeholder="jane.doe@example.com"
						autocomplete="off"
					/>
					<InputGroup.Addon>
						<Mail />
					</InputGroup.Addon>
				</InputGroup.Root>
				<Field.Error errors={context.errors['email']} />
			</Field.Field>

			<div class="grid grid-cols-2 gap-4">
				<Field.Field>
					<Field.Label for="first-name">First name</Field.Label>
					<Input
						id="first-name"
						bind:value={context.fields.firstName}
						maxlength={255}
						placeholder="Jane"
						autocomplete="off"
					/>
					<Field.Error errors={context.errors['firstName']} />
				</Field.Field>

				<Field.Field>
					<Field.Label for="last-name">Last name</Field.Label>
					<Input
						id="last-name"
						bind:value={context.fields.lastName}
						maxlength={255}
						placeholder="Doe"
						autocomplete="off"
					/>
					<Field.Error errors={context.errors['lastName']} />
				</Field.Field>
			</div>

			<!-- TODO: Ability to role manage afterwards ? -->
			{#if !params.id}
				<Field.Field>
					<Field.Label for="user-type">User type</Field.Label>
					<Tabs.Root bind:value={() => context.role, setRole} class="w-full">
						<Tabs.List class="grid w-full grid-cols-3">
							<Tabs.Trigger value="Applicant">Applicant <User /></Tabs.Trigger>
							<Tabs.Trigger value="Student">Student <GraduationCap /></Tabs.Trigger>
							<Tabs.Trigger value="Staff">Staff <ShieldUser /></Tabs.Trigger>
						</Tabs.List>
						<Tabs.Content value="Applicant">
							<Alert.Root>
								<User />
								<Alert.Title>About Applicants</Alert.Title>
								<Alert.Description>
									Applicants have limited access and can be assigned to a kickoff to launch their official
									starting date.
								</Alert.Description>
							</Alert.Root>
						</Tabs.Content>
						<Tabs.Content value="Student">
							<Alert.Root>
								<GraduationCap />
								<Alert.Title>About Students</Alert.Title>
								<Alert.Description>
									Students are the standard role in app. They may subscribe to other projects, goals and
									available cursi.
								</Alert.Description>
							</Alert.Root>
						</Tabs.Content>
						<Tabs.Content value="Staff">
							<Alert.Root>
								<ShieldUser />
								<Alert.Title>About Staff</Alert.Title>
								<Alert.Description>
									Staff have limitless access and can manage accounts, events, kickoffs, etc.
								</Alert.Description>
							</Alert.Root>
						</Tabs.Content>
					</Tabs.Root>
				</Field.Field>
			{/if}
		</Field.Group>
	</Card.Content>

	<Separator />
	<Card.Content>
		<Field.Group>
			<Field.Field>
				<Field.Label for="login">Login</Field.Label>
				{#if !params.id}
					<InputGroup.Root>
						<InputGroup.Input
							id="login"
							bind:value={context.fields.login}
							maxlength={255}
							placeholder="jdoe"
							autocomplete="off"
							autocapitalize="none"
							spellcheck={false}
						/>
						<InputGroup.Addon>
							<AtSign />
						</InputGroup.Addon>
						{#if context.fields.login !== generated}
							<InputGroup.Addon align="inline-end">
								<InputGroup.Button
									type="button"
									size="icon-xs"
									aria-label="Reset to generated login"
									title="Reset to generated login"
									onclick={() => (context.fields.login = generated)}
								>
									<RotateCcw />
								</InputGroup.Button>
							</InputGroup.Addon>
						{/if}
					</InputGroup.Root>
				{:else}
					<InputGroup.Root>
						<InputGroup.Input
							id="login"
							disabled
							value={context.fields.login}
							maxlength={255}
							placeholder="jdoe"
							autocomplete="off"
							autocapitalize="none"
							spellcheck={false}
						/>
						<InputGroup.Addon>
							<AtSign />
						</InputGroup.Addon>
					</InputGroup.Root>
				{/if}
				<Field.Description>Generated from the name above. Edit it to override.</Field.Description>
				<Field.Error errors={context.errors['login']} />
			</Field.Field>
		</Field.Group>
	</Card.Content>

	<Separator />

	<Card.Footer>
		<Button type="submit" class="w-full" onclick={() => context.submit()}>
			{params.id ? 'Update' : 'Create'}
		</Button>
	</Card.Footer>
</Card.Root>
