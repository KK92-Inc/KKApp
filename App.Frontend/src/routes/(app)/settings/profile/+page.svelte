<script lang="ts">
	import { page } from '$app/state';
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import { Input } from '$lib/components/input';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Save, Trash2 } from '@lucide/svelte';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import { fade } from 'svelte/transition';
	import { useDialog } from '$lib/components/dialog';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { Separator } from '$lib/components/separator';
	import { updateUser } from '$lib/remotes/account.remote';
	import { getUser } from '$lib/remotes/user.remote';

	const dialog = useDialog();
	const user = $derived(await getUser(page.data.session.userId));
	$effect(() => {
		updateUser.fields.set({
			avatarUrl: user.avatarUrl,
			displayName: user.displayName,
			details: {
				firstName: user.details?.firstName,
				lastName: user.details?.lastName,
				websiteUrl: user.details?.websiteUrl,
				githubUrl: user.details?.githubUrl,
				linkedinUrl: user.details?.linkedinUrl,
				redditUrl: user.details?.redditUrl
			}
		});
	});
</script>

{#each updateUser.fields.allIssues() as issue}
	<p>{issue.message}</p>
{/each}

<svelte:boundary>
	{#snippet pending()}
		Please wait...
	{/snippet}
	<form {...updateUser} enctype="multipart/form-data">
		<div class="flex items-center justify-between gap-1 pb-2">
			<h1 class="text-xl font-bold">Account Settings</h1>
			<Separator class="w-min flex-1" />
			<Button
				type="button"
				variant="outline"
				onclick={async () =>
					await dialog
						.confirm('Clear Cache ?', 'Clearing your cache removes the local browsing history')
						.ok(() => localStorage.clear())}
			>
				Clear Cache
				<Trash2 />
			</Button>
			<Button type="submit" variant="outline" class="w-min">
				Update Profile
				<Save />
			</Button>
		</div>

		<Field.Set class="grid grid-cols-1 gap-6 rounded bg-card p-6 md:grid-cols-4">
			<Field.Group>
				<Field.Field>
					<Field.Label>Avatar</Field.Label>
					<Field.Description>Upload a profile image.</Field.Description>
					<Thumbnail src="https://github.com/w2wizard.png" {...updateUser.fields.avatarUrl.as('url')} />
				</Field.Field>

				<Field.Separator />

				<Field.Field>
					<Field.Label for="id">User ID</Field.Label>
					<InputGroup.Root>
						<InputGroup.Input id="id" autocomplete="off" autocorrect="off" readonly value={user.id} />
						<InputGroup.Addon align="inline-end">
							<InputGroup.Copy value={user.id} />
						</InputGroup.Addon>
					</InputGroup.Root>
					<Field.Description class="text-xs">Your unique, non-editable identifier</Field.Description>
				</Field.Field>

				<Field.Field>
					<Field.Label for="login">Login</Field.Label>
					<Input id="login" type="text" name="login" disabled readonly value={user.login} />
					<Field.Description class="text-xs">Your login handle</Field.Description>
				</Field.Field>
			</Field.Group>

			<Field.Group class="md:col-span-3">
				<Field.Field>
					<Field.Label for="displayName">Display name</Field.Label>
					<Field.Description class="text-xs">This name appears publicly</Field.Description>
					<Input
						id="displayName"
						placeholder="x_johnny_silverhand_x"
						autocorrect="off"
						{...updateUser.fields.displayName.as('text')}
					/>
				</Field.Field>

				<Field.Group class="grid grid-cols-2 gap-4">
					<Field.Field>
						<Field.Label for="firstName">First name</Field.Label>
						<Input
							id="firstName"
							placeholder="John"
							autocomplete="given-name"
							autocorrect="off"
							{...updateUser.fields.details.firstName.as('text')}
						/>
					</Field.Field>
					<Field.Field>
						<Field.Label for="lastName">Last name</Field.Label>
						<Input
							id="lastName"
							placeholder="Doe"
							autocomplete="family-name"
							autocorrect="off"
							{...updateUser.fields.details.lastName.as('text')}
						/>
					</Field.Field>
				</Field.Group>

				<Field.Separator />

				<Field.Group class="grid grid-cols-1 gap-4 sm:grid-cols-2">
					<Field.Field>
						<Field.Label for="website">Website</Field.Label>
						<Field.Description class="text-xs">Personal or project website URL</Field.Description>
						<Input
							id="website"
							placeholder="https://example.com"
							autocomplete="off"
							autocorrect="off"
							{...updateUser.fields.details.websiteUrl.as('text')}
						/>
					</Field.Field>

					<Field.Field>
						<Field.Label for="github">GitHub</Field.Label>
						<Field.Description class="text-xs">Link to your GitHub profile</Field.Description>
						<Input
							id="github"
							placeholder="https://github.com/username"
							autocomplete="off"
							autocorrect="off"
							{...updateUser.fields.details.githubUrl.as('text')}
						/>
					</Field.Field>

					<Field.Field>
						<Field.Label for="linkedin">LinkedIn</Field.Label>
						<Field.Description class="text-xs">Link to your LinkedIn profile</Field.Description>
						<Input
							id="linkedin"
							placeholder="https://linkedin.com/in/username"
							autocomplete="off"
							autocorrect="off"
							{...updateUser.fields.details.linkedinUrl.as('text')}
						/>
					</Field.Field>

					<Field.Field>
						<Field.Label for="reddit">Reddit</Field.Label>
						<Field.Description class="text-xs">Link to your Reddit profile or user page</Field.Description>
						<Input
							id="reddit"
							placeholder="https://reddit.com/user/username"
							autocomplete="off"
							autocorrect="off"
							{...updateUser.fields.details.redditUrl.as('text')}
						/>
					</Field.Field>
				</Field.Group>

				<Field.Separator />

				<Field.Field>
					<Field.Label for="login">Biography</Field.Label>
					<Field.Description class="text-xs"
						>You can write a small biography about yourself.</Field.Description
					>
					<MarkdownTextarea {...updateUser.fields.details.markdown.as('text')} />
				</Field.Field>
			</Field.Group>
		</Field.Set>
	</form>
</svelte:boundary>
