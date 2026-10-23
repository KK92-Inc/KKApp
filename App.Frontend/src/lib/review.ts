// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// File containing everything related to reviews e.g:
// - Review Icons and descriptions
// - Common review actions
// - ...
// ============================================================================

import type { components } from '$lib/api/api';
import type { BadgeVariant } from '$lib/components/badge/badge.svelte';
import { Ban, Bot, CheckCheck, LaptopMinimalCheck, Search, User, Users, X } from '@lucide/svelte';

type Schemas = components['schemas'];
export type ReviewKind = Schemas['ReviewKinds'];
export type ReviewState = Schemas['ReviewState'];
export type RoundState = Schemas['ReviewRoundState'];
export type Annotation = Schemas['AnnotationData'];
export type CommentAnnotation = Extract<Annotation, { $type: 'Comment' }>;

// ============================================================================
// Metadata
// ============================================================================

/** Icon, label and explanation for every kind of review. */
export const KINDS = {
	Self: {
		icon: User,
		label: 'Self',
		description: 'A self evaluation done by the team leader of the project before others look at it.'
	},
	Peer: {
		icon: Users,
		label: 'Peer',
		description: 'An onsite evaluation done by a fellow student, they must start it from onsite.'
	},
	Async: {
		icon: LaptopMinimalCheck,
		label: 'Async',
		description: 'A remote evaluation, the reviewer can look at the submission whenever it suits them.'
	},
	Auto: {
		icon: Bot,
		label: 'Auto',
		description: 'This is an automated review. No human input is required.'
	}
} as const;

/** Label and badge style for the state of a single review. */
export const STATES = {
	Pending: { label: 'Pending', variant: 'secondary' },
	InProgress: { label: 'In Progress', variant: 'default' },
	Finished: { label: 'Finished', variant: 'success' },
	Cancelled: { label: 'Cancelled', variant: 'destructive' }
} as const satisfies Record<ReviewState, { label: string; variant: BadgeVariant }>;

/** Icon, label and badge style for the state of an evaluation round. */
export const ROUND_STATES = {
	Open: { icon: Search, label: 'Open', variant: 'outline' },
	Passed: { icon: CheckCheck, label: 'Pass', variant: 'success' },
	Failed: { icon: X, label: 'Fail', variant: 'destructive' },
	Cancelled: { icon: Ban, label: 'Cancelled', variant: 'destructive' }
} as const satisfies Record<RoundState, { icon: unknown; label: string; variant: BadgeVariant }>;

/** Text for the confirmation dialogs so every place that asks uses the same words. */
export const CONFIRM = {
	claim: (project: string) => ({
		title: `Claim review slot for ${project}`,
		message:
			"Claiming a review means you intend to start it within 48 hours. You can then choose to start whenever you're ready in that time window."
	}),
	start: (project: string) => ({
		title: `Start review for ${project}`,
		message: 'Once you start you have 24 Hours to complete it.'
	}),
	cancel: () => ({
		title: 'Are you sure you want to cancel this review ?',
		message: 'If this review is a part of round it will be available to someone else. Otherwise you can safely cancel this review.'
	}),
	cancelRound: () => ({
		title: 'Cancel this evaluation round ?',
		message: 'All the reviews below this round are cancelled with it.'
	})
} as const;

// ============================================================================
// Helpers
// ============================================================================

/** The annotations as the backend expects them: the file comments and the conclusion. */
export function toAnnotations(comments: Iterable<CommentAnnotation>, conclusion: string): Annotation[] {
	const out: Annotation[] = [...comments];
	const body = conclusion.trim();
	if (body) out.push({ $type: 'Conclusion', body });
	return out;
}
