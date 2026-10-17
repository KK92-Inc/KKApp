<script lang="ts">
  import * as Item from '$lib/components/item';
  import { GitBranch, FolderGit2, SearchCheck, HeartHandshake } from '@lucide/svelte';
  import * as Page from './context.svelte';
  import * as Remote from './page.remote';
  import * as Card from '$lib/components/card';
  import * as InputGroup from '$lib/components/input-group';
  import { env } from '$env/dynamic/public';
	import Separator from '$lib/components/separator/separator.svelte';

  const context = Page.getContext();
  const { review } = $derived(await Remote.load(context.reviewId()));
  const repoUrl = $derived(`${env.PUBLIC_GIT_URL}/${review.userProject.projectId}/${review.userProject.id}`);
  const mirror = $derived(`git checkout ${review.sha}`);
</script>


<div class="space-y-4 p-4 max-w-7xl mx-auto">
  <h3 class="flex items-center gap-2 text-lg font-semibold tracking-tight">
    <HeartHandshake class="shrink-0" />
    Start Page & Overview
    <Separator class="flex-1"/>
  </h3>

  <!-- Added a clear helper message explaining these are informational cards -->
  <p class="text-xs text-muted-foreground">
    Reference Guide: Below are three overview guides to help you navigate this review page.
  </p>

  <Item.Group class="flex flex-row gap-3">
    <Item.Root class="flex-1" variant="muted" size="sm">
      <Item.Media variant="icon"><FolderGit2 class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">Step 1: Finding Files</Item.Title>
        <Item.Description class="line-clamp-none">
          To browse files, use the file explorer located on the left sidebar.
        </Item.Description>
      </Item.Content>
    </Item.Root>

    <Item.Root class="flex-1" variant="muted" size="sm">
      <Item.Media variant="icon"><SearchCheck class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">Step 2: Grading Rubric</Item.Title>
        <Item.Description class="line-clamp-none">
          Compare the submission against guidelines using the 'View Rubric' from the menu above.
        </Item.Description>
      </Item.Content>
    </Item.Root>

    <Item.Root class="flex-1" variant="muted" size="sm">
      <Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">Step 3: Testing Locally</Item.Title>
        <Item.Description class="line-clamp-none">
          For deeper review, follow the commands in the box below to clone and test on your computer.
        </Item.Description>
      </Item.Content>
    </Item.Root>
  </Item.Group>

  <Card.Root class="gap-0 py-0">
    <Card.Header class="gap-4 border-b bg-muted/40 p-4 pb-0">
      <Card.Title>
        Quick Setup & Local Cloning
        <span class="font-normal text-muted-foreground"> (Instructions for copying to your computer)</span>
      </Card.Title>
      <InputGroup.Root>
        <InputGroup.Input
          id="id"
          autocomplete="off"
          autocorrect="off"
          readonly
          value={`git clone ${repoUrl}`}
        />
        <InputGroup.Addon align="inline-end">
          <InputGroup.Copy value={`git clone ${repoUrl}`} />
        </InputGroup.Addon>
      </InputGroup.Root>
    </Card.Header>

    <Card.Content class="space-y-1 p-4">
      <h3 class="font-semibold">Switch to the reviewed version</h3>
      <p class="text-xs text-muted-foreground">
        To match the exact file state shown in this web viewer, open your terminal/command line, navigate into your cloned folder, and run this command:
      </p>
      <div class="group relative">
        <pre
          class="overflow-x-auto rounded-md border bg-muted/50 p-4 font-mono text-xs leading-relaxed">{mirror}</pre>
        <span class="absolute top-2 right-2 h-7 w-7 opacity-0 transition-opacity group-hover:opacity-100">
          <InputGroup.Copy value={mirror} />
        </span>
      </div>
    </Card.Content>
  </Card.Root>
</div>
