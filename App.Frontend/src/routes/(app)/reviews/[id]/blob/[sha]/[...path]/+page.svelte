<script lang="ts">
  import * as Page from '../../../context.svelte';
  import * as Remote from '../../../page.remote';
  import * as Git from '$lib/remotes/git.remote';
  import type { PageProps } from './$types';
  import { ExplorerFile } from '$lib/components/explorer';
  import { Button } from '$lib/components/button';

  const context = Page.getContext();
  const data = $derived(await Remote.getData(context.reviewId()));
  const { params }: PageProps = $props();

  interface CommentAnnotation {
    id: string;
    line: number;
    author: string;
    text: string;
    path: string;
  }

  let annotations = $state<CommentAnnotation[]>([]);
  let draftCommentText = $state('');

  // Filter annotations for the current file
  const fileAnnotations = $derived(
    annotations.filter((a) => a.path === params.path)
  );

  async function addComment(line: number, close: () => void) {
    if (!draftCommentText.trim()) return;

    const newComment: CommentAnnotation = {
      id: crypto.randomUUID(),
      line,
      author: 'You',
      text: draftCommentText.trim(),
      path: params.path
    };

    // TODO: Trigger remote endpoint mutation here if needed
    annotations.push(newComment);
    draftCommentText = '';
    close();
  }

  async function removeComment(id: string) {
    // TODO: Trigger remote endpoint mutation here if needed
    annotations = annotations.filter((a) => a.id !== id);
  }
</script>

<svelte:boundary>
  {@const content = await Git.getBlob({
    id: data.review.userProject.gitInfoId,
    ref: data.review.ref,
    path: params.path
  })}

  <ExplorerFile
    name={params.path.split('/').pop() ?? ''}
    {content}
    annotations={fileAnnotations}
    onRemoveAnnotation={removeComment}
  >
    {#snippet annotation({ annotation, remove })}
      <div class="rounded-md border bg-card p-3 text-card-foreground shadow-sm">
        <div class="flex items-center justify-between">
          <span class="text-xs font-semibold text-muted-foreground">{annotation.author}</span>
          <Button
            variant="ghost"
            size="sm"
            class="h-auto p-0 text-xs text-destructive hover:bg-transparent"
            onclick={remove}
          >
            Delete
          </Button>
        </div>
        <p class="mt-1 text-sm">{annotation.text}</p>
      </div>
    {/snippet}

    {#snippet annotationInput({ line, close })}
      <div class="flex flex-col gap-2 rounded-md border bg-card p-3 shadow-sm">
        <textarea
          bind:value={draftCommentText}
          placeholder={`Add a comment on line ${line}...`}
          rows={2}
          class="w-full resize-none rounded-md border bg-background p-2 text-sm outline-none focus:ring-1 focus:ring-ring"
        ></textarea>

        <div class="flex justify-end gap-2">
          <Button variant="ghost" size="sm" onclick={close}>Cancel</Button>
          <Button size="sm" onclick={() => addComment(line, close)}>Comment</Button>
        </div>
      </div>
    {/snippet}
  </ExplorerFile>
</svelte:boundary>
