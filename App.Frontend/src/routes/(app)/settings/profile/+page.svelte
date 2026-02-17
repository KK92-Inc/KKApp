<script lang="ts">
	import { page } from '$app/state';
	import { Button } from '$lib/components/button';
	import { Checkbox } from '$lib/components/checkbox';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import * as Select from '$lib/components/select';
	import { Textarea } from '$lib/components/textarea';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { buttonVariants } from '$lib/components/button/index.js';
	import { CircleQuestionMark, Copy, CopyCheck } from '@lucide/svelte';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import { fade } from 'svelte/transition';
	import { useDialog } from '$lib/components/dialog';

	const dialog = useDialog();
	async function kek() {
		await dialog.confirm();
	}
</script>

<form method="POST" enctype="multipart/form-data" class="rounded bg-card p-6">
	<Button onclick={kek}>Hello</Button>
	<Field.Set class="grid grid-cols-1 gap-6 md:grid-cols-3">
		<Field.Group>
			<Field.Field>
				<Field.Label>Avatar</Field.Label>
				<Field.Description>Upload a profile image.</Field.Description>
				<div class="flex flex-col items-center gap-4">
					<Thumbnail src="https://github.com/w2wizard.png" />
					<Field.Description class="text-xs text-muted-foreground"
						>PNG, JPG — max 2MB</Field.Description
					>
				</div>
			</Field.Field>

			<Field.Separator />

			<Field.Field>
				<Field.Label for="id">User ID</Field.Label>
				<InputGroup.Root>
					<InputGroup.Input
						id="id"
						autocomplete="off"
						autocorrect="off"
						readonly
						value={page.data.session.userId}
					/>
					<InputGroup.Addon align="inline-end">
						<InputGroup.Copy value={page.data.session.userId} />
					</InputGroup.Addon>
				</InputGroup.Root>
				<Field.Description class="text-xs">Your unique, non-editable identifier</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="login">Login</Field.Label>
				<Input
					id="login"
					type="text"
					name="login"
					disabled
					readonly
					value={page.data.session.username}
				/>
				<Field.Description class="text-xs">Your login handle</Field.Description>
			</Field.Field>
		</Field.Group>
	</Field.Set>
</form>

