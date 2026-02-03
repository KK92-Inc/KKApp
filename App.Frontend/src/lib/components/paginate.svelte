<script lang="ts">
	import * as Pagination from '$lib/components/pagination';
	import { Pagination as PaginationPrimitive } from "bits-ui";

	const { ...rest }: PaginationPrimitive.RootProps = $props();
</script>

<Pagination.Root {...rest}>
	{#snippet children({ pages, currentPage })}
		<Pagination.Content>
			<Pagination.Item>
				<Pagination.Previous />
			</Pagination.Item>
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
			<Pagination.Item>
				<Pagination.Next />
			</Pagination.Item>
		</Pagination.Content>
	{/snippet}
</Pagination.Root>
