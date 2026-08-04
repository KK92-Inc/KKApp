// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from 'svelte';

interface StepperOptions {
	/** Arrange steps top-to-bottom instead of left-to-right. @default false */
	vertical?: boolean;
	/** Allow clicking a completed/active indicator to jump to it. @default false */
	editable?: boolean;
}

export class Stepper {
	current = $state(1);
	steps = $state<number[]>([]);
	vertical = $state(false);
	editable = $state(false);

	constructor(initial: number, options: StepperOptions = {}) {
		this.current = initial;
		this.vertical = options.vertical ?? false;
		this.editable = options.editable ?? false;
	}

	register(id: number) {
		if (!this.steps.includes(id)) {
			this.steps = [...this.steps, id].sort((a, b) => a - b);
		}
	}

	unregister(id: number) {
		this.steps = this.steps.filter((s) => s !== id);
	}

	isActive(id: number): boolean {
		return this.current === id;
	}

	isDone(id: number): boolean {
		return this.steps.indexOf(id) < this.index;
	}

	goto(id: number) {
		if (this.steps.includes(id)) this.current = id;
	}

	next() {
		if (!this.isLast) this.current = this.steps[this.index + 1];
	}

	back() {
		if (!this.isFirst) this.current = this.steps[this.index - 1];
	}

	get index(): number {
		return this.steps.indexOf(this.current);
	}

	get isFirst(): boolean {
		return this.index <= 0;
	}

	get isLast(): boolean {
		return this.index === this.steps.length - 1;
	}
}

export const [getStepperContext, setStepperContext] = createContext<Stepper>();
