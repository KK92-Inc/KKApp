<script lang="ts">
	import * as Pagination from '$lib/components/pagination';
	import { Pagination as PaginationPrimitive } from 'bits-ui';

	type Props = PaginationPrimitive.RootProps & {
		variant?: 'default' | 'short';
	};

	let { page = $bindable(0), onPageChange, variant = 'default', ...rest }: Props = $props();
</script>

<Pagination.Root
	page={page + 1}
	onPageChange={(newPage) => {
		page = newPage - 1;
		onPageChange?.(newPage - 1);
	}}
	{...rest}
>
	{#snippet children({ pages, currentPage })}
		<Pagination.Content>
			<Pagination.Item>
				<Pagination.Previous />
			</Pagination.Item>

			{#if variant === 'default'}
				{#each pages as page (page.key)}
					{#if page.type === 'page'}
						<Pagination.Item>
							<Pagination.Link {page} isActive={currentPage === page.value}>
								{page.value}
							</Pagination.Link>
						</Pagination.Item>
					{:else}
						<Pagination.Item>
							<Pagination.Ellipsis />
						</Pagination.Item>
					{/if}
				{/each}
			{:else}
				<Pagination.Item>
					{page}
				</Pagination.Item>
			{/if}

			<Pagination.Item>
				<Pagination.Next />
			</Pagination.Item>
		</Pagination.Content>
	{/snippet}
</Pagination.Root>
