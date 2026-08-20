<script lang="ts">
	import CheckIcon from '@lucide/svelte/icons/check';
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down';
	import { tick } from 'svelte';
	import * as Page from './context.svelte';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Rubric from '$lib/remotes/rubric.remote';
	import * as Command from '$lib/components/command';
	import * as Popover from '$lib/components/popover';
	import { Button } from '$lib/components/button';
	import { cn } from '$lib/utils.js';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Loader from '$lib/components/loader.svelte';
	import { CircleAlert, Dices } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';

	const ctx = Page.getContext();

	let query = $state<string>();
	const update = useDebounce((v: string) => (query = v), 250);

	let open = $state(false);
	let triggerRef = $state<HTMLButtonElement>(null!);

	function closeAndFocusTrigger() {
		open = false;
		tick().then(() => {
			triggerRef.focus();
		});
	}
</script>

<div class="flex flex-col gap-2">
	<svelte:boundary>
		{@const projects = await Projects.getPage({ name: query })}
		{@const entries = projects.data.map((v) => {
			return { value: v.id, label: v.name };
		})}

		{#snippet pending()}
			<span class="flex items-center gap-2">
				<Loader /> Please wait...
			</span>
		{/snippet}

		<Popover.Root bind:open>
			<Popover.Trigger bind:ref={triggerRef}>
				{#snippet child({ props })}
					<Button
						{...props}
						variant="outline"
						class={cn('w-full justify-between transition-colors')}
						role="combobox"
						aria-expanded={open}
					>
						<span>
							{#if ctx.fields.projectId === null}
								Wildcard Rubric
							{:else}
								{entries.find((f) => f.value === ctx.fields.projectId)?.label ?? 'Select a project...'}
							{/if}
						</span>

						<ChevronsUpDownIcon class={cn('size-4 opacity-50')} />
					</Button>
				{/snippet}
			</Popover.Trigger>

			<Popover.Content class="p-0" align="start">
				<svelte:boundary>
					{#snippet pending()}
						<div class="p-4 text-center text-xs text-muted-foreground">Loading projects...</div>
					{/snippet}

					<Command.Root>
						<Command.Input
							placeholder="Search project..."
							oninput={(e) => update.fn(e.currentTarget.value)}
						/>
						<Command.List>
							<Command.Empty>No Projects Available</Command.Empty>
							<Command.Group value="projects">
								<Command.Item
									value="wildcard"
									keywords={['wildcard', 'default']}
									onSelect={() => {
										ctx.fields.projectId = null;
										closeAndFocusTrigger();
									}}
								>
									<CheckIcon class={cn('mr-2 size-4', ctx.fields.projectId !== null && 'text-transparent')} />
									<span class="flex flex-1 flex-col items-start">
										<span>Wildcard Rubric</span>
										<span class="text-xs text-muted-foreground"> Apply to all projects as rubric. </span>
									</span>
									<Dices class="size-4 shrink-0 opacity-50" />
								</Command.Item>

								{#if entries.length > 0}
									<Command.Separator />
								{/if}
								{#each entries as project (project.value)}
									<Command.Item
										value={project.value}
										keywords={[project.label]}
										onSelect={() => {
											ctx.fields.projectId = project.value;
											closeAndFocusTrigger();
										}}
									>
										<CheckIcon
											class={cn('mr-2 size-4', ctx.fields.projectId !== project.value && 'text-transparent')}
										/>
										{project.label}
									</Command.Item>
								{/each}
							</Command.Group>
						</Command.List>
					</Command.Root>
				</svelte:boundary>
			</Popover.Content>
		</Popover.Root>

		{#if ctx.fields.projectId === null}
			<Alert.Root>
				<Dices />
				<Alert.Title>About Wildcard Rubrics</Alert.Title>
				<Alert.Description>
					<p>A wildcard rubric is a fallback rubric used if there are no project specific rubrics available.</p>
					<!-- <ul class="list-inside list-disc text-sm">
						<li>There can only ever be a one wildcard rubric</li>
						<li>Ensure sufficient funds</li>
						<li>Verify billing address</li>
					</ul> -->
				</Alert.Description>
			</Alert.Root>
		{/if}
	</svelte:boundary>
</div>
