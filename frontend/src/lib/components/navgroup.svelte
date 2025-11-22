<script lang="ts" generics="R extends readonly RouteId[]">
	import { FileQuestionMark, Icon } from '@lucide/svelte';
	import type { RouteId, RouteParams } from '$app/types';
	import { resolve } from '$app/paths';
	import { Separator } from './separator';
	import { MetaData } from '../../routes/index.svelte';
	import { page } from '$app/state';

	type MergeParams<T extends RouteId> = Partial<T extends any ? RouteParams<T> : never>;
	interface Props<R extends readonly RouteId[]> {
		title?: string;
		routes: R;
		args?: MergeParams<R[number]>;
	}

	interface NavItem {
		icon: typeof Icon;
		label: string;
		href: string;
	}

	const { title, routes, args, ...rest }: Props<R> = $props();
	const navigation = routes.reduce<NavItem[]>((acc, r) => {
		const data = MetaData.get(r);
		const item: NavItem = {
			label: (data?.label as string) ?? 'Unknown',
			icon: (data?.icon as typeof Icon) ?? FileQuestionMark,
			//@ts-expect-error id could be anything and so could be args
			href: resolve(r, {
				userId: page.data.session.userId, // Added by default
				...args
			})
		};

		const required: string[] | undefined = data?.scopes;
		const permissions: string[] = page.data?.session?.permissions ?? [];
		if (!required || required.length === 0) {
			acc.push(item);
		} else if (required.some((s: string) => permissions.includes(s))) {
			acc.push(item);
		}

		return acc;
	}, []);
</script>

<div>
	{#if title}
		<p class="mb-1 text-sm font-semibold">{title}</p>
		<Separator class="mb-1" />
	{/if}

	<menu>
		{#each navigation as nav, i (i)}
			{@const Icon = nav.icon}
			<li
				class="gap-2-y relative flex cursor-pointer list-none items-center justify-around rounded-md py-0.5 before:absolute before:left-[-2px] before:h-[20px] before:w-[6px] before:rounded-md before:bg-primary before:opacity-0 hover:bg-muted hover:before:opacity-100"
			>
				<a
					{...rest}
					href={nav.href}
					class="flex flex-1 items-center gap-2 rounded-md p-[0.35rem] focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
				>
					<Icon size={20} class="ml-1" />
					{nav.label}
				</a>
			</li>
		{/each}
	</menu>
</div>
