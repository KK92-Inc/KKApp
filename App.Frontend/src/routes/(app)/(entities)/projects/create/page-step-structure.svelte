<script lang="ts">
  import * as Page from './context.svelte';
  import * as Field from '$lib/components/field';
  import * as Alert from '$lib/components/alert';
  import * as Card from '$lib/components/card';
  import * as Item from '$lib/components/item';
  import * as Empty from '$lib/components/empty';
  import * as InputGroup from '$lib/components/input-group';
  import * as Resizable from '$lib/components/resizable';
  import * as ButtonGroup from '$lib/components/button-group';
  import { Button } from '$lib/components/button';
  import { Badge } from '$lib/components/badge';
  import Separator from '$lib/components/separator/separator.svelte';

  import {
    FileTerminal,
    FileText,
    Info,
    GitBranch,
    Database,
    MousePointerClick,
    Copy,
    PencilLine,
    Trash2,
    FolderUp,
    FilePlus2,
    FileQuestion,
    ExternalLink,
    TriangleAlert
  } from '@lucide/svelte';

  import { FileTree } from '@pierre/trees';
  import type { Attachment } from 'svelte/attachments';

  // Import our refactored tree utilities
  import {
    MAX_FILES,
    README_PATH,
    DEFAULT_README,
    type FileEntry,
    isTextBased,
    readAsText,
    formatBytes,
    uniquePath,
    movePrefix,
    addedStatus
  } from '$lib/components/filetree';

  // Kept from the original file — other steps in this wizard read/write through
  // this context, even though this step doesn't need it directly.
  const context = Page.getContext();

  // ---------------------------------------------------------------------------
  // State
  // ---------------------------------------------------------------------------

  let files = $state<Record<string, FileEntry>>({
    [README_PATH]: {
      path: README_PATH,
      kind: 'text',
      content: DEFAULT_README,
      size: new Blob([DEFAULT_README]).size,
      mimeType: 'text/markdown'
    }
  });

  let selectedPath = $state(README_PATH);
  let notice = $state<{ tone: 'error' | 'info'; text: string } | null>(null);

  let filePickerEl: HTMLInputElement | undefined;
  let folderPickerEl: HTMLInputElement | undefined;

  // The FileTree instance is imperative state, not UI state
  let fileTree: FileTree | undefined;

  const fileCount = $derived(Object.keys(files).length);
  const canAddMore = $derived(fileCount < MAX_FILES);
  const selectedEntry = $derived(files[selectedPath]);

  // Keep the mounted tree's paths, and its "these are new" git-status badges,
  // in sync with our data whenever a file is added, removed, or moved.
  $effect(() => {
    const currentPaths = Object.keys(files);
    fileTree?.resetPaths(currentPaths);
    fileTree?.setGitStatus(addedStatus(currentPaths));
  });

  // ---------------------------------------------------------------------------
  // Add / remove / move files
  // ---------------------------------------------------------------------------

  async function addFilesAtPaths(items: { file: File; path: string }[]): Promise<string[]> {
    const available = Math.max(MAX_FILES - fileCount, 0);
    const accepted = items.slice(0, available);
    const overflow = items.length - accepted.length;

    const next = { ...files };
    const addedPaths: string[] = [];

    for (const { file, path: rawPath } of accepted) {
      const path = uniquePath(rawPath, next);
      const text = isTextBased(file) ? await readAsText(file) : undefined;
      next[path] = {
        path,
        kind: text !== undefined ? 'text' : 'binary',
        content: text,
        size: file.size,
        mimeType: file.type || 'application/octet-stream'
      };
      addedPaths.push(path);
    }

    files = next;
    notice = overflow > 0
        ? {
            tone: 'info',
            text: `Added ${accepted.length} file(s). ${overflow} skipped — the ${MAX_FILES}-file limit was reached.`
          }
        : null;
    return addedPaths;
  }

  async function onFilePicked(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    const [path] = await addFilesAtPaths([{ file, path: file.name }]);
    if (path) selectedPath = path;
  }

  async function onFolderPicked(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const picked = Array.from(input.files ?? []);
    input.value = '';
    if (picked.length === 0) return;
    await addFilesAtPaths(
      picked.map((file) => ({ file, path: (file.webkitRelativePath || file.name).replace(/^\/+/, '') }))
    );
  }

  function removeFile(path: string) {
    if (fileCount <= 1) return;
    const { [path]: _removed, ...rest } = files;
    files = rest;
    if (selectedPath === path) {
      selectedPath = Object.keys(rest)[0];
    }
  }

  function updateContent(path: string, value: string) {
    const entry = files[path];
    if (!entry) return;
    files = { ...files, [path]: { ...entry, content: value } };
  }

  // ---------------------------------------------------------------------------
  // Trees callbacks
  // ---------------------------------------------------------------------------

  function handleRename({
    sourcePath,
    destinationPath,
    isFolder
  }: {
    sourcePath: string;
    destinationPath: string;
    isFolder: boolean;
  }) {
    files = movePrefix(files, sourcePath, destinationPath);
    if (selectedPath === sourcePath || (isFolder && selectedPath.startsWith(`${sourcePath}/`))) {
      selectedPath = destinationPath + selectedPath.slice(sourcePath.length);
    }
    notice = null;
  }

  function handleTreeError(message: string) {
    notice = { tone: 'error', text: message };
  }

  function tree(): Attachment<HTMLDivElement> {
    return (container) => {
      container.style.setProperty('--trees-theme-list-hover-bg', 'var(--accent)');
      container.style.setProperty('--trees-theme-focus-ring', 'var(--ring)');
      container.style.setProperty('--trees-search-bg', 'var(--muted)');
      container.style.setProperty('--trees-border-color', 'var(--border)');
      container.style.setProperty('--trees-padding-inline', '0.75rem');
      container.style.setProperty('--trees-bg', 'transparent');

      const instance = new FileTree({
        paths: Object.keys(files),
        search: true,
        icons: 'standard',
        flattenEmptyDirectories: true,
        gitStatus: addedStatus(Object.keys(files)),
        renderRowDecoration: ({ item }: { item: { path: string } }) => {
          const entry = files[item.path];
          if (!entry) return null;
          return { text: formatBytes(entry.size), title: `${entry.size.toLocaleString()} bytes` };
        },
        renaming: {
          canRename: () => true,
          onRename: handleRename,
          onError: handleTreeError
        },
        onSelectionChange: (paths) => {
          if (paths[0]) selectedPath = paths[0];
        }
      });

      instance.render({ fileTreeContainer: container });
      fileTree = instance;

      return () => {
        instance.cleanUp();
        fileTree = undefined;
      };
    };
  }
