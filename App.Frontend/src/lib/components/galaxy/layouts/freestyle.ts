// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Force-directed tree, after https://observablehq.com/@d3/force-directed-tree
// ============================================================================

import * as d3 from 'd3';
import config from '../config';
import { radiusOf } from '../geometry';
import type { SimLink, SimNode } from '../types';
import type { Hierarchy, Placement } from './index';

const { link, charge, collision, simulation: tuning, drag: dragTuning } = config.freestyle;

export function freestyle<TMeta>({ nodes, links }: Hierarchy<TMeta>, onMove: () => void): Placement<TMeta> {
	const simulation = d3
		.forceSimulation<SimNode<TMeta>>(nodes)
		.force(
			'link',
			d3.forceLink<SimNode<TMeta>, SimLink<TMeta>>(links)
				.distance(link.distance)
				.strength(link.strength)
				.iterations(link.iterations)
		)
		.force('charge', d3.forceManyBody<SimNode<TMeta>>().strength(charge.strength).distanceMax(charge.distanceMax))
		.force('collide', d3.forceCollide<SimNode<TMeta>>((n) => radiusOf(n) + collision.padding).strength(collision.strength))
		.force('center', d3.forceCenter(0, 0))
		.alphaMin(tuning.alphaMin)
		.alphaDecay(tuning.alphaDecay)
		.velocityDecay(tuning.velocityDecay)
		.stop();

	// Run the simulation to rest right now instead of animating it from a
	// blob in the middle: the first paint is the finished layout (and the
	// renderer can fit the camera to it). It only wakes up again when a node
	// is dragged. This is exactly how many ticks d3 would have run on its own.
	simulation.tick(Math.ceil(Math.log(simulation.alphaMin()) / Math.log(1 - simulation.alphaDecay())));
	simulation.on('tick', onMove);

	const drag = d3
		.drag<SVGGElement, SimNode<TMeta>>()
		.on('start', (event, node) => {
			if (!event.active) simulation.alphaTarget(dragTuning.alphaTarget).restart();
			node.fx = node.x;
			node.fy = node.y;
		})
		.on('drag', (event, node) => {
			node.fx = event.x;
			node.fy = event.y;
		})
		.on('end', (event, node) => {
			if (!event.active) simulation.alphaTarget(0);
			node.fx = null;
			node.fy = null;
		});

	return {
		guides: [],
		path: ({ source, target }) => `M${source.x ?? 0},${source.y ?? 0}L${target.x ?? 0},${target.y ?? 0}`,
		drag,
		stop: () => void simulation.stop()
	};
}
