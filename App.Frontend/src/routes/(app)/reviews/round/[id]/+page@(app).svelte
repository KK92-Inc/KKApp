<script lang="ts">
	import * as Reviews from '$lib/remotes/review.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as User from '$lib/remotes/user.remote';
	import * as Card from '$lib/components/card';
	import * as Table from '$lib/components/table';
	import * as Avatar from '$lib/components/avatar';
	import type { PageProps } from './$types';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';

	const { params }: PageProps = $props();
	const round = $derived(await Reviews.getRound(params.id));
	const session = $derived(await UserProject.get(round.userProjectId));
</script>

<Card.Root class="container mx-auto my-8">
	<Card.Header class="">
		<Card.Title class="text-2xl font-bold text-muted-foreground">
			Evaluation Round {round.number}: {session.project.name}
		</Card.Title>
		<Card.Description class="flex items-center gap-3">
			<svelte:boundary>
				{@const members = await UserProject.getMembersPage({ id: round.userProjectId, active: true })}
				{#each members.data as member (member.id)}
					<Avatar.Root>
						<Avatar.Image src={member.user.avatarUrl} alt="@{member.user.login}" />
						<Avatar.Fallback>{member.user.login.slice(0, 2).toUpperCase()}</Avatar.Fallback>
					</Avatar.Root>
				{/each}
			</svelte:boundary>
			<Badge variant="secondary" class="rounded-sm">{round.ref}</Badge>
			<Badge variant="secondary" class="rounded-sm">{round.sha.slice(0, 8)}</Badge>
		</Card.Description>
	</Card.Header>

	<Separator />

	<Card.Content>
		<Table.Root>
			<Table.Header>
				<Table.Row>
					<Table.Head>Reviewer</Table.Head>
					<Table.Head>Type</Table.Head>
					<Table.Head>Duration</Table.Head>
					<Table.Head>Outcome</Table.Head>
					<Table.Head class="text-right">Actions</Table.Head>
				</Table.Row>
			</Table.Header>

			<Table.Body>
				{#each round.slots as slot (slot.reviewId)}
					<Table.Row class="h-16">
						<Table.Cell>
							{slot.reviewerId}
							<!-- {#if slot.reviewerId}
								<svelte:boundary>
									{#snippet failed()}
										Something went wrong...
									{/snippet}

									{@const reviewer = await User.get(slot.reviewId)}
									<div class="flex items-center gap-3">
										<Avatar.Root class="h-9 w-9">
											<Avatar.Image src={reviewer.avatarUrl} alt={reviewer.displayName ?? reviewer.login} />
											<Avatar.Fallback>
												{(reviewer.displayName ?? reviewer.login ?? 'U').slice(0, 2).toUpperCase()}
											</Avatar.Fallback>
										</Avatar.Root>

										<div class="flex flex-col">
											<span class="text-sm leading-none font-medium">
												{reviewer.displayName ?? reviewer.login}
											</span>
											<span class="mt-1 text-xs text-muted-foreground">
												@{reviewer.login}
											</span>
										</div>
									</div>
								</svelte:boundary>
							{:else}
								<div class="flex items-center gap-3 text-muted-foreground">
									<Avatar.Root class="h-9 w-9">
										<Avatar.Fallback>?</Avatar.Fallback>
									</Avatar.Root>
									<span class="text-sm italic">Unassigned</span>
								</div>
							{/if} -->
						</Table.Cell>
					</Table.Row>
				{/each}
			</Table.Body>
		</Table.Root>
	</Card.Content>
</Card.Root>

<!-- <div class="container mx-auto py-8 max-w-5xl space-y-6">
  <Card.Root>
    <Card.Header class="flex flex-row items-center justify-between pb-6 border-b">
      <div class="space-y-1">
        <Card.Title class="text-2xl font-bold">
          Evaluation Round
        </Card.Title>
        <Card.Description>
          ID: {params.id}
        </Card.Description>
      </div>

      {#if round?.projectId}
        <Button.Root href="/projects/{round.projectId}" variant="outline">
          View Project
        </Button.Root>
      {/if}
    </Card.Header>

    <Card.Content class="pt-6">
      <Table.Root>
        <Table.Header>
          <Table.Row>
            <Table.Head>Reviewer</Table.Head>
            <Table.Head>Type</Table.Head>
            <Table.Head>Duration</Table.Head>
            <Table.Head>Outcome</Table.Head>
            <Table.Head class="text-right">Actions</Table.Head>
          </Table.Row>
        </Table.Header>

        <Table.Body>
          {#each round.slots as slot (slot.reviewId ?? slot.id)}
            <Table.Row class="h-16">
              <Table.Cell>
                {#if slot.reviewer}
                  <div class="flex items-center gap-3">
                    <Avatar.Root class="h-9 w-9">
                      <Avatar.Image
                        src={slot.reviewer.avatarUrl}
                        alt={slot.reviewer.displayName ?? slot.reviewer.login}
                      />
                      <Avatar.Fallback>
                        {(slot.reviewer.displayName ?? slot.reviewer.login ?? "U")
                          .slice(0, 2)
                          .toUpperCase()}
                      </Avatar.Fallback>
                    </Avatar.Root>

                    <div class="flex flex-col">
                      <span class="text-sm font-medium leading-none">
                        {slot.reviewer.displayName ?? slot.reviewer.login}
                      </span>
                      <span class="text-xs text-muted-foreground mt-1">
                        @{slot.reviewer.login}
                      </span>
                    </div>
                  </div>
                {:else}
                  <div class="flex items-center gap-3 text-muted-foreground">
                    <Avatar.Root class="h-9 w-9">
                      <Avatar.Fallback>?</Avatar.Fallback>
                    </Avatar.Root>
                    <span class="text-sm italic">Unassigned</span>
                  </div>
                {/if}
              </Table.Cell>

              <Table.Cell>
                <Badge variant="secondary">
                  {slot.type ?? "Peer"}
                </Badge>
              </Table.Cell>

              <Table.Cell class="text-sm">
                {formatDuration(slot.startedAt, slot.completedAt)}
              </Table.Cell>

              <Table.Cell>
                {#if !slot.reviewer || slot.state === "Pending"}
                  <Badge variant="outline">Pending</Badge>
                {:else}
                  <svelte:boundary>
                    {#snippet failed()}
                      <Badge variant="destructive">Error</Badge>
                    {/snippet}

                    {@const annotations = await Reviews.getAnnotations({
                      reviewId: slot.reviewId,
                      type: "Conclusion"
                    })}
                    {@const conclusion = annotations?.[0]}

                    {#if conclusion?.passed === true || conclusion?.value === "pass" || conclusion?.verdict === "PASS"}
                      <Badge class="bg-emerald-600 text-white hover:bg-emerald-700">
                        Passed
                      </Badge>
                    {:else if conclusion?.passed === false || conclusion?.value === "fail" || conclusion?.verdict === "FAIL"}
                      <Badge variant="destructive">
                        Failed
                      </Badge>
                    {:else if conclusion}
                      <Badge variant="secondary">
                        {conclusion.value ?? "Completed"}
                      </Badge>
                    {:else}
                      <Badge variant="outline">
                        {slot.state ?? "In Progress"}
                      </Badge>
                    {/if}
                  </svelte:boundary>
                {/if}
              </Table.Cell>

              <Table.Cell class="text-right space-x-2">
                {#if !slot.reviewer}
                  <Button
                    size="sm"
                    variant="default"
                    onclick={() => handleClaimSlot(slot.id)}
                  >
                    Claim Slot
                  </Button>
                {/if}

                {#if slot.reviewId}
                  <Button
                    size="sm"
                    variant="ghost"
                    href="/reviews/{slot.reviewId}"
                  >
                    View Review
                  </Button>
                {/if}
              </Table.Cell>
            </Table.Row>
          {/each}
        </Table.Body>
      </Table.Root>
    </Card.Content>
  </Card.Root>
</div> -->
