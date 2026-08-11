<script lang="ts">
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { Button } from '$lib/components/button';
	import {
		Archive,
		ChevronDown,
		GraduationCap,
		MessageSquareCode,
		Plus,
		Trophy,
		Wrench
	} from '@lucide/svelte';
	import { page } from '$app/state';

	const permissions = $derived(page.data.session.permissions);
	const projects = $derived(permissions.includes('projects:write'));
	const goals = $derived(permissions.includes('goals:write'));
	const cursus = $derived(permissions.includes('cursus:write'));
	const rubrics = $derived(permissions.includes('rubrics:write'));
	const workspace = $derived(permissions.includes('workspaces:read'));
	const any = $derived(projects || goals || cursus || rubrics || workspace);
</script>

<DropdownMenu.Root>
	{#if any}
		<DropdownMenu.Trigger class="max-md:hidden">
			{#snippet child({ props })}
				<Button {...props} variant="outline">
					<Plus />
					<ChevronDown class="max-md:hidden" />
				</Button>
			{/snippet}
		</DropdownMenu.Trigger>
	{/if}

	<DropdownMenu.Content class="w-56" align="start">
		<DropdownMenu.Label>Creation Menu</DropdownMenu.Label>
		{#if projects || goals || cursus || rubrics}
			<DropdownMenu.Separator />
		{/if}
		<DropdownMenu.Group>
			{#if cursus}
				<DropdownMenu.Item href="/cursus/manage">
					<GraduationCap />
					New Cursus
				</DropdownMenu.Item>
			{/if}
			{#if goals}
				<DropdownMenu.Item href="/goals/manage">
					<Trophy />
					New Goal
				</DropdownMenu.Item>
			{/if}
			{#if projects}
				<DropdownMenu.Item href="/projects/manage">
					<Archive />
					New Project
				</DropdownMenu.Item>
			{/if}
			{#if rubrics}
				<DropdownMenu.Item href="/rubrics/manage">
					<MessageSquareCode />
					New Rubric
				</DropdownMenu.Item>
			{/if}
		</DropdownMenu.Group>
		{#if workspace}
			<DropdownMenu.Group>
				<DropdownMenu.Separator />
				<DropdownMenu.Item href="/workspace">
					<Wrench />
					View Workspace
				</DropdownMenu.Item>
			</DropdownMenu.Group>
		{/if}
	</DropdownMenu.Content>
</DropdownMenu.Root>