</script>

<Field.Set>
  <Field.Legend class="text-xl font-bold tracking-tight">Project Workspace</Field.Legend>
  <Field.Description class="mt-1 text-sm text-muted-foreground">
    This step creates a real Git repository. Add whatever files your project needs — documentation, source
    code, or data — and they'll become the first commit.
  </Field.Description>

  <Item.Group class="grid gap-3 sm:grid-cols-3">
    <Item.Root variant="muted" size="sm">
      <Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">A Git repo is created</Item.Title>
        <Item.Description class="text-xs">Everything below becomes the initial commit.</Item.Description>
      </Item.Content>
      <Item.Footer>
        <a
          href="https://docs.github.com/en/get-started/using-git/about-git"
          target="_blank"
          rel="noopener noreferrer"
          class="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground hover:underline"
        >
          What is a Git repository? <ExternalLink class="size-3" />
        </a>
      </Item.Footer>
    </Item.Root>
    <Item.Root variant="muted" size="sm">
      <Item.Media variant="icon"><FileText class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">README.md explains it</Item.Title>
        <Item.Description class="text-xs">The first thing collaborators will read.</Item.Description>
      </Item.Content>
      <Item.Footer>
        <a
          href="https://www.makeareadme.com/"
          target="_blank"
          rel="noopener noreferrer"
          class="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground hover:underline"
        >
          Writing a good README <ExternalLink class="size-3" />
        </a>
      </Item.Footer>
    </Item.Root>
    <Item.Root variant="muted" size="sm">
      <Item.Media variant="icon"><Database class="size-4" /></Item.Media>
      <Item.Content>
        <Item.Title class="text-sm">Any file type welcome</Item.Title>
        <Item.Description class="text-xs">data.csv, stuff.xlsx, configs — whatever it needs.</Item.Description>
      </Item.Content>
    </Item.Root>
  </Item.Group>

  <Separator />

  <Alert.Root>
    <FileTerminal class="h-4 w-4" />
    <Alert.Title>Repository Target</Alert.Title>
    <Alert.Description class="mt-1 block text-xs text-muted-foreground">
      These files are committed directly to your new repository's root directory on creation. This panel is a
      live preview of that first commit — every file shown is marked "added," the same badge Git itself would
      show you.
    </Alert.Description>
  </Alert.Root>

  <div class="flex flex-wrap items-center gap-2">
    <ButtonGroup.Root>
      <input bind:this={filePickerEl} type="file" class="hidden" onchange={onFilePicked} />
      <Button variant="outline" size="sm" disabled={!canAddMore} onclick={() => filePickerEl?.click()}>
        <FilePlus2 />
        Add file
      </Button>

      <input
        bind:this={folderPickerEl}
        type="file"
        class="hidden"
        multiple
        webkitdirectory
        onchange={onFolderPicked}
      />
      <Button variant="outline" size="sm" disabled={!canAddMore} onclick={() => folderPickerEl?.click()}>
        <FolderUp />
        Add folder
      </Button>
    </ButtonGroup.Root>

    <Badge variant="secondary" class="ml-auto font-mono">{fileCount}/{MAX_FILES} files</Badge>
  </div>

  {#if notice}
    <Alert.Root variant={notice.tone === 'error' ? 'destructive' : 'default'}>
      <TriangleAlert class="h-4 w-4" />
      <Alert.Description class="text-xs">{notice.text}</Alert.Description>
    </Alert.Root>
  {/if}

  <div class="relative">
    <Card.Root class="p-0 shadow-none">
      <Card.Content class="p-0">
        <Resizable.PaneGroup direction="horizontal" class="flex-1">
          <Resizable.Pane defaultSize={30} minSize={25} class="flex flex-col">
            <div {@attach tree()} class="pierre-theme-wrapper min-h-72 flex-1 pt-4"></div>
          </Resizable.Pane>
          <Resizable.Handle withHandle />
          <Resizable.Pane defaultSize={70}>
            {#if selectedEntry}
              <div class="flex h-full flex-col">
                <div class="flex items-center gap-1 border-b px-3 py-2">
                  <span class="truncate font-mono text-sm font-medium">{selectedEntry.path}</span>
                  <span class="text-xs text-muted-foreground">· {formatBytes(selectedEntry.size)}</span>
                  <div class="ms-auto flex items-center gap-1">
                    <InputGroup.Button
                      aria-label="Rename or move"
                      title="Rename or move"
                      size="icon-xs"
                      onclick={() => fileTree?.startRenaming(selectedEntry.path)}
                    >
                      <PencilLine />
                    </InputGroup.Button>
                    <InputGroup.Button
                      aria-label="Copy path"
                      title="Copy path"
                      size="icon-xs"
                      onclick={() => navigator.clipboard?.writeText(selectedEntry.path)}
                    >
                      <Copy />
                    </InputGroup.Button>
                    <InputGroup.Button
                      aria-label="Remove file"
                      title="Remove file"
                      size="icon-xs"
                      disabled={fileCount <= 1}
                      onclick={() => removeFile(selectedEntry.path)}
                    >
                      <Trash2 />
                    </InputGroup.Button>
                  </div>
                </div>

                {#if selectedEntry.kind === 'text'}
                  <InputGroup.Root class="flex-1 rounded-none border-0">
                    <InputGroup.Textarea
                      value={selectedEntry.content ?? ''}
                      oninput={(e) =>
                        updateContent(selectedEntry.path, (e.currentTarget as HTMLTextAreaElement).value)}
                      placeholder="console.log('Hello, world!');"
                      class="min-h-72 font-mono text-sm"
                    />
                  </InputGroup.Root>
                {:else}
                  <div class="flex flex-1 items-center justify-center p-6">
                    <Empty.Root class="border border-dashed">
                      <Empty.Header>
                        <Empty.Media variant="icon"><FileQuestion /></Empty.Media>
                        <Empty.Title>Binary file</Empty.Title>
                        <Empty.Description>
                          {selectedEntry.mimeType || 'Unknown type'} · {formatBytes(selectedEntry.size)}
                          — this file type can't be previewed here, but it will be uploaded as-is.
                        </Empty.Description>
                      </Empty.Header>
                    </Empty.Root>
                  </div>
                {/if}
              </div>
            {:else}
              <Empty.Root class="m-6 flex-1 rounded-lg border border-dashed bg-background/50">
                <Empty.Header>
                  <Empty.Media variant="icon">
                    <MousePointerClick class="size-6 text-muted-foreground/60" />
                  </Empty.Media>
                  <Empty.Title class="text-sm">No file selected</Empty.Title>
                  <Empty.Description class="text-xs">
                    Select a node from the directory tree to view or edit its contents.
                  </Empty.Description>
                </Empty.Header>
              </Empty.Root>
            {/if}
          </Resizable.Pane>
        </Resizable.PaneGroup>
      </Card.Content>
    </Card.Root>
  </div>

  <Field.Description class="text-xs text-muted-foreground">
    New files land at the repository root — rename one (pencil icon) to a path like
    <code class="font-mono">data/input.csv</code> to nest it in a folder, or drag it onto another folder in the
    tree. Note: an empty folder can't be added on its own, since Git only tracks files, never empty
    directories.
  </Field.Description>

  <Alert.Root variant="warning">
    <Info size={16} />
    <Alert.Title>File Upload Limit</Alert.Title>
    <Alert.Description class="text-xs leading-tight">
      Limited to {MAX_FILES} initial files online ({fileCount} used). Run git clone locally afterward to add more.
    </Alert.Description>
  </Alert.Root>
</Field.Set>
