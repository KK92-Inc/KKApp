<script lang="ts">
	import * as Stepper from '$lib/components/stepper/index.svelte';
	import { User, CreditCard, PackageCheck } from '@lucide/svelte';

	// ─── Basic example ────────────────────────────────────────────────────────
	let step = $state(1);

	// ─── Vertical example ────────────────────────────────────────────────────
	let vStep = $state(1);

	// ─── Alt-labels example ──────────────────────────────────────────────────
	let altStep = $state(1);

	// ─── Error example ───────────────────────────────────────────────────────
	let eStep = $state(2);

	// ─── Custom icon example ─────────────────────────────────────────────────
	let iStep = $state(1);
</script>

<!-- ─────────────────────────────────────────────────────────────────────────
  1. BASIC HORIZONTAL STEPPER
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Basic stepper</h2>
	<p class="text-sm text-muted-foreground">Currently on step: <strong>{step}</strong></p>

	<Stepper.Root bind:step class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Account" subtitle="Email & password" />
			<Stepper.Item value={2} title="Profile" subtitle="Name & avatar" />
			<Stepper.Item value={3} title="Review" subtitle="Confirm details" />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={1} class="space-y-4 py-4">
				<p class="text-sm">Enter your email and choose a password.</p>
			</Stepper.WindowItem>

			<Stepper.WindowItem value={2} class="space-y-4 py-4">
				<p class="text-sm">Tell us a bit about yourself.</p>
			</Stepper.WindowItem>

			<Stepper.WindowItem value={3} class="space-y-4 py-4">
				<p class="text-sm">Everything looks good! Hit Finish to continue.</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Done!')}>
			{#snippet prev()}
				<button class="text-sm text-muted-foreground underline underline-offset-2">
					Go back
				</button>
			{/snippet}
		</Stepper.Actions>
	</Stepper.Root>
</section>


<!-- ─────────────────────────────────────────────────────────────────────────
  2. EDITABLE (click completed steps to go back)
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Editable stepper</h2>

	<Stepper.Root bind:step={step} editable class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Account" subtitle="Click to revisit" />
			<Stepper.Item value={2} title="Profile" subtitle="Click to revisit" />
			<Stepper.Item value={3} title="Review" />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={1} class="py-4">
				<p class="text-sm">Step 1 content — editable once completed.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={2} class="py-4">
				<p class="text-sm">Step 2 content.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={3} class="py-4">
				<p class="text-sm">Step 3 content.</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Done!')} />
	</Stepper.Root>
</section>


<!-- ─────────────────────────────────────────────────────────────────────────
  3. ALT LABELS (labels below indicators)
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Alt labels</h2>

	<Stepper.Root bind:step={altStep} altLabels class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Cart" subtitle="Review items" />
			<Stepper.Item value={2} title="Shipping" subtitle="Choose address" />
			<Stepper.Item value={3} title="Payment" subtitle="Enter card" />
			<Stepper.Item value={4} title="Confirm" subtitle="Place order" />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={1} class="py-4">
				<p class="text-sm">Review your cart.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={2} class="py-4">
				<p class="text-sm">Choose your shipping address.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={3} class="py-4">
				<p class="text-sm">Enter your payment details.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={4} class="py-4">
				<p class="text-sm">Confirm and place your order.</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Order placed!')} />
	</Stepper.Root>
</section>


<!-- ─────────────────────────────────────────────────────────────────────────
  4. VERTICAL
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Vertical stepper</h2>

	<Stepper.Root bind:step={vStep} vertical class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Personal info" subtitle="Name, date of birth" />
			<Stepper.Item value={2} title="Contact" subtitle="Phone and email" />
			<Stepper.Item value={3} title="Preferences" subtitle="Notification settings" />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={1} class="py-4">
				<p class="text-sm">Enter your personal information.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={2} class="py-4">
				<p class="text-sm">Enter your contact details.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={3} class="py-4">
				<p class="text-sm">Configure your preferences.</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Saved!')} />
	</Stepper.Root>
</section>


<!-- ─────────────────────────────────────────────────────────────────────────
  5. ERROR STATE
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Error state</h2>

	<Stepper.Root bind:step={eStep} class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Details" complete />
			<Stepper.Item value={2} title="Verification" subtitle="Identity check failed" error />
			<Stepper.Item value={3} title="Finish" />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={2} class="py-4">
				<p class="text-sm text-destructive">
					We couldn't verify your identity. Please try again.
				</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Done!')} />
	</Stepper.Root>
</section>


<!-- ─────────────────────────────────────────────────────────────────────────
  6. CUSTOM ICONS + CUSTOM ACTION BUTTONS
───────────────────────────────────────────────────────────────────────────── -->
<section class="space-y-6 p-8">
	<h2 class="text-xl font-semibold">Custom icons & actions</h2>

	<Stepper.Root bind:step={iStep} altLabels class="rounded-xl border bg-card p-6 shadow-sm">
		<Stepper.Header>
			<Stepper.Item value={1} title="Identity" icon={User} />
			<Stepper.Item value={2} title="Payment" icon={CreditCard} />
			<Stepper.Item value={3} title="Complete" icon={PackageCheck} />
		</Stepper.Header>

		<Stepper.Window>
			<Stepper.WindowItem value={1} class="py-4">
				<p class="text-sm">Verify your identity.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={2} class="py-4">
				<p class="text-sm">Enter payment information.</p>
			</Stepper.WindowItem>
			<Stepper.WindowItem value={3} class="py-4">
				<p class="text-sm">🎉 All done!</p>
			</Stepper.WindowItem>
		</Stepper.Window>

		<Stepper.Actions onfinish={() => alert('Complete!')}>
			{#snippet prev()}
				<button class="text-sm text-muted-foreground underline underline-offset-2">
					Go back
				</button>
			{/snippet}
		</Stepper.Actions>
	</Stepper.Root>
</section>
