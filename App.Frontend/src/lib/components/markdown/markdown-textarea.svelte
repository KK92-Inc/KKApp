<script lang="ts">
	import { Textarea } from '../textarea';
	import { cn } from 'tailwind-variants';
	import { plugins } from './context.svelte';
	import { buttonVariants } from '../button';
	import * as Popover from '../popover';
	import {
		Bold,
		Italic,
		Strikethrough,
		Link,
		List,
		ListOrdered,
		Code,
		Heading,
		Quote,
		Ellipsis
	} from '@lucide/svelte';
	import type { Component } from 'svelte';
	import * as Tabs from '$lib/components/tabs';

	interface Props {
		value?: string;
		class?: string;
	}

	interface Shortcut {
		icon: Component<{ size?: number; class?: string }>;
		label: string;
		action: () => void;
	}

	// State

	let textarea = $state<HTMLTextAreaElement>(null!);
	let { class: className, value = $bindable('') }: Props = $props();

	let mode = $state<'write' | 'preview'>('write');

	function wrapSelection(before: string, after: string) {
		if (!textarea) return;
		const start = textarea.selectionStart;
		const end = textarea.selectionEnd;
		const selected = value.slice(start, end);
		value = value.slice(0, start) + before + selected + after + value.slice(end);
		textarea.focus();
		requestAnimationFrame(() => {
			textarea.selectionStart = start + before.length;
			textarea.selectionEnd = end + before.length;
		});
	}

	function prefixLine(prefix: string) {
		if (!textarea) return;
		const start = textarea.selectionStart;
		const lineStart = value.lastIndexOf('\n', start - 1) + 1;
		value = value.slice(0, lineStart) + prefix + value.slice(lineStart);
		textarea.focus();
		requestAnimationFrame(() => {
			textarea.selectionStart = start + prefix.length;
			textarea.selectionEnd = start + prefix.length;
		});
	}

	const shortcuts: Shortcut[] = [
		{ icon: Heading, label: 'Heading', action: () => prefixLine('### ') },
		{ icon: Bold, label: 'Bold', action: () => wrapSelection('**', '**') },
		{ icon: Italic, label: 'Italic', action: () => wrapSelection('_', '_') },
		{ icon: Strikethrough, label: 'Strikethrough', action: () => wrapSelection('~~', '~~') },
		{ icon: Link, label: 'Link', action: () => wrapSelection('[', '](url)') },
		{ icon: Code, label: 'Code', action: () => wrapSelection('`', '`') },
		{ icon: List, label: 'Unordered list', action: () => prefixLine('- ') },
		{ icon: ListOrdered, label: 'Ordered list', action: () => prefixLine('1. ') },
		{ icon: Quote, label: 'Quote', action: () => prefixLine('> ') }
	];
</script>

{#snippet shortcutButton(props: Shortcut)}
	<button
		type="button"
		class={buttonVariants({
			variant: 'ghost',
			size: 'icon',
			class: 'shadow-none'
		})}
		title={props.label}
		onclick={() => {
			props.action();
			mode = 'write';
		}}
	>
		<props.icon size={16} class="m-auto" />
	</button>
{/snippet}

<Tabs.Root value="account" class={cn("gap-0", className)}>
	<Tabs.List class="h-fit w-full justify-between rounded-b-none border border-b-0">
		<div class="w-fit p-1">
			<Tabs.Trigger value="account">Write</Tabs.Trigger>
			<Tabs.Trigger value="password">Preview</Tabs.Trigger>
		</div>
		<menu class="flex items-center gap-px">
			{#each shortcuts.slice(0, 5) as props}
				{@render shortcutButton(props)}
			{/each}
			{#if shortcuts.length > 5}
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({
							variant: 'ghost',
							size: 'icon',
							class: 'shadow-none'
						})}
					>
						<Ellipsis size={16} class="m-auto" />
					</Popover.Trigger>
					<Popover.Content class="w-10 p-2 py-1">
						<menu class="flex flex-col items-center gap-1">
							{#each shortcuts.slice(5) as props}
								{@render shortcutButton(props)}
							{/each}
						</menu>
					</Popover.Content>
				</Popover.Root>
			{/if}
		</menu>
	</Tabs.List>
	<Tabs.Content value="account">
		<Textarea
			data-mode={mode}
			bind:ref={textarea}
			draggable="false"
			bind:value
			class="rounded-t-none shadow-none focus-visible:ring-0"
		/>
	</Tabs.Content>
	<Tabs.Content value="password" class={cn('markdown rounded-t-none border-0 dark:bg-input/30 ')}>
		<!-- <Markdown md={value} /> -->
	</Tabs.Content>
</Tabs.Root>

<!-- <div class={cn('rounded-md border', className)}>
	<div class="flex items-center justify-between border-b px-2 py-1">
		<menu class="flex items-center gap-1">
			<button
				type="button"
				class={buttonVariants({
					variant: mode === 'write' ? 'secondary' : 'ghost',
					size: 'sm',
					class: 'shadow-none'
				})}
				onclick={() => (mode = 'write')}
			>
				Write
			</button>
			<button
				type="button"
				class={buttonVariants({
					variant: mode === 'preview' ? 'secondary' : 'ghost',
					size: 'sm',
					class: 'shadow-none'
				})}
				onclick={() => (mode = 'preview')}
			>
				Preview
			</button>
		</menu>

		<menu class="flex items-center gap-px">
			{#each shortcuts.slice(0, 5) as props}
				{@render shortcutButton(props)}
			{/each}
			{#if shortcuts.length > 5}
				<Popover.Root>
					<Popover.Trigger
						class={buttonVariants({
							variant: 'ghost',
							size: 'icon',
							class: 'shadow-none'
						})}
					>
						<Ellipsis size={16} class="m-auto" />
					</Popover.Trigger>
					<Popover.Content class="w-10 p-2 py-1">
						<menu class="flex flex-col items-center gap-1">
							{#each shortcuts.slice(5) as props}
								{@render shortcutButton(props)}
							{/each}
						</menu>
					</Popover.Content>
				</Popover.Root>
			{/if}
		</menu>
	</div>

	{#if mode === 'write'}
		<Textarea
			data-mode={mode}
			bind:ref={textarea}
			draggable="false"
			bind:value
			class="rounded-t-none border-0 shadow-none focus-visible:ring-0"
		/>
	{:else}
		<div class={cn('markdown prose prose-sm min-h-20 p-3')}>
			<Markdown md={value} />
		</div>
	{/if}
</div> -->
