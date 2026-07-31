<script lang="ts">
  import * as UserProjects from '$lib/remotes/user-project.remote';
  import { ClockFading, UserCheck, Sparkles, Hourglass, Ban, TriangleAlert } from '@lucide/svelte';
  import { Button } from '$lib/components/button';
  import * as Page from './context.svelte';
  import * as User from '$lib/remotes/user.remote';
  import { page } from '$app/state';
  import { useDialog } from '$lib/components/dialog';
  import * as Subscription from '$lib/remotes/subscription.remote';
  import { Problem } from '$lib/api';
  import * as Alert from '$lib/components/alert';
  import Failed from '$lib/components/empty/failed.svelte';
	import { DateFormatter } from '@internationalized/date';

  const dialog = useDialog();
  const context = Page.getContext();
	const formatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric',
		hour: '2-digit',
		minute: '2-digit'
	});


  const userProject = $derived(context.userProject);
  const unlocksAt = $derived(
    userProject?.entity.unlocksAt ? new Date(userProject.entity.unlocksAt) : null
  );
  const isCoolingDown = $derived(unlocksAt !== null && unlocksAt > new Date());
  const isReactivation = $derived(userProject !== undefined && userProject.entity.state === 'Inactive');

  const subscribeConfirmTitle = $derived(
    isReactivation
      ? `Reactivate ${context.project.entity.name}?`
      : `Subscribe to ${context.project.entity.name}?`
  );

  const subscribeConfirmDescription = $derived(
    isReactivation
      ? 'Are you sure you want to reactivate your project session? This will restore your repository access.'
      : 'Are you sure you want to subscribe to this project? Doing so will start a new project session.'
  );

  const subscribe = $derived(
    dialog.confirm(subscribeConfirmTitle, subscribeConfirmDescription)
  );

  const unsubscribe = $derived(
    dialog.confirm(
      `Unsubscribe from ${context.project.entity.name}?`,
      'Are you sure you want to unsubscribe? Doing so will deactivate your session and place resubscription on temporary cooldown.'
    )
  );
</script>

