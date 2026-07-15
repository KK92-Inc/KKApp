<script lang="ts">
	import * as User from '$lib/remotes/user.remote';
	import * as Account from '$lib/remotes/account.remote';
	import * as Field from '$lib/components/field';
	import * as InputGroup from '$lib/components/input-group';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Input } from '$lib/components/input';
	import { Button } from '$lib/components/button';
	import { useDialog } from '$lib/components/dialog';
	import { Separator } from '$lib/components/separator';
	import { CircleAlert, House, Save, Trash2 } from '@lucide/svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import * as Alert from '$lib/components/alert';
	import { Problem, type ValidationErrors } from '$lib/api';
	import { toast } from 'svelte-sonner';

	const dialog = useDialog();
	const user = await Account.get();

	let errors = $state<ValidationErrors>({});
	let avatar = $state<File | string | null>(user.avatarUrl);
	async function clearCache() {
		const confirm = await dialog.confirm(
			'Clear cache ?',
			'This action is non-destructive and clears local storage only.'
		);

		if (confirm) localStorage.clear();
	}

	function normalize(value?: string | null): string | null {
		const trimmed = value?.trim();
		return trimmed ? trimmed : null;
	}

	let details = $state({
		firstName: user.details?.firstName ?? '',
		lastName: user.details?.lastName ?? '',
		markdown: user.details?.markdown ?? '',
		websiteUrl: user.details?.websiteUrl ?? '',
		githubUrl: user.details?.githubUrl ?? '',
		linkedinUrl: user.details?.linkedinUrl ?? '',
		redditUrl: user.details?.redditUrl ?? ''
	});

	async function submit() {
		try {
			await User.update({
				userId: user.id,
				avatarUrl: avatar,
				details: {
					firstName: normalize(details.firstName),
					lastName: normalize(details.lastName),
					markdown: normalize(details.markdown),
					websiteUrl: normalize(details.websiteUrl),
					githubUrl: normalize(details.githubUrl),
					linkedinUrl: normalize(details.linkedinUrl),
					redditUrl: normalize(details.redditUrl)
				}
			}).updates(Account.get());
		} catch (e) {
			const resolved = Problem.resolve(e);
			if (resolved.kind === 'validation') {
				errors = resolved.fields;
			} else {
				toast.error(resolved.message);
			}
		}
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
			<Thumbnail size={256} bind:value={avatar} />
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
				<Input readonly disabled value={user.login} />
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
				<Input bind:value={details.firstName} />
				<Field.Description>Display your first name</Field.Description>
				<Field.Error />
			</Field.Field>

			<Field.Field>
				<Field.Label>Last Name</Field.Label>
				<Input bind:value={details.lastName} />
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
				<Input
					id="website"
					bind:value={details.websiteUrl}
					placeholder="https://example.com"
					autocomplete="off"
					autocorrect="off"
				/>
				{#if errors['details.WebsiteUrl']}
					<Field.Error errors={errors['details.WebsiteUrl']} class="text-xs" />
				{:else}
					<Field.Description class="text-xs">Personal or project website URL</Field.Description>
				{/if}
			</Field.Field>

			<Field.Field>
				<Field.Label for="github">GitHub</Field.Label>
				<Input
					id="github"
					bind:value={details.githubUrl}
					placeholder="https://github.com/username"
					autocomplete="off"
					autocorrect="off"
				/>
				{#if errors['details.GithubUrl']}
					<Field.Error errors={errors['details.GithubUrl']} class="text-xs" />
				{:else}
					<Field.Description class="text-xs">Link to your GitHub profile</Field.Description>
				{/if}
			</Field.Field>

			<Field.Field>
				<Field.Label for="linkedin">LinkedIn</Field.Label>
				<Input
					id="linkedin"
					bind:value={details.linkedinUrl}
					placeholder="https://linkedin.com/in/username"
					autocomplete="off"
					autocorrect="off"
				/>
				{#if errors['details.LinkedinUrl']}
					<Field.Error errors={errors['details.LinkedinUrl']} class="text-xs" />
				{:else}
					<Field.Description class="text-xs">Link to your LinkedIn profile</Field.Description>
				{/if}
			</Field.Field>

			<Field.Field>
				<Field.Label for="reddit">Reddit</Field.Label>
				<Input
					id="reddit"
					bind:value={details.redditUrl}
					placeholder="https://reddit.com/user/username"
					autocomplete="off"
					autocorrect="off"
				/>
				{#if errors['details.RedditUrl']}
					<Field.Error errors={errors['details.RedditUrl']} class="text-xs" />
				{:else}
					<Field.Description class="text-xs">Link to your Reddit profile or user page</Field.Description>
				{/if}
			</Field.Field>
		</Field.Group>

		<Field.Separator class="col-span-full" />
		<Field.Field class="col-span-full">
			<Field.Label for="login">Biography</Field.Label>
			<Field.Description class="text-xs">You can write a small biography about yourself.</Field.Description>
			<MarkdownTextarea bind:value={details.markdown} maxlength={2000} />
			<Field.Error errors={errors['details.Markdown']} class="text-xs" />
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
