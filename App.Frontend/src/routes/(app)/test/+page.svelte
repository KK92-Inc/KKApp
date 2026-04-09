<script lang="ts">
	import * as Page from './page.svelte';
	import * as Projects from '$lib/remotes/project.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { page } from '$app/state';

	const context = Page.setContext(new Page.Context());
	const data = $derived(Projects.getPage({  }).then((v) => v.data));
</script>

<svelte:boundary>
	<ul>
		{#each await data as p (p.id)}
			<li><a href="/projects/{p.slug}">{p.name}</a></li>
		{:else}
			<li>No projects found.</li>
		{/each}
	</ul>

	{#snippet pending()}
		<p>loading...</p>
	{/snippet}
</svelte:boundary>
