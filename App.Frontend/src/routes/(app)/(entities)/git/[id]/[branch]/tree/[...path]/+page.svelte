<script lang="ts">
	import { parseGitTree } from '$lib/components/explorer';
	import * as Explorer from '$lib/components/explorer';
	import * as Git from '$lib/remotes/git.remote';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const nodes = $derived(parseGitTree(await Git.tree({
		id: params.id,
		branch: params.branch,
		path: params.path
	})));

</script>

<Explorer.Browser baseUrl="./src" dotdot {nodes} />
