<script lang="ts">
	import * as Alert from '$lib/components/alert';
	import * as AlertDialog from '$lib/components/alert-dialog/index.js';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Stepper from '$lib/components/stepper/index.js';
	import { Input } from '$lib/components/input/index.js';
	import { Label } from '$lib/components/label/index.js';
	import { CircleAlert, ShieldCheck } from '@lucide/svelte';
	import { bootstrap } from './page.remote';
	import { bootstrapSchema } from './schema';
	import * as Field from '$lib/components/field';

	let step = $state(1);
	let disabled = $derived.by(() => {
		if (step !== 2) return false;

		const hasIssues = (bootstrap.fields?.allIssues?.()?.length ?? 0) > 0;
		const hasRequired =
			bootstrap.fields.email.value?.() !== undefined && bootstrap.fields.login.value?.() !== undefined;

		return !hasRequired || hasIssues;
	});
</script>

<AlertDialog.Root open>
	<AlertDialog.Content class="min-w-xl">
		<AlertDialog.Header>
			<AlertDialog.Title>KKApp — Bootstrap</AlertDialog.Title>
			<AlertDialog.Description>
				<p>Welcome to the bootstrap process.</p>
			</AlertDialog.Description>
		</AlertDialog.Header>

		<form {...bootstrap.preflight(bootstrapSchema)} oninput={() => bootstrap.validate()}>
			<Stepper.Root bind:step>
				<Stepper.Progress>
					<Stepper.Step value={1} title="Welcome" />
					<Stepper.Step value={2} title="Account" />
				</Stepper.Progress>

				<Stepper.Content>
					<Stepper.Panel value={1}>
						<Alert.Root variant="warning">
							<CircleAlert />
							<Alert.Title>Before you continue</Alert.Title>
							<Alert.Description>
								Finish this process before exposing the app publicly, until it's complete, anyone who reaches
								this instance can create the first admin account.
							</Alert.Description>
						</Alert.Root>
					</Stepper.Panel>

					<Stepper.Panel value={2}>
						<Field.Group>
							<Field.Set>
								<Field.Legend>Admin Account</Field.Legend>
								<Field.Description>
									You will need to create the first admin account. You will be prompted for a password on
									first login.
								</Field.Description>
								<Field.Group>
									<Field.Field>
										<Field.Label for="login">Login*</Field.Label>
										<Input id="login" placeholder="lde-la-h" {...bootstrap.fields.login.as('text')} />
										<Field.Description>Enter your account login handle.</Field.Description>
										<Field.Error errors={bootstrap.fields.login.issues()} />
									</Field.Field>
									<!-- <div class="grid grid-cols-2 gap-4"> -->
										<Field.Field class="col-span-2">
											<Field.Label for="email">Email*</Field.Label>
											<Input
												id="email"
												placeholder="lde-la-h@kkapp.dev"
												{...bootstrap.fields.email.as('email')}
											/>
											<Field.Description>Enter your work email.</Field.Description>
											<Field.Error errors={bootstrap.fields.email.issues()} />
										</Field.Field>
									<!-- </div> -->
								</Field.Group>
							</Field.Set>
						</Field.Group>
					</Stepper.Panel>
				</Stepper.Content>

				<Stepper.Controls {disabled} finishLabel="Create admin & finish" loading={!!bootstrap.pending} onfinish={bootstrap.submit}/>
			</Stepper.Root>
		</form>
	</AlertDialog.Content>
</AlertDialog.Root>
