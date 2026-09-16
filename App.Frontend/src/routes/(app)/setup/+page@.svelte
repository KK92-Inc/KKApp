<script lang="ts">
	import * as Alert from '$lib/components/alert';
	import * as AlertDialog from '$lib/components/alert-dialog/index.js';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Stepper from '$lib/components/stepper/index.js';
	import { Input } from '$lib/components/input/index.js';
	import { CircleAlert, Cog, KeyRound, Mail, ShieldCog } from '@lucide/svelte';
	import { bootstrap } from './page.remote';
	import * as Field from '$lib/components/field';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import Badge from '$lib/components/badge/badge.svelte';
	import { env } from '$env/dynamic/public';
	import type { components } from '$lib/api/api';
	import { Problem, type ValidationErrors } from '$lib/api';
	import { goto } from '$app/navigation';
	import { toast } from 'svelte-sonner';

	let step = $state(1);
	let disabled = $derived.by(() => {
		if (step === 1) return false;
		return !Object.values(data).every((value) =>
			Boolean(value?.trim())
		);
	});

	let errors = $state<ValidationErrors>({});
	const data = $state<components['schemas']['SystemInitDTO']>({
		email: '',
		firstname: '',
		lastname: '',
		login: ''
	});
</script>

<AlertDialog.Root open>
	<AlertDialog.Content class="min-w-xl">
		<Stepper.Root bind:step>
			<Stepper.Progress>
				<Stepper.Step value={1} title="Welcome" />
				<Stepper.Step value={2} title="Account" />
			</Stepper.Progress>

			<Stepper.Content>
				<!-- Step 1 -->
				<Stepper.Panel value={1} class="space-y-5 pt-2">
					<div class="space-y-1">
						<h3 class="text-base font-semibold text-foreground">Welcome to KK92 Setup</h3>
						<p class="text-sm text-muted-foreground">
							Please ensure the following prerequisites are configured before getting started:
						</p>
					</div>

					<Item.Group class="space-y-2.5">
						<Item.Root variant="muted">
							<Item.Content>
								<Item.Title>
									<Badge class="size-6" variant="outline">1</Badge>
									Creating S3 Buckets
								</Item.Title>
								<Item.Description class="text-xs">
									You need to create the following buckets in S3:
									<Badge class="rounded-xs" variant="outline">avatars</Badge>
									<Badge class="rounded-xs" variant="outline">events</Badge>

									<p class="mt-2">Make sure that these buckets are set to <strong>public</strong>.</p>
								</Item.Description>
								<Button variant="outline" size="sm" class="mt-2" href={env.PUBLIC_S3_ENDPOINT}>
									Edit Buckets
									<Cog />
								</Button>
							</Item.Content>
						</Item.Root>

						<Item.Root variant="muted">
							<Item.Content>
								<Item.Title>
									<Badge class="size-6" variant="outline">2</Badge>
									Setup Keycloak Account
								</Item.Title>
								<Item.Description class="text-xs">
									Make sure to create a proper Keycloak master account instead of using the temporary one. By
									default the login and password are both:
									<Badge class="rounded-xs" variant="outline">admin</Badge>
								</Item.Description>
								<Button
									variant="outline"
									size="sm"
									class="mt-2"
									href="{env.PUBLIC_KC_ORIGIN}/admin/master/console/#/master/users"
								>
									Edit Account
									<ShieldCog />
								</Button>
							</Item.Content>
						</Item.Root>

						<Item.Root variant="muted">
							<Item.Content>
								<Item.Title>
									<Badge class="size-6" variant="outline">3</Badge>
									Configure Keycloak SMTP
								</Item.Title>
								<Item.Description class="text-xs">
									Configure SMTP settings in Keycloak so password resets work for the student realm.
								</Item.Description>

								<Button
									variant="outline"
									size="sm"
									class="mt-2"
									href="{env.PUBLIC_KC_ORIGIN}/admin/master/console/#/student/realm-settings/email"
								>
									Edit SMTP
									<Mail />
								</Button>
							</Item.Content>
						</Item.Root>
					</Item.Group>

					<Separator />

					<!-- Security Warning -->
					<Alert.Root variant="warning">
						<CircleAlert class="h-4 w-4" />
						<Alert.Title>Before opening to the Internet</Alert.Title>
						<Alert.Description class="text-xs">
							Complete this process before exposing the app publicly. Anyone reaching this uncompleted
							instance can create the initial admin account.
						</Alert.Description>
					</Alert.Root>
				</Stepper.Panel>

				<!-- Step 2 -->
				<Stepper.Panel value={2} class="space-y-4 pt-2">
					<Field.Group>
						<Field.Set>
							<Field.Legend>Admin Account</Field.Legend>
							<Field.Description>Set up details for the primary administrator account.</Field.Description>

							<Field.Group class="grid grid-cols-2 gap-4 pt-2">
								<Field.Field>
									<Field.Label for="firstName">First Name*</Field.Label>
									<Input id="firstName" placeholder="Lucien" bind:value={data.firstname} />
									<Field.Error errors={errors['firstname']} />
								</Field.Field>

								<Field.Field>
									<Field.Label for="lastName">Last Name*</Field.Label>
									<Input id="lastName" placeholder="de la Housse" bind:value={data.lastname} />
									<Field.Error errors={errors['lastname']} />
								</Field.Field>

								<Field.Field class="col-span-2">
									<Field.Label for="email">Email*</Field.Label>
									<Input id="email" placeholder="lde-la-h@kkapp.dev" bind:value={data.email} />
									<Field.Description
										>Used primarily for receiving notifications and system updates.</Field.Description
									>
									<Field.Error errors={errors['email']} />
								</Field.Field>

								<Field.Field class="col-span-2">
									<Field.Label for="login">Login*</Field.Label>
									<Input id="login" placeholder="lde-la-h" bind:value={data.login} />
									<Field.Description>Enter your account login handle. This will also be your initial password.</Field.Description>
									<Field.Error errors={errors['login']} />
								</Field.Field>
							</Field.Group>
						</Field.Set>
					</Field.Group>

					<Separator />

					<!-- Password Info Box -->
					<Alert.Root class="border-border bg-muted/40">
						<KeyRound class="h-4 w-4 text-muted-foreground" />
						<Alert.Title class="text-xs font-semibold">Password Setup</Alert.Title>
						<Alert.Description class="text-xs text-muted-foreground">
							Your password will be the same as your login. You will be prompted to set a new password upon
							your first login attempt.
						</Alert.Description>
					</Alert.Root>
				</Stepper.Panel>
			</Stepper.Content>

			<Stepper.Controls
				{disabled}
				finishLabel="Create admin & finish"
				loading={!!bootstrap.pending}
				onfinish={async () => {
					await Problem.try(
						async () => {
							await bootstrap(data);
							await goto('/auth');
							toast.success(`You can now login, your password is: '${data.login}'`)
						},
						{ onValidation: (e) => (errors = e) }
					);
				}}
			/>
		</Stepper.Root>
	</AlertDialog.Content>
</AlertDialog.Root>
