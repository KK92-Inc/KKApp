<script lang="ts">
	import { parseGitTree } from '$lib/components/explorer';
	import * as Explorer from '$lib/components/explorer';
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const tree = $derived(Git.treeViaUser({
		projectId: params.projectId,
		id: params.userId,
		branch: params.branch,
		path: params.path
	}));

	const files = $derived(parseGitTree(await tree));
</script>

<Explorer.Browser baseUrl="./src" dotdot nodes={files} />
