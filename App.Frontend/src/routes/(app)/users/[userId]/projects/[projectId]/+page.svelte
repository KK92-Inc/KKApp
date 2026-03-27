<script lang="ts">
	import type { PageProps } from './$types';
	import * as Projects from '$lib/remotes/project.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Invites from '$lib/remotes/invite.remote';
	import Button from '$lib/components/button/button.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { invalidateAll } from '$app/navigation';

	const { data }: PageProps = $props();
</script>

{#each await UserProjects.members({ id: data.userProject.id }) as member}
	<div class="flex items-center gap-2">
		<Thumbnail readonly src="/placeholder.svg" class="size-24 shrink-0" />
		<span>{member.user.displayName}</span>
	</div>
{/each}

<!-- {#if data.userProject}
	<div class="flex gap-2">
		<Button
			onclick={async () => {
				await Invites.send({
					userProjectId: data.userProject.id,
					inviteeId: "74193b39-7c21-4711-9b8c-b8de3333ee88"
				}).updates(UserProjects.members({ id: data.userProject.id }));

				await invalidateAll();
			}}
		>
			Invite
		</Button>

		<Button
			onclick={async () => {
				await Invites.revoke({
					userProjectId: data.userProject.id,
					inviteeId: "74193b39-7c21-4711-9b8c-b8de3333ee88"
				}).updates(UserProjects.members({ id: data.userProject.id }));
			}}
		>
			Revoke Invite
		</Button>
	</div>
{/if} -->

{#if data.userProject}
	<div class="flex gap-2">
		<form {...Invites.send}>
			<input type="hidden" {...Invites.send.fields.userProjectId.as('text')} value={data.userProject.id} />
			<input type="hidden" {...Invites.send.fields.inviteeId.as('text')} value="74193b39-7c21-4711-9b8c-b8de3333ee88" />
			<Button type="submit">Send Invite</Button>
		</form>

		<form {...Invites.revoke}>
			<input type="hidden" {...Invites.revoke.fields.userProjectId.as('text')} value={data.userProject.id} />
			<input type="hidden" {...Invites.revoke.fields.inviteeId.as('text')} value="74193b39-7c21-4711-9b8c-b8de3333ee88" />
			<Button type="submit">Revoke Invite</Button>
		</form>
	</div>
{/if}

<pre><code>
{JSON.stringify(data, null, 2)}
</code></pre>
