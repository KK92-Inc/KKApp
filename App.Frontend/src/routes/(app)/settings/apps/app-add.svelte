<script lang="ts">
  import { Button, buttonVariants } from '$lib/components/button';
  import { Plus, Save, Pencil } from '@lucide/svelte';
  import * as Workspace from '$lib/remotes/workspace.remote';
  import * as Dialog from '$lib/components/dialog';
  import { Input } from '$lib/components/input';
  import * as Field from '$lib/components/field';
  import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
  import { toast } from 'svelte-sonner';
  import { Problem, type ValidationErrors } from '$lib/api';

  let {
    workspaceId,
    app = null,
  }: {
    workspaceId: string;
    app?: any;
  } = $props();

  let open = $state(false);
  let errors = $state<ValidationErrors>({});
  let isSubmitting = $state(false);

  // Initialize fields (handle both edit and create modes)
  let fields = $state({
    name: app?.name ?? '',
    description: app?.description ?? '',
    redirectUris: app?.redirectUris ? app.redirectUris.join(', ') : ''
  });

  async function submit() {
    isSubmitting = true;
    errors = {};

    try {
      // Parse the comma-separated URIs into an array
      const uris = fields.redirectUris
        .split(',')
        .map(uri => uri.trim())
        .filter(Boolean);

      if (app) {
        await Workspace.updateApplication({
          id: workspaceId,
          appId: app.id, // Assuming your app object has an id
          name: fields.name,
          description: fields.description,
          redirectUris: uris.length > 0 ? uris : null
        });
        toast.success("Application updated successfully!");
      } else {
        await Workspace.createApplication({
          id: workspaceId,
          name: fields.name,
          description: fields.description,
          enabled: true,
          redirectUris: uris
        });
        toast.success("Application created successfully!");
      }

      open = false;
    } catch (e) {
      const resolved = Problem.resolve(e);
      if (resolved.kind === 'validation') {
        errors = resolved.fields;
      } else {
        toast.error(resolved.message);
      }
    } finally {
      isSubmitting = false;
    }
  }
</script>

<Dialog.Root bind:open onOpenChange={(v) => { if (!v) errors = {}}}>
  <Dialog.Trigger type="button" class={buttonVariants({ variant: app ? 'ghost' : 'default', size: app ? 'icon' : 'sm' })}>
    {#if app}
      <Pencil class="size-4" />
    {:else}
      Create App
      <Plus class="ml-2 h-4 w-4" />
    {/if}
  </Dialog.Trigger>

  <Dialog.Content class="sm:max-w-[500px]">
    <Dialog.Header>
      <Dialog.Title>{app ? 'Edit Application' : 'Create Application'}</Dialog.Title>
      <Dialog.Description>
        {app ? 'Update your OAuth application settings.' : 'Register a new third-party OAuth application.'}
      </Dialog.Description>
    </Dialog.Header>

    <Field.Set class="mt-4">
      <Field.Group>
        <Field.Field>
          <Field.Label for="app-name">App Name</Field.Label>
          <Input
            id="app-name"
            autocomplete="off"
            placeholder="My Cool Integration"
            bind:value={fields.name}
          />
          <Field.Description>A public-facing name for your application.</Field.Description>
          <Field.Error errors={errors.name} />
        </Field.Field>

        <Field.Field>
          <Field.Label for="app-uris">Redirect URIs</Field.Label>
          <Input
            id="app-uris"
            autocomplete="off"
            placeholder="https://yourapp.com/callback, http://localhost:3000/callback"
            bind:value={fields.redirectUris}
          />
          <Field.Description>Comma-separated list of allowed callback URLs.</Field.Description>
          <Field.Error errors={errors.redirectUris} />
        </Field.Field>

        <Field.Field>
          <Field.Label for="app-desc">Description</Field.Label>
          <MarkdownTextarea bind:value={fields.description} maxlength={500} />
          <Field.Description>Optional details about what this app does.</Field.Description>
          <Field.Error errors={errors.description} />
        </Field.Field>
      </Field.Group>
    </Field.Set>

    <Dialog.Footer>
      <Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>Cancel</Dialog.Close>
      <Button type="button" disabled={isSubmitting} onclick={submit}>
        {app ? 'Save Changes' : 'Create'} <Save class="ml-2 h-4 w-4" />
      </Button>
    </Dialog.Footer>
  </Dialog.Content>
</Dialog.Root>
