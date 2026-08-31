<script lang="ts">
  import Layout from '$lib/components/layout.svelte';
  import * as v from 'valibot';
  import * as InputGroup from '$lib/components/input-group';
  import * as Field from '$lib/components/field';
  import * as Tabs from '$lib/components/tabs';
  import * as Select from '$lib/components/select';
  import * as Empty from '$lib/components/empty';
  import * as Item from '$lib/components/item';
  import * as Projects from '$lib/remotes/projects.remote';
  import { Archive, FolderCode, Search } from '@lucide/svelte';
  import useDebounce from '$lib/hooks/debounce.svelte';
  import useSearchParams from '$lib/hooks/url.svelte';
  import { page } from '$app/state';
  import type { PageProps } from './$types';
  import { Separator } from '$lib/components/separator';
  import Paginate from '$lib/components/paginate.svelte';
  import teleport from '$lib/hooks/teleport.svelte';
  import Skeleton from '$lib/components/skeleton/skeleton.svelte';
  import { Button } from '$lib/components/button';

  const { params }: PageProps = $props();

  const url = useSearchParams({
    index: v.fallback(
      v.pipe(
        v.string(),
        v.transform(Number),
        v.check((n) => !isNaN(n) && n > 0)
      ),
      1
    ),
    search: v.fallback(v.string(), '')
  });

  const search = url.query('search');
  const index = url.query('index');
  const debounced = useDebounce((query: string) => {
    if (query.length <= 0) search.clear();
    else search.value = query;
  });
</script>

{#snippet loader()}
  <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
    <Skeleton class="h-36 rounded-xl" />
    <Skeleton class="h-36 rounded-xl" />
    <Skeleton class="h-36 rounded-xl" />
    <Skeleton class="h-36 rounded-xl" />
  </div>
{/snippet}

{#snippet empty()}
  <Empty.Root class="col-span-full py-12">
    <Empty.Header>
      <Empty.Media variant="icon">
        <FolderCode class="size-6" />
      </Empty.Media>
      <Empty.Title>Nothing here</Empty.Title>
      <Empty.Description>
        Nothing matched your criteria, thus we have nothing to show for you.
      </Empty.Description>
    </Empty.Header>
  </Empty.Root>
{/snippet}

<div class="w-full space-y-6 px-4 py-4 lg:px-6">
  <!-- Toolbar Header -->
  <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
    <InputGroup.Root class="w-full sm:w-80">
      <InputGroup.Addon>
        <Search class="size-4 text-muted-foreground" />
      </InputGroup.Addon>
      <InputGroup.Input
        placeholder="Search projects..."
        value={search.value}
        oninput={(e) => debounced.fn(e.currentTarget.value)}
      />
    </InputGroup.Root>
		<Separator class="flex-1"/>
    <!-- Teleport container for pagination -->
    <div id="pagination" class="flex shrink-0 items-center justify-end"></div>
  </div>

  <svelte:boundary>
    {@const result = await Projects.getPage({
      page: index.value,
      workspaceId: params.id,
      name: search.value
    })}

    {#snippet pending()}
      {@render loader()}
    {/snippet}

    <span {@attach teleport('pagination')}>
      <Paginate
        page={index.value}
        onPageChange={(p) => (index.value = p)}
        perPage={result.perPage}
        count={result.count}
      />
    </span>

    <Item.Group class="grid grid-cols-1 gap-4 lg:grid-cols-2 xl:grid-cols-3">
      {#each result.data as project (project.id)}
        <Item.Project {project}>
          {#snippet actions()}
            <Button
              variant="outline"
              size="sm"
              href="/users/{page.data.session.userId}/projects/{project.id}"
            >
              View
            </Button>
          {/snippet}
        </Item.Project>
				        <Item.Project {project}>
          {#snippet actions()}
            <Button
              variant="outline"
              size="sm"
              href="/users/{page.data.session.userId}/projects/{project.id}"
            >
              View
            </Button>
          {/snippet}
        </Item.Project>
      {:else}
        {@render empty()}
      {/each}
    </Item.Group>
  </svelte:boundary>
</div>
