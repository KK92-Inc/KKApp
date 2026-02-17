// ============================================================================
// Dialog Actions Context
// Provides programmatic confirm, alert, and input dialogs via a fluent API.
// ============================================================================

import { createContext } from "svelte";

// ============================================================================

export type DialogType = "confirm" | "alert";

export interface DialogOptions {
	type: DialogType;
	title: string;
	message: string;
	/** Label for the confirm/OK button */
	confirmLabel?: string;
	/** Label for the cancel button (confirm only) */
	cancelLabel?: string;
	/** When set, shows an input field and the confirm button is disabled until the value matches */
	inputMatch?: string;
	/** Placeholder for the input field */
	placeholder?: string;
}

interface DialogEntry {
	options: DialogOptions;
	resolve: (confirmed: boolean) => void;
}

// ============================================================================

/**
 * Fluent builder returned by `dialog.confirm()` and `dialog.alert()`.
 * Supports chaining `.input()`, `.ok()`, `.cancel()` and is `PromiseLike<boolean>`.
 */
export class DialogBuilder implements PromiseLike<boolean> {
	private _options: DialogOptions;
	private _onOk?: (() => void | Promise<void>);
	private _onCancel?: (() => void | Promise<void>);
	private _ctx: DialogActionContext;

	constructor(ctx: DialogActionContext, options: DialogOptions) {
		this._ctx = ctx;
		this._options = options;
	}

	/** Require the user to type `match` before the confirm button is enabled */
	input(match: string, placeholder?: string): this {
		this._options.inputMatch = match;
		this._options.placeholder = placeholder ?? `Type "${match}" to confirm`;
		return this;
	}

	/** Register a callback that runs when the user confirms */
	ok(fn: () => void | Promise<void>): this {
		this._onOk = fn;
		return this;
	}

	/** Register a callback that runs when the user cancels */
	cancel(fn: () => void | Promise<void>): this {
		this._onCancel = fn;
		return this;
	}

	/** Set the confirm button label */
	confirmLabel(label: string): this {
		this._options.confirmLabel = label;
		return this;
	}

	/** Set the cancel button label */
	cancelLabel(label: string): this {
		this._options.cancelLabel = label;
		return this;
	}

	/** Opens the dialog and returns a promise resolving to `true` (confirmed) or `false` (dismissed). */
	private _execute(): Promise<boolean> {
		return new Promise<boolean>((resolve) => {
			this._ctx.inputValue = "";
			this._ctx.current = { options: this._options, resolve };
		}).then(async (confirmed) => {
			if (confirmed) {
				await this._onOk?.();
			} else {
				await this._onCancel?.();
			}
			return confirmed;
		});
	}

	then<TResult1 = boolean, TResult2 = never>(
		onfulfilled?: ((value: boolean) => TResult1 | PromiseLike<TResult1>) | null,
		onrejected?: ((reason: unknown) => TResult2 | PromiseLike<TResult2>) | null,
	): Promise<TResult1 | TResult2> {
		return this._execute().then(onfulfilled, onrejected);
	}
}

// ============================================================================

export class DialogActionContext {
	current = $state<DialogEntry | null>(null);
	inputValue = $state("");

	/**
	 * Show a confirm dialog. Returns a chainable `DialogBuilder`.
	 * @example
	 * ```ts
	 * await dialog.confirm("Delete?", "This is permanent.")
	 *   .input(key.title)
	 *   .ok(() => form.submit())
	 *   .cancel(() => console.log("cancelled"));
	 * ```
	 */
	confirm(title: string = "Are you sure?", message: string = ""): DialogBuilder {
		return new DialogBuilder(this, { type: "confirm", title, message });
	}

	/**
	 * Show an alert dialog. Returns a chainable `DialogBuilder`.
	 * Only has an OK button (cancel is not shown).
	 */
	alert(title: string = "Alert", message: string = ""): DialogBuilder {
		return new DialogBuilder(this, { type: "alert", title, message });
	}

	/** @internal Resolve the current dialog with a positive result */
	accept() {
		if (!this.current) return;
		this.current.resolve(true);
		this.current = null;
		this.inputValue = "";
	}

	/** @internal Resolve the current dialog with a negative result */
	dismiss() {
		if (!this.current) return;
		this.current.resolve(false);
		this.current = null;
		this.inputValue = "";
	}
}

// ============================================================================

export const [get, init] = createContext<DialogActionContext>();