<!-- <form method="POST" enctype="multipart/form-data" class=" p-6 bg-card rounded-lg">
	<div class="grid grid-cols-1 md:grid-cols-3 gap-6">
		<div class="flex flex-col items-center gap-2 md:items-start">
			<Field.Field>
				<Field.Label>Avatar</Field.Label>
				<Field.Description>Upload a profile image that represents you.</Field.Description>
				<div class="flex items-center gap-4 flex-col">
					<Thumbnail src="https://github.com/w2wizard.png" />
					<Field.Description class="text-muted-foreground text-xs">PNG, JPG — max 2MB</Field.Description>
				</div>
			</Field.Field>

			<Field.Separator />

			<Field.Field>
				<Field.Label for="id">User ID</Field.Label>
				<Input id="id" type="text" name="id" disabled readonly value={page.data.session.userId} />
				<Field.Description class="text-xs">Your unique, non-editable identifier</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="login">Login</Field.Label>
				<Input id="login" type="text" name="login" disabled readonly value={page.data.session.username} />
				<Field.Description class="text-xs">Your login handle</Field.Description>
			</Field.Field>
		</div>

		<div class="md:col-span-2">
			<Field.Set>
				<Field.Legend>Profile Settings</Field.Legend>
				<Field.Description>Configure your profile: display name, links and biography.</Field.Description>

				<Field.Separator class="my-1" />

				<Field.Group>
					<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<Field.Field>
							<Field.Label for="displayName">Display name</Field.Label>
							<Input
								id="displayName"
								name="displayName"
								placeholder="x_johnny_silverhand_x"
								autocorrect="off"
								value={page.data.session.username}
								aria-invalid={page.data.session.username ? 'true' : undefined}
							/>
							<Field.Description class="text-xs">This name appears publicly</Field.Description>
						</Field.Field>

						<div class="grid grid-cols-2 gap-4">
							<Field.Field>
								<Field.Label for="firstName">First name</Field.Label>
								<Input id="firstName" name="firstName" placeholder="John" autocomplete="given-name" autocorrect="off" />
							</Field.Field>
							<Field.Field>
								<Field.Label for="lastName">Last name</Field.Label>
								<Input id="lastName" name="lastName" placeholder="Doe" autocomplete="family-name" autocorrect="off" />
							</Field.Field>
						</div>
					</div>

					<Field.Separator class="my-3" />

					<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<Field.Field>
							<Field.Label for="website">Website</Field.Label>
							<Input id="website" name="website" placeholder="https://example.com" autocomplete="off" autocorrect="off" />
						</Field.Field>

						<Field.Field>
							<Field.Label for="github">GitHub</Field.Label>
							<Input id="github" name="github" placeholder="https://github.com/username" autocomplete="off" autocorrect="off" />
						</Field.Field>

						<Field.Field>
							<Field.Label for="linkedin">LinkedIn</Field.Label>
							<Input id="linkedin" name="linkedin" placeholder="https://linkedin.com/in/username" autocomplete="off" autocorrect="off" />
						</Field.Field>

						<Field.Field>
							<Field.Label for="reddit">Reddit</Field.Label>
							<Input id="reddit" name="reddit" placeholder="https://reddit.com/user/username" autocomplete="off" autocorrect="off" />
						</Field.Field>
					</div>

					<Field.Separator class="my-3" />

					<Field.Field>
						<Field.Label for="birthday">Birthday</Field.Label>
						<Field.Description class="text-xs">Optional — used to show age or for birthday badges</Field.Description>
						<div class="grid grid-cols-3 gap-3 mt-2 items-center">
							<Field.Field>
								<Field.Label for="exp-month" class="sr-only">Month</Field.Label>
								<Select.Root type="single" bind:value={month}>
									<Select.Trigger id="exp-month" class="min-w-[72px]">
										<span>{month || 'MM'}</span>
									</Select.Trigger>
									<Select.Content>
										{#each Array.from({ length: 12 }, (_, i) => String(i + 1).padStart(2, '0')) as m}
											<Select.Item value={m}>{m}</Select.Item>
										{/each}
									</Select.Content>
								</Select.Root>
							</Field.Field>

							<Field.Field>
								<Field.Label for="exp-year" class="sr-only">Year</Field.Label>
								<Select.Root type="single" bind:value={year}>
									<Select.Trigger id="exp-year" class="min-w-[88px]">
										<span>{year || 'YYYY'}</span>
									</Select.Trigger>
									<Select.Content>
										{#each [2024,2025,2026,2027,2028,2029] as y}
											<Select.Item value={String(y)}>{y}</Select.Item>
										{/each}
									</Select.Content>
								</Select.Root>
							</Field.Field>

							<Field.Field>
								<Field.Label for="tz" class="sr-only">Timezone</Field.Label>
								<Input id="tz" name="timezone" placeholder="Timezone (optional)" />
							</Field.Field>
						</div>
					</Field.Field>

					<Field.Separator class="my-3" />

					<Field.Field>
						<Field.Label for="markdown">Biography</Field.Label>
						<Field.Description class="text-xs">Write a short bio — Markdown supported</Field.Description>
						<Textarea
							id="markdown"
							name="markdown"
							placeholder="# This project is about..."
							class="mt-2 min-h-[120px] max-h-[420px] overflow-y-auto"
						/>
					</Field.Field>

				</Field.Group>

				<Field.Separator class="my-4" />

				<div class="flex flex-col sm:flex-row sm:justify-between gap-3">
					<div class="flex gap-2">
						<Button type="submit">Save changes</Button>
						<Button type="button" variant="destructive">Delete cache</Button>
					</div>

					<div class="text-sm text-muted-foreground mt-2 sm:mt-0">
						<span class="font-medium">Last saved:</span> <span>— never</span>
					</div>
				</div>
			</Field.Set>
		</div>
	</div>
</form> -->
