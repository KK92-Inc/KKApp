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
</script>

<DropdownMenu.Root>
	<DropdownMenu.Trigger class="max-md:hidden">
		{#snippet child({ props })}
			<Button {...props} variant="outline">
				<Plus />
				<ChevronDown class="max-md:hidden"/>
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content class="w-56" align="start">
		<DropdownMenu.Label>Creation Menu</DropdownMenu.Label>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Item href="/cursus/create">
				<GraduationCap />
				New Cursus
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/goals/create">
				<Trophy />
				New Goal
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/projects/create">
				<Archive />
				New Project
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/rubrics/create">
				<MessageSquareCode />
				New Rubric
			</DropdownMenu.Item>
		</DropdownMenu.Group>
		<DropdownMenu.Group>
			<DropdownMenu.Separator />
			<DropdownMenu.Item href="/workspace">
				<Wrench />
				View Workspace
			</DropdownMenu.Item>
		</DropdownMenu.Group>
	</DropdownMenu.Content>
</DropdownMenu.Root>
