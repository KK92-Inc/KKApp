<script lang="ts">
	import { Textarea } from '../textarea';
	import { cn } from 'tailwind-variants';
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
		Ellipsis,
		type Icon,

		ExternalLink

	} from '@lucide/svelte';
	import * as Tabs from '$lib/components/tabs';
	import Markdown from './markdown.svelte';
	import type { HTMLTextareaAttributes } from 'svelte/elements';

	interface Props extends HTMLTextareaAttributes {
		value?: string;
		class?: string;
		placeholder?: string;
	}

	interface Shortcut {
		icon: typeof Icon;
		label: string;
		action: () => void;
	}

	let textarea = $state<HTMLTextAreaElement>(null!);
	let { class: className, value = $bindable(''), placeholder = "# Hello World", ...rest }: Props = $props();

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

<Tabs.Root bind:value={mode} class={cn('gap-0', className)}>
	<Tabs.List class="h-fit w-full justify-between rounded-b-none border border-b-0">
		<div class="w-fit p-1">
			<Tabs.Trigger value="write">Write</Tabs.Trigger>
			<Tabs.Trigger value="preview">Preview</Tabs.Trigger>
		</div>
		{#if mode === 'write'}
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
		{/if}
	</Tabs.List>
	<Tabs.Content value="write">
		<Textarea
			data-mode={mode}
			bind:ref={textarea}
			draggable="false"
			{placeholder}
			bind:value
			class="field-sizing-content rounded-none shadow-none focus-visible:ring-0"
			{...rest}
		/>
		<div class="text-muted-foreground flex items-center gap-1.5 px-2 py-1.5 text-xs border border-t-0 border-input bg-muted rounded-b">
			<svg
				aria-hidden="true"
				xmlns="http://www.w3.org/2000/svg"
				width={18}
				height={18}
				viewBox="0 0 24 24"
				fill="currentColor"
			>
				<path
					d="M20.553 18.15H3.447a1.443 1.443 0 0 1-1.442-1.441V7.291c0-.795.647-1.441 1.442-1.441h17.105c.795 0 1.442.646 1.442 1.441v9.418a1.441 1.441 0 0 1-1.441 1.441zM6.811 15.268V11.52l1.922 2.402 1.922-2.402v3.748h1.922V8.732h-1.922l-1.922 2.403-1.922-2.403H4.889v6.535h1.922zM19.688 12h-1.922V8.732h-1.923V12h-1.922l2.884 3.364L19.688 12z"
				/>
			</svg>
			<a
				href="https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax"
				target="_blank"
				rel="noopener noreferrer"
				class="hover:text-foreground transition-colors hover:underline flex items-center gap-1"
			>
				Markdown is supported
				<ExternalLink size={12}/>
			</a>
		</div>
	</Tabs.Content>
	<Tabs.Content value="preview" class={cn('markdown rounded-t-none border-0 dark:bg-input/30 ')}>
		<Markdown {value} class="min-h-20 rounded-md rounded-t-none border p-3" />
	</Tabs.Content>
</Tabs.Root>
