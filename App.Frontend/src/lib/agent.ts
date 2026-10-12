// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// Hand-written until `bun run backend:types` picks up POST /agent/chat, at which point these can be
// replaced by components['schemas'][...]. The shapes mirror AgentMessageDTO / AgentReplyDO.

// ============================================================================

export type AgentRole = 'user' | 'assistant' | 'tool';
export type AgentDecision = 'Approve' | 'Deny';

export interface AgentToolCall {
	name: string;
	arguments: Record<string, unknown>;
}

export interface AgentMessage {
	role: AgentRole;
	content: string;
	/** Tool calls the assistant made in this message. */
	toolCalls?: AgentToolCall[] | null;
	/** For role === 'tool': which tool produced this result. */
	toolName?: string | null;
}

export interface AgentPendingCall {
	name: string;
	description: string;
	arguments: Record<string, unknown>;
	/** False for read-only calls that are only held back because they came with a write. */
	requiresApproval: boolean;
}

export interface AgentPending {
	calls: AgentPendingCall[];
}

export interface AgentReply {
	/** New messages produced by this request, append them to the conversation. */
	messages: AgentMessage[];
	/** Set when the agent wants to change something and needs a human to approve it. */
	pending: AgentPending | null;
}
