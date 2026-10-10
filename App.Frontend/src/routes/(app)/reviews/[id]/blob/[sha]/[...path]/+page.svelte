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
  <!-- <div class="border-b bg-muted/20 px-4 py-3">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div class="flex items-center gap-2">
        <FileText size={16} class="text-primary" />
        <span class="font-mono text-sm font-semibold">
          {params.path.split('/').pop() ?? ''}
        </span>
        <span class="text-xs text-muted-foreground">({params.path})</span>
      </div>

      <div class="flex items-center gap-3">
        <div class="hidden items-center gap-1.5 text-xs text-muted-foreground sm:flex">
          <CheckSquare size={14} class="text-emerald-500" />
          <span>Rubric: <strong>2/4 verified</strong></span>
        </div>

        <Tabs bind:value={activeTab} class="h-8">
          <TabsList class="h-8 bg-muted/60 p-0.5">
            <TabsTrigger value="file" class="h-7 px-3 text-xs">
              Code View
            </TabsTrigger>
            <TabsTrigger value="rubric" class="h-7 px-3 text-xs gap-1.5">
              <CheckSquare size={13} />
              Rubric
            </TabsTrigger>
            <TabsTrigger value="subject" class="h-7 px-3 text-xs gap-1.5">
              <BookOpen size={13} />
              Subject
            </TabsTrigger>
          </TabsList>
        </Tabs>
      </div>
    </div>

    {#if activeTab === 'rubric'}
      <div class="mt-3 rounded-md bg-card/60 p-3 text-xs border border-border/50">
        <div class="flex items-center justify-between font-medium mb-2 text-foreground">
          <span>Evaluation Rubric Criteria</span>
          <Badge variant="outline" class="text-[10px] uppercase">Strict Evaluation</Badge>
        </div>
        <ul class="space-y-1.5 text-muted-foreground">
          <li class="flex items-start gap-2">
            <CheckCircle2 size={14} class="mt-0.5 text-emerald-500 shrink-0" />
            <span><strong>Code Structure & Naming:</strong> Functions and variable names adhere to domain conventions.</span>
          </li>
          <li class="flex items-start gap-2">
            <CheckCircle2 size={14} class="mt-0.5 text-emerald-500 shrink-0" />
            <span><strong>Error Handling:</strong> Async operations include error boundaries or try-catch blocks.</span>
          </li>
          <li class="flex items-start gap-2">
            <Clock size={14} class="mt-0.5 text-amber-500 shrink-0" />
            <span><strong>Memory & Leakage:</strong> Subscriptions and timers are cleaned up on teardown.</span>
          </li>
        </ul>
      </div>
    {:else if activeTab === 'subject'}
      <div class="mt-3 rounded-md bg-card/60 p-3 text-xs text-muted-foreground border border-border/50 space-y-2">
        <div class="font-medium text-foreground">Project Subject Overview</div>
        <p class="leading-relaxed">
          The goal of this assignment is to construct a scalable code editor layout with asynchronous tree fetching, annotation capabilities, and interactive peer evaluations.
        </p>
      </div>
    {/if}
  </div> -->

  <!-- Main Code View + Annotations -->
  <div class="flex-1 overflow-hidden">
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
