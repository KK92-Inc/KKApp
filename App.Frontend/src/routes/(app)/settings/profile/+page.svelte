<script lang="ts">
	import * as User from '$lib/remotes/user.remote';
	import * as Account from '$lib/remotes/account.remote';
	import * as Field from '$lib/components/field';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tooltip from '$lib/components/tooltip';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Input } from '$lib/components/input';
	import * as Select from '$lib/components/select';
	import { Textarea } from '$lib/components/textarea';
	import { Checkbox } from '$lib/components/checkbox';
	import { Button } from '$lib/components/button';
	import { useDialog } from '$lib/components/dialog';
	import { Separator } from '$lib/components/separator';
	import { CircleAlert, House, Save, Trash2 } from '@lucide/svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { page } from '$app/state';
	import * as Alert from '$lib/components/alert';
	import type { ValidationErrors } from '$lib/api';

	const dialog = useDialog();
	const user = await Account.get();

	const errors = $state<ValidationErrors>({});
	let avatar = $state<File | string | null>(user.avatarUrl);
	async function clearCache() {
		const confirm = await dialog.confirm(
			'Clear cache ?',
			'This action is non-destructive and clears local storage only.'
		);

		if (confirm) localStorage.clear();
	}

	async function submit() {
		User.update({
			userId: user.id,
			avatarUrl: avatar
		});
	}
</script>

<!-- Header settings -->
<div class="flex items-center justify-between gap-4 pb-2">
	<h1 class="text-xl font-bold">Account Settings</h1>
	<Separator class="flex-1" />
	<ButtonGroup.Root class="items-center">
		<ButtonGroup.Root>
			<Button size="sm" variant="outline" aria-label="Go to Profile">
				Go to my Profile
				<House />
			</Button>
		</ButtonGroup.Root>
		<Separator class="h-4!" orientation="vertical" />
		<ButtonGroup.Root>
			<Button size="sm" variant="secondary" aria-label="Delete Localstorage" onclick={clearCache}>
				Clear Cache
				<Trash2 />
			</Button>
			<Button size="sm" variant="default" aria-label="Update Profile" onclick={submit}>
				Update
				<Save />
			</Button>
		</ButtonGroup.Root>
	</ButtonGroup.Root>
</div>

<Field.Set>
	<Field.Group class="grid grid-cols-[auto_1fr] gap-2">
		<Field.Field>
			<Field.Label>Thumbnail</Field.Label>
			<Thumbnail class="min-w-52" bind:value={avatar} />
			<Field.Description>Your profile picture</Field.Description>
			<Field.Error />
		</Field.Field>

		<Field.Group class="grid grid-cols-2 grid-rows-[min-content_min-content] pl-2">
			<Field.Field class="col-span-2">
				<Field.Label for="id">User ID</Field.Label>
				<InputGroup.Root>
					<InputGroup.Input id="id" autocomplete="off" autocorrect="off" readonly disabled value={user.id} />
					<InputGroup.Addon align="inline-end">
						<InputGroup.Copy value={user.id} />
					</InputGroup.Addon>
				</InputGroup.Root>
				<Field.Description class="text-xs">Your unique, non-editable identifier</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label>Login</Field.Label>
				<Input value={user.login} />
				<Field.Description>Your Permanent login handle</Field.Description>
				<Field.Error />
			</Field.Field>

			<Field.Field>
				<Field.Label>Display</Field.Label>
				<Input value={user.displayName} />
				<Field.Description>Used to publicly display a alternative name</Field.Description>
				<Field.Error />
			</Field.Field>

			<Field.Field>
				<Field.Label>First Name</Field.Label>
				<Input value={user.login} />
				<Field.Description>Display your first name</Field.Description>
				<Field.Error />
			</Field.Field>

			<Field.Field>
				<Field.Label>Last Name</Field.Label>
				<Input value={user.displayName} />
				<Field.Description>Display your last name</Field.Description>
				<Field.Error />
			</Field.Field>
		</Field.Group>

		<div class="col-span-full flex items-center justify-between gap-4 pb-2">
			<h2 class="text-md font-bold">Social Settings</h2>
			<Separator class="flex-1" />
		</div>

		<Field.Group class="col-span-full grid grid-cols-1 gap-4 sm:grid-cols-2">
			<Field.Field>
				<Field.Label for="website">Website</Field.Label>
				<Input id="website" placeholder="https://example.com" autocomplete="off" autocorrect="off" />
				<Field.Description class="text-xs">Personal or project website URL</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="github">GitHub</Field.Label>
				<Input id="github" placeholder="https://github.com/username" autocomplete="off" autocorrect="off" />
				<Field.Description class="text-xs">Link to your GitHub profile</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="linkedin">LinkedIn</Field.Label>
				<Input
					id="linkedin"
					placeholder="https://linkedin.com/in/username"
					autocomplete="off"
					autocorrect="off"
				/>
				<Field.Description class="text-xs">Link to your LinkedIn profile</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="reddit">Reddit</Field.Label>
				<Input
					id="reddit"
					placeholder="https://reddit.com/user/username"
					autocomplete="off"
					autocorrect="off"
				/>
				<Field.Description class="text-xs">Link to your Reddit profile or user page</Field.Description>
			</Field.Field>
		</Field.Group>

		<Field.Separator class="col-span-full" />
		<Field.Field class="col-span-full">
			<Field.Label for="login">Biography</Field.Label>
			<Field.Description class="text-xs">You can write a small biography about yourself.</Field.Description>
			<MarkdownTextarea value={user.details?.markdown ?? ''} />
		</Field.Field>
	</Field.Group>
</Field.Set>
<Separator class="my-2" />
<Alert.Root variant="default">
	<CircleAlert />
	<Alert.Title>Permeating changes</Alert.Title>
	<Alert.Description>
		<p>Some fields may require you to logout and login again when changing them.</p>
	</Alert.Description>
</Alert.Root>
