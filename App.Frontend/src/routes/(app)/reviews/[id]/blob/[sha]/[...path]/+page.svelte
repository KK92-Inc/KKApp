<script lang="ts">
  import * as Page from '../../../context.svelte';
  import * as Remote from '../../../page.remote';
  import * as Git from '$lib/remotes/git.remote';
  import type { PageProps } from './$types';
  import { ExplorerFile } from '$lib/components/explorer';
  import { Button } from '$lib/components/button';
  import { Badge } from '$lib/components/badge';
  import { Tabs, TabsList, TabsTrigger, TabsContent } from '$lib/components/tabs';
  import {
    FileText,
    CheckSquare,
    BookOpen,
    Trash2,
    MessageSquarePlus,
    CornerDownLeft,
    CheckCircle2,
    Clock
  } from '@lucide/svelte';

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
  let activeTab = $state<'file' | 'rubric' | 'subject'>('file');

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

    annotations.push(newComment);
    draftCommentText = '';
    close();
  }

  async function removeComment(id: string) {
    annotations = annotations.filter((a) => a.id !== id);
  }
</script>

<div class="flex h-full flex-col text-foreground">

  <!-- Main Code View + Annotations -->
  <div class="flex-1">
    <svelte:boundary>
      {@const content = await Git.getBlob({
        id: data.review.userProject.gitInfoId,
        ref: data.review.ref,
        path: params.path
      })}

      <ExplorerFile
			backHref="/reviews/{params.id}"
        name={params.path.split('/').pop() ?? ''}
        {content}
        annotations={fileAnnotations}
        onRemoveAnnotation={removeComment}
      >
        <!-- Rendered Comment Callout -->
        {#snippet annotation({ annotation, remove })}
          <div class="group my-1.5 rounded-lg border bg-card/90 p-2.5 text-card-foreground shadow-sm transition-all hover:bg-card max-w-md">
            <div class="flex items-center justify-between gap-2 border-b border-border/40 pb-1.5">
              <div class="flex items-center gap-1.5">
                <div class="flex h-5 w-5 items-center justify-center rounded-full bg-primary/10 text-[10px] font-bold text-primary">
                  {annotation.author.charAt(0)}
                </div>
                <span class="text-xs font-medium text-foreground">{annotation.author}</span>
              </div>
              <Button
                variant="ghost"
                size="sm"
                class="h-6 w-6 p-0 text-muted-foreground opacity-0 group-hover:opacity-100 hover:text-destructive"
                onclick={remove}
                title="Delete comment"
              >
                <Trash2 size={13} />
              </Button>
            </div>
            <p class="mt-2 text-xs leading-relaxed text-muted-foreground">{annotation.text}</p>
          </div>
        {/snippet}

        <!-- Inline Comment Input Overlay -->
        {#snippet annotationInput({ line, close })}
          <div class="my-2 rounded-lg border bg-card p-3 shadow-md max-w-md">
            <div class="mb-2 flex items-center justify-between text-xs text-muted-foreground">
              <span class="flex items-center gap-1">
                <MessageSquarePlus size={13} class="text-primary" />
                Comment on line <strong class="text-foreground">#{line}</strong>
              </span>
            </div>

            <textarea
              bind:value={draftCommentText}
              placeholder="Add review feedback or suggest a change..."
              rows={2}
              class="w-full resize-none rounded-md border bg-background p-2 text-xs outline-none focus:border-primary/50 focus:ring-1 focus:ring-primary/20"
            ></textarea>

            <div class="mt-2 flex justify-end gap-2">
              <Button variant="ghost" size="sm" class="h-7 px-2.5 text-xs" onclick={close}>
                Cancel
              </Button>
              <Button size="sm" class="h-7 gap-1 px-3 text-xs" onclick={() => addComment(line, close)}>
                Comment
                <CornerDownLeft size={12} />
              </Button>
            </div>
          </div>
        {/snippet}
      </ExplorerFile>
    </svelte:boundary>
  </div>
</div>
