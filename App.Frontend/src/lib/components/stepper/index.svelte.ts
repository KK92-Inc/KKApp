// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from 'svelte';
export { default as Root } from './Stepper.svelte';
export { default as Header } from './StepperHeader.svelte';
export { default as Item } from './StepperItem.svelte';
export { default as Window } from './StepperWindow.svelte';
export { default as WindowItem } from './StepperWindowItem.svelte';
export { default as Actions } from './StepperActions.svelte';

// ============================================================================

export class Context {
	step = $state(1);
	steps = $state<number[]>([]);
	editable = $state(false);
	vertical = $state(false);

	constructor(
		initialStep: number,
		opts: { editable?: boolean; vertical?: boolean } = {}
	) {
		this.step = initialStep;
		this.editable = opts.editable ?? false;
		this.vertical = opts.vertical ?? false;
	}

	registerStep(value: number) {
		if (!this.steps.includes(value)) {
			this.steps = [...this.steps, value].sort((a, b) => a - b);
		}
	}

	unregisterStep(value: number) {
		this.steps = this.steps.filter((s) => s !== value);
	}

	isActive(value: number): boolean {
		return this.step === value;
	}

	isCompleted(value: number): boolean {
		return this.steps.indexOf(value) < this.steps.indexOf(this.step);
	}

	isLastStep(value: number): boolean {
		return this.steps[this.steps.length - 1] === value;
	}

	get isFirst(): boolean {
		return this.steps.indexOf(this.step) <= 0;
	}

	get isLast(): boolean {
		return this.steps.indexOf(this.step) === this.steps.length - 1;
	}

	next() {
		const idx = this.steps.indexOf(this.step);
		if (idx < this.steps.length - 1) this.step = this.steps[idx + 1];

		console.log('next step:', this.step);
	}

	prev() {
		const idx = this.steps.indexOf(this.step);
		if (idx > 0) this.step = this.steps[idx - 1];
		console.log('prev step:', this.step);
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
