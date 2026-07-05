// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for rendering a cursus entity onto the Galaxy
// ============================================================================

import type { components } from '$lib/api/api';
import type { GalaxyNode } from '../types';

// ============================================================================

export type Track = components['schemas']['CursusTrackDO'];
export type TrackNode = components['schemas']['CursusTrackNodeDO'];

function cluster(clusterId: string, members: TrackNode[]): GalaxyNode<TrackNode> {
	return {
		id: clusterId,
		label: members.map((m) => m.goal.name),
		color: 'var(--card)',
		textColor: 'var(--muted-foreground)',
		items: members.map((node) => {
			return {
				id: node.goal.id,
				label: node.goal.name,
				color: node.goal.active ? 'var(--primary)' : 'var(--card)',
				textColor: node.goal.active ? '#fff' : 'var(--muted-foreground)',
				meta: node,
			}
		}),
	};
}

function build(node: TrackNode): GalaxyNode<TrackNode> {
	const groups = new Map<string | null, TrackNode[]>();
	for (const child of node.children ?? []) {
		const key = child.choiceGroup ?? null;
		if (!groups.has(key)) groups.set(key, []);
		groups.get(key)!.push(child);
	}

	const children: GalaxyNode<TrackNode>[] = [];
	for (const [key, members] of groups) {
		if (key === null) members.forEach((m) => children.push(build(m)));
		else children.push(cluster(key, members));
	}

	const item = {
		id: node.goal.id,
		label: node.goal.name,
		color: node.goal.active ? 'var(--primary)' : 'var(--card)',
		textColor: node.goal.active ? '#fff' : 'var(--muted-foreground)',
		meta: node,
	};

	return {
		id: node.goal.id,
		label: [node.goal.name],
		color: item.color,
		textColor: item.textColor,
		items: [item],
		children: children.length ? children : undefined,
	};
}

export const Adapter = {
	/**
	 * Builds a generic tree from a cursus definition (no per-user progress).
	 * @param track The track of the cursus
	 * @returns A GalaxyNode entity to render
	 */
	construct(track: Track): GalaxyNode<TrackNode> {
		const roots = track.nodes ?? [];
		if (roots.length === 0) throw new Error('Cursus track has no nodes.');
		if (roots.length === 1) return build(roots[0]);

		// Multiple top-level nodes → wrap in a synthetic root so the renderer
		// still gets a single tree. Flag: I don't have a real sample of a
		// multi-root CursusTrackDO, so double check this matches your data.
		return {
			id: track.cursusId,
			label: [''],
			color: 'var(--card)',
			textColor: 'var(--muted-foreground)',
			items: [{ id: track.cursusId, label: '', color: 'var(--card)', textColor: 'var(--muted-foreground)', meta: roots[0] }],
			children: roots.map(build),
		};
	}
}
