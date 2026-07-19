<script lang="ts">
  import { buttonVariants } from '$lib/components/button/index.js';
  import { CircleQuestionMark, Route, ShieldAlert, KeySquare } from '@lucide/svelte';
  import * as Dialog from '$lib/components/dialog/index.js';
  import Separator from '$lib/components/separator/separator.svelte';
  import * as Item from '$lib/components/item';

  const { title = 'How Custom Apps Work' }: { title?: string } = $props();
</script>

<Dialog.Root>
  <Dialog.Trigger class={buttonVariants({ variant: 'outline', size: 'sm' })}>
    {title}
    <CircleQuestionMark class="ml-2 h-4 w-4" />
  </Dialog.Trigger>

  <Dialog.Content>
    <Dialog.Header>
      <Dialog.Title>Third-party OAuth Apps</Dialog.Title>
      <Dialog.Description>
        Creating an app allows external services to securely authenticate users via your platform.
      </Dialog.Description>
    </Dialog.Header>

    <Item.Group class="grid grid-rows-3 gap-3">
      <Item.Root variant="muted" size="sm">
        <Item.Media variant="icon"><KeySquare class="size-4" /></Item.Media>
        <Item.Content>
          <Item.Title class="text-sm">Client ID & Secret</Item.Title>
          <Item.Description class="line-clamp-none">
            Your app gets a public Client ID and a hidden Client Secret. Use the ID to start the login process, and the Secret on your backend to securely exchange the login code for an access token.
          </Item.Description>
        </Item.Content>
      </Item.Root>

      <Item.Root variant="muted" size="sm">
        <Item.Media variant="icon"><Route class="size-4" /></Item.Media>
        <Item.Content>
          <Item.Title class="text-sm">Redirect URIs</Item.Title>
          <Item.Description class="line-clamp-none">
            For security, you must specify exactly where we are allowed to send the user back to after they log in. If the URL doesn't match exactly, the login will be blocked.
          </Item.Description>
        </Item.Content>
      </Item.Root>

      <Item.Root variant="muted" size="sm">
        <Item.Media variant="icon"><ShieldAlert class="size-4" /></Item.Media>
        <Item.Content>
          <Item.Title class="text-sm">Keep your secret safe</Item.Title>
          <Item.Description class="line-clamp-none">
            Never expose your Client Secret in frontend code (like React or SvelteKit client-side). If it gets compromised, you can rotate it from the app settings to generate a new one.
          </Item.Description>
        </Item.Content>
      </Item.Root>
    </Item.Group>

    <Separator />

    <p class="text-xs text-muted-foreground">
      Ready to integrate? Close this and use the <span class="font-medium">Create App</span> button.
    </p>
  </Dialog.Content>
</Dialog.Root>
