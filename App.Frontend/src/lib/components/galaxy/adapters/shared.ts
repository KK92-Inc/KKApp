// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Shared machinery for adapters whose source data is a FLAT node list linked
// by parent ids (which is what every track DO from the API looks like).
// ============================================================================

import type { GalaxyItem, GalaxyNode } from '../types';
import type { GalaxyAdapter } from './index';

// ============================================================================
// Styling
// ============================================================================

export interface NodeStyle {
	/** Resolved CSS color for the circle fill. */
	color: string;
	/** Resolved CSS color for the label drawn on top of it. */
	textColor: string;
}

/** Look of a node that carries no progress information. */
export const NEUTRAL: NodeStyle = {
	color: 'var(--card)',
	textColor: 'var(--muted-foreground)'
};

// ============================================================================
// Grouping
// ============================================================================

export interface FlatNodeAccessors<T> {
	parentId(node: T): string | null | undefined;
	/** Optional: sources without choice groups simply omit it. */
	choiceGroup?(node: T): string | null | undefined;
}

/**
 * Groups a flat list of nodes into `parentId -> choiceGroup -> members[]`
 * buckets. Nodes without a parent id are skipped (they are roots).
 * Insertion order is preserved, so the input order is the render order.
 */
export function groupByParent<T>(nodes: readonly T[], accessors: FlatNodeAccessors<T>) {
	const map = new Map<string, Map<string | null, T[]>>();
	for (const node of nodes) {
		const parentId = accessors.parentId(node);
		if (!parentId) continue;
		if (!map.has(parentId)) map.set(parentId, new Map());
		const groups = map.get(parentId)!;
		const key = accessors.choiceGroup?.(node) ?? null;
		if (!groups.has(key)) groups.set(key, []);
		groups.get(key)!.push(node);
	}
	return map;
}

// ============================================================================
// Tree building
// ============================================================================

export interface TreeSpec<TNode> extends FlatNodeAccessors<TNode> {
	id(node: TNode): string;
	label(node: TNode): string;
	/** Colors for a node. Return `NEUTRAL` if there's no progress to show. */
	style(node: TNode): NodeStyle;
	/**
	 * Optional override for how a choice-group cluster is drawn. The default is
	 * a neutral core listing every member's label. Only used when `choiceGroup`
	 * is provided.
	 */
	cluster?(clusterId: string, items: GalaxyItem<TNode>[]): GalaxyNode<TNode>;
}

/** Name/id used for the wrapper root when a track has several top-level nodes. */
export interface SyntheticRoot {
	id: string;
	label: string;
}

function toItem<TNode>(spec: TreeSpec<TNode>, node: TNode): GalaxyItem<TNode> {
	return { id: spec.id(node), label: spec.label(node), ...spec.style(node), meta: node };
}

/** Depth-first, parent before children. Synthetic nodes have no items. */
export function flattenTree<TMeta>(tree: GalaxyNode<TMeta>): TMeta[] {
	const out: TMeta[] = [];
	const visit = (node: GalaxyNode<TMeta>) => {
		for (const item of node.items) out.push(item.meta);
		node.children?.forEach(visit);
	};
	visit(tree);
	return out;
}

/**
 * Builds a single-rooted `GalaxyNode` tree from a flat node list.
 *
 * - Roots are nodes with no parent, or whose parent isn't in the list
 *   (orphans are shown rather than silently dropped).
 * - One root is returned as-is. Several roots are wrapped in a synthetic one
 *   with no items, so it can't emit a bogus click/focus payload.
 * - Members of a choice group become one clustered node. Clusters are leaves
 *   in the renderer: children of a cluster member are not drawn.
 *
 * `synthetic.label` doubles as the track name in error messages.
 */
export function buildTree<TNode>(
	nodes: readonly TNode[],
	spec: TreeSpec<TNode>,
	synthetic: SyntheticRoot
): GalaxyNode<TNode> {
	const what = synthetic.label;
	if (nodes.length === 0) throw new Error(`Track "${what}" has no nodes.`);

	const known = new Set(nodes.map(spec.id));
	const groups = groupByParent(nodes, spec);

	const visit = (node: TNode): GalaxyNode<TNode> => {
		const item = toItem(spec, node);
		const children: GalaxyNode<TNode>[] = [];

		for (const [key, members] of groups.get(item.id) ?? []) {
			if (key === null) {
				for (const member of members) children.push(visit(member));
			} else {
				const items = members.map((m) => toItem(spec, m));
				children.push(
					spec.cluster?.(key, items) ?? {
						id: key,
						label: items.map((i) => i.label),
						...NEUTRAL,
						items
					}
				);
			}
		}

		return {
			id: item.id,
			label: [item.label],
			color: item.color,
			textColor: item.textColor,
			items: [item],
			children: children.length ? children : undefined
		};
	};

	const roots = nodes.filter((n) => {
		const parent = spec.parentId(n);
		return !parent || !known.has(parent);
	});
	if (roots.length === 0) {
		throw new Error(`Track "${what}" has no root node (do the parent links form a cycle?).`);
	}

	const tree: GalaxyNode<TNode> =
		roots.length === 1
			? visit(roots[0])
			: {
					id: synthetic.id,
					label: [synthetic.label],
					...NEUTRAL,
					items: [],
					children: roots.map(visit)
				};

	// Catch data problems (cycles, choice-group members with children) that
	// would otherwise make nodes vanish from the graph without any error.
	const drawn = flattenTree(tree).length;
	if (drawn !== nodes.length) {
		console.warn(`Track "${what}": ${nodes.length} nodes in, ${drawn} placed in the tree.`);
	}

	return tree;
}

/**
 * Wires a `TreeSpec` up as a full `GalaxyAdapter`, so a concrete adapter is
 * just "which fields to read, and how to color them".
 */
export function createFlatAdapter<TTrack, TNode>(config: {
	nodes(track: TTrack): readonly TNode[] | null | undefined;
	synthetic(track: TTrack): SyntheticRoot;
	spec: TreeSpec<TNode>;
}): GalaxyAdapter<TTrack, TNode> {
	return {
		construct(track) {
			const synthetic = config.synthetic(track);
			return buildTree(config.nodes(track) ?? [], config.spec, synthetic);
		},
		flatten: flattenTree
	};
}
