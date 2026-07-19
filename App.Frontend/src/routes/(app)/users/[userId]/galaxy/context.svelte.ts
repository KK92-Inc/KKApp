import { createContext } from 'svelte';
import type { Attachment } from 'svelte/attachments';
import * as UserCursus from '$lib/remotes/user-cursus.remote';
import { GalaxyRenderer } from '$lib/components/galaxy/render';
import { type Track, type TrackNode } from '$lib/components/galaxy/adapters/user-cursus';
import type { GalaxyNode } from '$lib/components/galaxy/types';

// ============================================================================
// TEMP: fake progressed track for testing the galaxy without a real API call.
// Flip this off (or delete the block) once you're done poking at it.
// ============================================================================

const USE_FAKE_TRACK = false;

function n(node: TrackNode): TrackNode {
	return node;
}

function buildFakeTrack(): Track {
	const nodes: TrackNode[] = [
		// --- root -------------------------------------------------------------
		n({ goalId: 'root', name: 'Common Core', slug: 'common-core', isUnlocked: true, state: 'Completed' }),

		// --- main chain: piscine -> c-basics -> (decided) algo choice ---------
		n({ goalId: 'piscine', name: 'Piscine', slug: 'piscine', isUnlocked: true, parentGoalId: 'root', state: 'Completed' }),
		n({ goalId: 'c-basics', name: 'C Basics', slug: 'c-basics', isUnlocked: true, parentGoalId: 'piscine', state: 'Completed' }),

		// choice group: already decided — Bubble Sort was picked, others are dead
		n({ goalId: 'algo-bubble', name: 'Bubble Sort', slug: 'bubble-sort', isUnlocked: false, parentGoalId: 'c-basics', choiceGroup: 'algo-group', state: null }),
		n({ goalId: 'algo-quick', name: 'Quick Sort', slug: 'quick-sort', isUnlocked: true, parentGoalId: 'c-basics', choiceGroup: 'algo-group', state: 'Active' }),
		n({ goalId: 'algo-merge', name: 'Merge Sort', slug: 'merge-sort', isUnlocked: false, parentGoalId: 'c-basics', choiceGroup: 'algo-group', state: null }),
		n({ goalId: 'algo2-merge', name: 'Merge Sort', slug: 'merge-sort', isUnlocked: false, parentGoalId: 'c-basics', choiceGroup: 'algo-group', state: null }),
		n({ goalId: 'algo3-merge', name: 'Merge Sort', slug: 'merge-sort', isUnlocked: false, parentGoalId: 'c-basics', choiceGroup: 'algo-group', state: null }),

		// --- continuing the chosen path: currently in progress -----------------
		n({ goalId: 'push-swap', name: 'Push Swap', slug: 'push-swap', isUnlocked: true, parentGoalId: 'algo-bubble', state: 'Active' }),

		// a sibling that's queued up next
		n({ goalId: 'minitalk', name: 'Minitalk', slug: 'minitalk', isUnlocked: true, parentGoalId: 'push-swap', state: 'Awaiting' }),

		// choice group: NOT decided yet — all three open, none chosen
		n({ goalId: 'net-practice', name: 'NetPractice', slug: 'netpractice', isUnlocked: true, parentGoalId: 'push-swap', choiceGroup: 'net-group', state: 'Inactive' }),
		n({ goalId: 'netwatch', name: 'Netwatch', slug: 'netwatch', isUnlocked: true, parentGoalId: 'push-swap', choiceGroup: 'net-group', state: 'Inactive' }),
		n({ goalId: 'inception', name: 'Inception', slug: 'inception', isUnlocked: true, parentGoalId: 'push-swap', choiceGroup: 'net-group', state: 'Inactive' }),

		// locked frontier — beyond an undecided group, still fully locked
		n({ goalId: 'webserv', name: 'WebServ', slug: 'webserv', isUnlocked: false, parentGoalId: 'net-practice', state: null }),
		n({ goalId: 'ft-irc', name: 'ft_irc', slug: 'ft-irc', isUnlocked: false, parentGoalId: 'netwatch', state: null }),

		// --- second branch off root: another completed chain + a dead end -----
		n({ goalId: 'rank02-exam', name: 'Exam Rank 02', slug: 'exam-rank-02', isUnlocked: true, parentGoalId: 'root', state: 'Completed' }),
		n({ goalId: 'libft', name: 'Libft', slug: 'libft', isUnlocked: true, parentGoalId: 'rank02-exam', state: 'Completed' }),
		n({ goalId: 'get-next-line', name: 'get_next_line', slug: 'gnl', isUnlocked: true, parentGoalId: 'rank02-exam', state: 'Awaiting' }),

		// --- far, fully locked frontier straight off root ----------------------
		n({ goalId: 'exam-rank-06', name: 'Exam Rank 06', slug: 'exam-rank-06', isUnlocked: true, parentGoalId: 'root', state: null }),
	];

	return {
		cursusId: 'fake-cursus',
		name: '42cursus (fake)',
		completionMode: 'FreeStyle',
		nodes,
	};
}

// ============================================================================

export class Context {
	public readonly renderer = new GalaxyRenderer<TrackNode>();

	constructor(
		public readonly userId: () => string,
		public readonly userCursusId: () => string | undefined,
	) {}

	get cursi() {
		return UserCursus.getPageByUser({ userId: this.userId(), size: 100 });
	}

	get track() {
		if (USE_FAKE_TRACK) return Promise.resolve(buildFakeTrack());

		const id = this.userCursusId();
		return id ? UserCursus.getTrack(id) : Promise.resolve(null);
	}

	attachment(tree: GalaxyNode<TrackNode>): Attachment<SVGElement> {
		return (element) => this.renderer.mount(element, tree);
	}

	focus(itemId: string): void {
		this.renderer.focus(itemId);
	}
}

export const [getContext, setContext] = createContext<Context>();
