// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { NodeDatum, NodeType, Track, TrackNode } from './types';

// ============================================================================

/**
 * Assembles a flat API node list into a D3-ready hierarchy.
 * @param track
 * @returns
 */
export function getTree(track: Track): NodeDatum {
	const { nodes } = track;
	const map = new Map<string, Map<string | null, TrackNode[]>>();

	for (const node of nodes) {
		if (!node.parentGoalId) continue;
		if (!map.has(node.parentGoalId)) map.set(node.parentGoalId, new Map());
		const groups = map.get(node.parentGoalId)!;
		const key = node.choiceGroup ?? null;
		if (!groups.has(key)) groups.set(key, []);
		groups.get(key)!.push(node);
	}

	function toNode(apiNode: TrackNode, type: NodeType): NodeDatum {
		const groups = map.get(apiNode.goalId);
		const children: NodeDatum[] = [];

		if (groups) {
			for (const [groupKey, members] of groups) {
				if (groupKey === null) {
					for (const child of members) children.push(toNode(child, 'goal'));
				} else {
					// Collapse all members into one visual node
					children.push({ id: groupKey, name: members[0].name, choices: members, type: 'goal' });
				}
			}
		}

		return {
			id: apiNode.goalId,
			name: apiNode.name,
			choices: [apiNode],
			type,
			children: children.length > 0 ? children : undefined,
		};
	}

	const root = nodes.find((n) => !n.parentGoalId);
	if (!root) throw new Error(`Track "${track.name}" has no root node.`);
	return toNode(root, 'root');
}

/**
 * Extracts a flat goal list out of an assembled tree.
 * @param tree
 * @returns
 */
export function getGoals(tree: NodeDatum): TrackNode[] {
	const flat: TrackNode[] = [];
	const traverse = (node: NodeDatum) => {
		flat.push(...node.choices);
		node.children?.forEach(traverse);
	};
	traverse(tree);
	return flat;
}
