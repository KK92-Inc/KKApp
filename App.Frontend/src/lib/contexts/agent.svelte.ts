// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from 'svelte';
import { Problem } from '$lib/api';
import { chat } from '$lib/remotes/agent.remote';
import type { components } from '$lib/api/api';

// ============================================================================

type AgentDecision = components['schemas']['AgentDecision'];
type AgentMessage = components['schemas']['AgentMessageDTO'];
type AgentPending = components['schemas']['AgentPendingDO'];

// ============================================================================

/** State of one agent conversation (a single context window). */
export class AgentChat {
	messages = $state<AgentMessage[]>([]);
	pending = $state<AgentPending | null>(null);
	loading = $state(false);
	error = $state<string | null>(null);

	/** Ask something. Ignored while a request is running or a decision is outstanding. */
	async send(text: string) {
		const content = text.trim();
		if (!content || this.loading || this.pending) return;

		this.messages.push({ role: 'user', content });
		await this.run();
	}

	/** Answer the outstanding approval. */
	async decide(decision: AgentDecision) {
		if (!this.pending || this.loading) return;
		await this.run(decision);
	}

	/** Re-send the conversation after a failure. */
	async retry() {
		if (this.loading) return;
		await this.run();
	}

	/** Forget everything and start a new context window. */
	reset() {
		if (this.loading) return;
		this.messages = [];
		this.pending = null;
		this.error = null;
	}

	private async run(decision?: AgentDecision) {
		this.loading = true;
		this.error = null;

		const waiting = this.pending;
		this.pending = null;

		try {
			const reply = await chat({
				// serializer can't take reactive proxies
				messages: $state.snapshot(this.messages),
				decision
			});

			this.messages.push(...reply.messages);
			this.pending = reply.pending;
		} catch (e) {
			const problem = Problem.resolve(e);
			this.error = problem.kind === 'validation' ? 'The request was rejected.' : problem.message;
			this.pending = waiting;
		} finally {
			this.loading = false;
		}
	}
}

// ============================================================================

export const [get, init] = createContext<AgentChat>();
