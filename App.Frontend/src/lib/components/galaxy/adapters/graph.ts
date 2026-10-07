// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Shared machinery for adapters whose source data is a FLAT node list linked
// by parent ids, which is what every track DO from the API looks like.
// ============================================================================

import type { components } from '$lib/api/api';
import type { GalaxyGraph, GalaxyLayout, GalaxyNode, GalaxyStatus } from '../types';

// ============================================================================

// Exhaustive on purpose: when/if the API grows a mode, this stops compiling
const LAYOUT_BY_MODE: Record<components['schemas']['CursusMode'], GalaxyLayout> = {
	FreeStyle: 'freestyle',
	Ring: 'ring'
};

export function getLayout(mode: components['schemas']['CursusMode']): GalaxyLayout {
	return LAYOUT_BY_MODE[mode] ?? 'freestyle'; // Fallback
}

// ============================================================================

/** How to read a domain node. */
export interface NodeAccessors<TNode> {
	id(node: TNode): string;
	parentId(node: TNode): string | null | undefined;
	label(node: TNode): string;
	status(node: TNode): GalaxyStatus;
}

export interface GraphOptions {
	layout: GalaxyLayout;
	legend: readonly GalaxyStatus[];
}

// ============================================================================


const HUB_ID = '::hub';

/**
 * Builds a single-rooted graph from a flat node list.
 *
 * - Roots are nodes with no parent, or whose parent isn't in the list
 *   (orphans are shown rather than silently dropped).
 * - One root is used as-is. Several roots are joined by a hub: a synthetic
 *   node with no `meta`, so it can't produce a bogus select payload.
 * - Children keep the order of the input.
 * - Nodes that can't be reached from a root (a parent cycle) are left out and
 *   reported in the console, rather than hanging or throwing.
 */
export function buildGraph<TNode>(
	nodes: readonly TNode[],
	accessors: NodeAccessors<TNode>,
	options: GraphOptions
): GalaxyGraph<TNode> | null {
	if (nodes.length === 0) return null;

	const known = new Set(nodes.map(accessors.id));
	const childrenOf = new Map<string, TNode[]>();
	const roots: TNode[] = [];

	for (const node of nodes) {
		const parent = accessors.parentId(node);
		if (!parent || !known.has(parent)) {
			roots.push(node);
			continue;
		}
		const siblings = childrenOf.get(parent);
		if (siblings) siblings.push(node);
		else childrenOf.set(parent, [node]);
	}

	const visit = (node: TNode): GalaxyNode<TNode> => {
		const id = accessors.id(node);
		return {
			id,
			label: accessors.label(node),
			status: accessors.status(node),
			meta: node,
			children: (childrenOf.get(id) ?? []).map(visit)
		};
	};

	const tops = roots.map(visit);
	if (tops.length === 0) {
		console.warn(`Galaxy: ${nodes.length} nodes but none can be a root (do the parent links form a cycle?).`);
		return null;
	}

	const root: GalaxyNode<TNode> =
		tops.length === 1 ? tops[0] : { id: HUB_ID, label: '', status: 'default', meta: null, children: tops };

	const flat = flatten(root);
	if (flat.length !== nodes.length) {
		console.warn(`Galaxy: ${nodes.length} nodes in, ${flat.length} placed. The rest are unreachable (parent cycle?).`);
	}

	return { layout: options.layout, root, nodes: flat, legend: options.legend };
}

/** Depth-first, parents before children, hub excluded. */
function flatten<TNode>(root: GalaxyNode<TNode>): GalaxyNode<TNode>[] {
	const out: GalaxyNode<TNode>[] = [];
	const visit = (node: GalaxyNode<TNode>) => {
		if (node.meta !== null) out.push(node);
		node.children.forEach(visit);
	};
	visit(root);
	return out;
}
