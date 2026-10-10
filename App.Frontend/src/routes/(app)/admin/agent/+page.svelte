<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import * as Bubble from '$lib/components/bubble';
	import * as Message from '$lib/components/message';
	import * as Card from '$lib/components/card/index.js';
	import * as DropdownMenu from '$lib/components/dropdown-menu/index.js';
	import * as Empty from '$lib/components/empty/index.js';
	import * as InputGroup from '$lib/components/input-group/index.js';
	import * as Tooltip from '$lib/components/tooltip/index.js';

	import ArrowUpIcon from '@lucide/svelte/icons/arrow-up';
	import GlobeIcon from '@lucide/svelte/icons/globe';
	import ImageIcon from '@lucide/svelte/icons/image';
	import MessageCircleDashedIcon from '@lucide/svelte/icons/message-circle-dashed';
	import PaperclipIcon from '@lucide/svelte/icons/paperclip';
	import PlusIcon from '@lucide/svelte/icons/plus';
	import RotateCwIcon from '@lucide/svelte/icons/rotate-cw';
	import TelescopeIcon from '@lucide/svelte/icons/telescope';
</script>

<div class="flex w-full flex-col gap-6 py-12">
	<Message.Root>
		<Message.Avatar>
			<Avatar.Root>
				<Avatar.Image src="/avatars/03.png" alt="@avatar" />
				<Avatar.Fallback>R</Avatar.Fallback>
			</Avatar.Root>
		</Message.Avatar>
		<Message.Content>
			<Bubble.Root variant="muted">
				<Bubble.Content>The build failed during dependency installation.</Bubble.Content>
			</Bubble.Root>
		</Message.Content>
	</Message.Root>

	<Message.Root align="end">
		<Message.Avatar>
			<Avatar.Root>
				<Avatar.Image src="/avatars/10.png" alt="@avatar" />
				<Avatar.Fallback>R</Avatar.Fallback>
			</Avatar.Root>
		</Message.Avatar>
		<Message.Content>
			<Bubble.Root>
				<Bubble.Content>Can you share the exact error?</Bubble.Content>
			</Bubble.Root>
		</Message.Content>
	</Message.Root>

	<Message.Root>
		<Message.Avatar>
			<Avatar.Root>
				<Avatar.Image src="/avatars/03.png" alt="@avatar" />
				<Avatar.Fallback>R</Avatar.Fallback>
			</Avatar.Root>
		</Message.Avatar>
		<Message.Content>
			<Bubble.Group>
				<Bubble.Root variant="muted">
					<Bubble.Content>Here's the error from the logs</Bubble.Content>
				</Bubble.Root>
				<Bubble.Root variant="muted">
					<Bubble.Content>
						Something went wrong with the build. The libraries are not installed correctly. Try running the
						build again.
					</Bubble.Content>
				</Bubble.Root>
			</Bubble.Group>
		</Message.Content>
	</Message.Root>

	<form class="w-full">
		<InputGroup.Root>
			<div class="h-14 w-full py-2.5">
				<InputGroup.Input placeholder="Search..." />
				<span class="line-clamp-2 opacity-60 data-[status=ready]:opacity-100">
					<!-- <span class="text-muted-foreground"> No messages queued. Reset the conversation. </span> -->
				</span>
			</div>
			<InputGroup.Addon align="block-end" class="pt-1">
				<!-- <DropdownMenu.Root>
					<DropdownMenu.Trigger>
						{#snippet child({ props })}
							<InputGroup.Button
								{...props}
								aria-label="Add files"
								type="button"
								size="icon-sm"
								variant="outline"
							>
								<PlusIcon />
							</InputGroup.Button>
						{/snippet}
					</DropdownMenu.Trigger>
					<DropdownMenu.Content align="start" side="top" class="w-44">
						<DropdownMenu.Item>
							<PaperclipIcon />
							Add Photos & Files
						</DropdownMenu.Item>
						<DropdownMenu.Separator />
						<DropdownMenu.Item>
							<ImageIcon />
							Create Image
						</DropdownMenu.Item>
						<DropdownMenu.Item>
							<TelescopeIcon />
							Deep Research
						</DropdownMenu.Item>
						<DropdownMenu.Item>
							<GlobeIcon />
							Web Search
						</DropdownMenu.Item>
					</DropdownMenu.Content>
				</DropdownMenu.Root> -->
				<InputGroup.Button
					type="submit"
					variant="default"
					size="icon-sm"
					class="ml-auto"
				>
					<ArrowUpIcon />
					<span class="sr-only">Send</span>
				</InputGroup.Button>
			</InputGroup.Addon>
		</InputGroup.Root>
	</form>
</div>
