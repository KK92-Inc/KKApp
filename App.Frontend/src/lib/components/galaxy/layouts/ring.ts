// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Ring layout: "same depth, same ring".
//
// Deterministic, but scattered on purpose. Position is derived from a hash of
// each goal's id, so the layout looks organic yet is identical on every load,
// and adding a goal only reshuffles the ring it lands on.
// ============================================================================

import * as d3 from 'd3';
import config from '../config';
import { radiusOf } from '../geometry';
import type { SimNode } from '../types';
import type { Hierarchy, Placement } from './index';

// ============================================================================

const TAU = 2 * Math.PI;

export function ring<TMeta>({ nodes }: Hierarchy<TMeta>): Placement<TMeta> {
	const radii = placeOnRings(nodes);
	return { guides: radii.slice(1), stop() {} };
}

// ============================================================================

// Compute x, y onto every node, returns the radius of each ring
function placeOnRings<TMeta>(nodes: SimNode<TMeta>[]): number[] {
	const byDepth = d3.group(nodes, (node) => node.depth);
	const maxDepth = d3.max(nodes, (node) => node.depth) ?? 0;

	const angles = new Map<SimNode<TMeta>, number>();
	const rings: { nodeRadius: number; angles: number[] }[] = [];

	for (let depth = 0; depth <= maxDepth; depth++) {
		const members = [...(byDepth.get(depth) ?? [])].sort((a, b) => rand(a.data.id) - rand(b.data.id));
		const slot = TAU / members.length;
		const rotation = rand(`ring:${depth}`) * TAU;
		const entry = { nodeRadius: 0, angles: [] as number[] };

		members.forEach((node, i) => {
			// Jitter stays inside the node's own slot, so neighbours are always
			// at least '(1 - jitter) * slot' apart.
			const nudge = (rand(`nudge:${node.data.id}`) - 0.5) * config.ring.jitter;
			const angle = rotation + (i + 0.5 + nudge) * slot;
			angles.set(node, angle);
			entry.angles.push(angle);
			entry.nodeRadius = Math.max(entry.nodeRadius, radiusOf(node));
		});
		rings.push(entry);
	}

	const radii = [0];
	for (let depth = 1; depth <= maxDepth; depth++) {
		const inner = rings[depth - 1];
		const outer = rings[depth];
		// Two circles are never closer than the difference of their radii, so
		// this alone keeps neighbouring rings from touching.
		const clearance = radii[depth - 1] + inner.nodeRadius + outer.nodeRadius + config.ring.ringGap;
		radii.push(Math.max(clearance, radiusToFit(outer.angles, outer.nodeRadius)));
	}

	for (const node of nodes) {
		const radius = radii[node.depth];
		const theta = (angles.get(node) ?? 0) - Math.PI / 2; // 0 => up
		node.x = radius * Math.cos(theta);
		node.y = radius * Math.sin(theta);
	}

	return radii;
}

// Smallest radius at which the closest two of these nodes
function radiusToFit(angles: number[], radius: number): number {
	if (angles.length < 2) return 0;

	const sorted = [...angles].map((a) => ((a % TAU) + TAU) % TAU).sort((a, b) => a - b);
	let gap = sorted[0] + TAU - sorted[sorted.length - 1]; // across the seam
	for (let i = 1; i < sorted.length; i++) gap = Math.min(gap, sorted[i] - sorted[i - 1]);

	// chord = 2 * r * sin(gap / 2)  =>  r = chord / (2 * sin(gap / 2))
	// With 2+ nodes the smallest gap is at most PI, so sin() is monotonic here.
	const chord = 2 * radius + config.ring.nodeGap;
	return chord / (2 * Math.sin(Math.max(gap, 1e-3) / 2));
}

// FNV-1a (my favourite) + avalanche step
function rand(key: string): number {
	let h = 2166136261;
	for (let i = 0; i < key.length; i++) {
		h ^= key.charCodeAt(i);
		h = Math.imul(h, 16777619);
	}
	h ^= h >>> 15;
	h = Math.imul(h, 2246822507);
	h ^= h >>> 13;
	h = Math.imul(h, 3266489909);
	h ^= h >>> 16;
	return (h >>> 0) / 2 ** 32;
}