{#snippet cooldownAlert(unlockDate: Date)}
  <Alert.Root class="border-dashed border-amber-500/30 bg-amber-500/5 shadow-sm">
    <ClockFading class="h-5 w-5 text-amber-500 shrink-0" />
    <Alert.Title class="text-amber-700 dark:text-amber-400">Resubscription Cooldown</Alert.Title>
    <Alert.Description class="text-amber-600/80 dark:text-amber-300/80">
			<p>
				You recently unsubscribed. You can resubscribe or reactivate after
				<span class="font-semibold inline">{formatter.format(unlockDate)}</span>.
			</p>
    </Alert.Description>
  </Alert.Root>
{/snippet}

{#snippet completedAlert()}
  <Alert.Root
    class="relative overflow-hidden rounded-xl border-emerald-500/40 bg-linear-to-br from-emerald-500/10 via-emerald-400/5 to-teal-500/10 shadow-[0_0_25px_rgba(16,185,129,0.15)] ring-1 ring-emerald-500/20 ring-inset"
  >
    <div class="absolute -top-6 -right-6 h-32 w-32 rounded-full bg-emerald-500/20 blur-3xl"></div>

    <Sparkles size={20} class="text-emerald-500 shrink-0" />
    <Alert.Title
      class="bg-linear-to-r from-emerald-600 to-teal-600 bg-clip-text text-lg font-bold tracking-tight text-transparent dark:from-emerald-400 dark:to-teal-400"
    >
      Project Completed!
    </Alert.Title>
    <Alert.Description class="font-medium text-emerald-700/80 dark:text-emerald-300/80">
      Outstanding work! You've successfully finished this project.
    </Alert.Description>
  </Alert.Root>
{/snippet}

{#snippet awaitingAlert()}
  <Alert.Root class="border-dashed border-amber-500/30 bg-amber-500/5 shadow-sm">
    <Hourglass class="h-5 w-5 text-amber-500 shrink-0" />
    <Alert.Title class="text-amber-700 dark:text-amber-400">Project Awaiting</Alert.Title>
    <Alert.Description class="text-amber-600/80 dark:text-amber-300/80">
      The project is currently awaiting further action. Hang tight!
    </Alert.Description>
  </Alert.Root>
{/snippet}

{#snippet deprecatedAlert()}
  <Alert.Root class="border-dashed border-destructive/30 bg-destructive/5 shadow-sm">
    <Ban class="h-5 w-5 text-destructive shrink-0" />
    <Alert.Title class="text-destructive">Project Deprecated</Alert.Title>
    <Alert.Description class="text-destructive/80">
      This project has been deprecated and is no longer accepting new subscriptions.
    </Alert.Description>
  </Alert.Root>
{/snippet}

<svelte:boundary>
  {#snippet failed(error, reset)}
    <Failed {error} {reset} />
  {/snippet}

  {@const members = await context.members()}
  {@const current = members.find((m) => m.userId === page.data.session.userId && !m.leftAt)}

  {#if page.params.userId !== page.data.session.userId && !current}
    <p class="text-xs leading-relaxed text-muted-foreground">
      To view your project page, click
      <a
        href="/users/{page.data.session.userId}/projects/{context.project.entity.id}"
        class="font-medium text-primary underline underline-offset-2 hover:text-primary/80"
      >
        here
      </a>.
    </p>

  {:else if current?.role === 'Pending' && userProject}
    <div class="mb-3 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
      <UserCheck size={14} class="shrink-0 text-muted-foreground" />
      <p class="text-[11px] font-medium text-muted-foreground">You've been invited to this team</p>
    </div>
    <div class="flex gap-2">
      <Button
        class="flex-1"
        loading={UserProjects.accept.pending > 0}
        onclick={() => UserProjects.accept(userProject.entity.id)}
      >
        Accept
      </Button>
      <Button
        variant="outline"
        class="flex-1"
        loading={UserProjects.decline.pending > 0}
        onclick={() => UserProjects.decline(userProject.entity.id)}
      >
        Decline
      </Button>
    </div>

  {:else if userProject?.entity.state === 'Active'}
    <Button
      variant="outline"
      class="w-full"
      loading={Subscription.unsubscribeFromProject.pending > 0}
      onclick={async () => {
        if (!(await unsubscribe)) return;
        Problem.try(async () => {
          await Subscription.unsubscribeFromProject({
            userId: page.data.session.userId,
            projectId: context.project.entity.id
          });
        });
      }}
    >
      Unsubscribe
    </Button>

  {:else if userProject?.entity.state === 'Completed'}
    {@render completedAlert()}
  {:else if userProject?.entity.state === 'Awaiting'}
    {@render awaitingAlert()}
  {:else if isCoolingDown && unlocksAt}
    {@render cooldownAlert(unlocksAt)}
  {:else if context.project.entity.deprecated && current === undefined}
    {@render deprecatedAlert()}
  {:else}
    <svelte:boundary>
      {@const eligible = await User.getEligiblePage({
        id: context.project.entity.id,
        type: 'Project',
        userId: page.data.session.userId
      })}

      {#if eligible.data.length > 0}
        <Button
          class="w-full"
          loading={Subscription.subscribeToProject.pending > 0}
          onclick={async () => {
            if (!(await subscribe)) return;
            Problem.try(async () => {
              await Subscription.subscribeToProject({
                userId: page.data.session.userId,
                projectId: context.project.entity.id
              });
            });
          }}
        >
          {isReactivation ? 'Reactivate Project' : 'Subscribe'}
        </Button>
      {:else}
        <Alert.Root class="border-dashed border-destructive/30 bg-destructive/5 shadow-sm">
          <TriangleAlert size={32} class="stroke-destructive shrink-0" />
          <Alert.Title class="text-destructive">Unable to subscribe</Alert.Title>
          <Alert.Description class="text-destructive/80">
            You do not meet the prerequisites to subscribe to this project at the moment.
          </Alert.Description>
        </Alert.Root>
      {/if}
    </svelte:boundary>
  {/if}
</svelte:boundary>
