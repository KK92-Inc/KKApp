// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

export interface FlatNodeAccessors<T> {
	parentId(node: T): string | null | undefined;
	choiceGroup(node: T): string | null | undefined;
}

// ============================================================================

/**
 * Groups a flat list of nodes into `parentId -> choiceGroup -> members[]`
 * buckets. Shared by adapters whose source data is a flat list rather than
 * an already-nested tree.
 */
export function groupByParent<T>(nodes: T[], accessors: FlatNodeAccessors<T>) {
	const map = new Map<string, Map<string | null, T[]>>();
	for (const node of nodes) {
		const parentId = accessors.parentId(node);
		if (!parentId) continue;
		if (!map.has(parentId)) map.set(parentId, new Map());
		const groups = map.get(parentId)!;
		const key = accessors.choiceGroup(node) ?? null;
		if (!groups.has(key)) groups.set(key, []);
		groups.get(key)!.push(node);
	}
	return map;
}
