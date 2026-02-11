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
	<DropdownMenu.Trigger>
		{#snippet child({ props })}
			<Button {...props} variant="outline">
				<Plus />
				<ChevronDown />
			</Button>
		{/snippet}
	</DropdownMenu.Trigger>
	<DropdownMenu.Content class="w-56" align="start">
		<DropdownMenu.Label>Creation Menu</DropdownMenu.Label>
		<DropdownMenu.Separator />
		<DropdownMenu.Group>
			<DropdownMenu.Item href="/cursus/new">
				<GraduationCap />
				New Cursus
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/goals/new">
				<Trophy />
				New Goal
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/projects/new">
				<Archive />
				New Project
			</DropdownMenu.Item>
			<DropdownMenu.Item href="/rubrics/new">
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
