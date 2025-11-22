// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as d3 from 'd3';
import { createContext } from 'svelte';
import type { components } from '$lib/api/api';
import type { Attachment } from 'svelte/attachments';
import config from "./config.json" with { type: 'json' };
import data from './data.json' with { type: 'json' };

// ============================================================================

/**
 * Root: The top-level node of the hierarchy.
 * Node: An internal node with children and some goals.
 * Leafe: A leaf node without children, navigates to more children.
 */
type NodeType = 'root' | 'node' | 'leaf';
type NodeState = components['schemas']['TaskState'];
interface NodeDatum extends d3.SimulationNodeDatum {
	name: string; // Name based on the goals
	goals: components['schemas']['TrackGoalDTO'][];
	children?: NodeDatum[];
	type: NodeType;
	state: NodeState;
}

// ============================================================================

export default class Galaxy {
	public svg = $state<SVGElement>();
	public width: number = $state(800);
	public height: number = $state(600);
	public tree = $state<NodeDatum>(data);

	/** Construct a new galaxy state */
	constructor(kek: components['schemas']['CursusTrackDTO']) {
		// TODO: Build the tree
		this.tree = data;
	}

	/**
	 * Attachment to be used on an SVG element.
	 * @example
	 * <svg {@attach ctx.attachment()}></svg>
	 * @returns The attachment function.
	 */
	public attachment(): Attachment<SVGElement> {
		const controller = new AbortController();

		return (element) => {
			this.svg = element;
			this.width = window.innerWidth;
			this.height = window.innerHeight;
			document.body.style.overflow = 'hidden';

			this.render();
			window.addEventListener('keydown', this.onKey, {
				signal: controller.signal
			});

			window.addEventListener('resize', this.onResize, {
				signal: controller.signal
			});

			return () => {
				document.body.style.overflow = '';
				controller.abort();
			};
		};
	}

	// ==========================================================================

	private onKey(event: KeyboardEvent) {
		// Handle key events
	}

	private onResize(event: UIEvent) {
		this.width = window.innerWidth;
		this.height = window.innerHeight;
		this.render();
	}

	// ==========================================================================

	private render() {
		if (!this.svg) return;
		const svg = d3.select(this.svg);
		svg.selectAll('*').remove();

		const drag = (simulation: d3.Simulation<d3.HierarchyNode<NodeDatum>, undefined>) => {
			function dragstarted(event, d) {
				if (!event.active) simulation.alphaTarget(0.3).restart();
				d.fx = d.x;
				d.fy = d.y;
			}

			function dragged(event, d) {
				d.fx = event.x;
				d.fy = event.y;
			}

			function dragended(event, d) {
				if (!event.active) simulation.alphaTarget(0);
				d.fx = null;
				d.fy = null;
			}

			return d3.drag().on('start', dragstarted).on('drag', dragged).on('end', dragended);
		};

		svg
			.attr('width', this.width)
			.attr('height', this.height)
			.attr('viewBox', [-this.width / 2, -this.height / 2, this.width, this.height])
			.attr('style', 'max-width: 100%; height: auto;');

		const hierarchy = d3.hierarchy(this.tree, (d) => d.children);
		const links = hierarchy.links();
		const nodes = hierarchy.descendants();
		const simulation = d3
			.forceSimulation(nodes)
			.force(
				'link',
				d3
					.forceLink(links)
					.distance(config.link.distance)
					.strength(config.link.strength)
					.iterations(config.link.iterations)
			)
			.force('charge', d3.forceManyBody().strength(-1000).distanceMax(1000)) // Stronger repulsion for better spacing
			.force('center', d3.forceCenter(0, 0));

		const mainG = svg.append('g');

		const link = mainG
			.append('g')
			.attr('stroke', '#999')
			.attr('stroke-opacity', 0.6)
			.selectAll('line')
			.data(links)
			.join('line');

		// Create the node elements
		const node = mainG
			.selectAll('.node')
			.data(nodes)
			.enter()
			.append('g')
			.attr('class', 'node')
			//.on("click", (event, d) => handleNodeClick(event, d)) // You can add your click handler here
			.call(drag(simulation));

		// Add background circle (group boundary)
		node
			.append('circle')
			.attr('class', 'node-boundary')
			.attr('r', (d) => {
				if (d.data.type === 'node') return 30; // Larger route nodes
				if (d.data.type === 'root') return 60; // Much larger root node
				return 45; // Larger regular nodes
			})
			.attr('stroke', (d) => (d.data.type === 'root' ? 'var(--primary)' : '#ddd'))
			.attr('stroke-width', (d) => (d.data.type === 'root' ? 3 : 1))
			.attr('stroke-dasharray', (d) => (d.data.type === 'node' ? '3,3' : 'none'))
			.attr('opacity', 0.7);

		// Add main central node circle
		node
			.append('circle')
			.attr('class', 'node-center')
			.attr('r', (d) => {
				if (d.data.type === 'node') return 25; // Larger route nodes
				if (d.data.type === 'root') return 40; // Much larger root node
				return 30; // Larger regular nodes
			})
			.attr('fill', (d) => {
				if (d.data.type === 'node') return 'var(--primary)';
				if (d.data.type === 'root') return 'var(--primary)';
				return d.data.state === 'Active' ? '#4CAF50' : '#ccc'; // Assuming 'Active' state maps to state 1
			})
			.attr('stroke', 'var(--border)')
			.attr('stroke-width', 1.5);

		// Add individual goal circles around main node
		node.each(function (d) {
			const isRoot = d.data.type === 'root';
			const isRouteNode = d.data.type === 'node';
			if (isRouteNode || !d.data.goals.length) return; // Skip route nodes or nodes with no goals

			const numGoals = d.data.goals.length;
			if (numGoals <= 1) return; // Skip nodes with only one goal

			// Larger goal circles and positioning
			const goalRadius = isRoot ? 15 : 12;
			const circleRadius = isRoot ? 45 : 35; // Placement circle radius

			const goalGroup = d3
				.select(this)
				.append('g')
				.attr('class', 'goal-group')
				.attr('pointer-events', 'none'); // Let events pass through to parent

			// Draw individual goals as circles around the node
			d.data.goals.forEach((goal, i) => {
				const angle = (2 * Math.PI * i) / numGoals;
				const x = circleRadius * Math.cos(angle);
				const y = circleRadius * Math.sin(angle);

				// Goal circle
				goalGroup
					.append('circle')
					.attr('cx', x)
					.attr('cy', y)
					.attr('r', goalRadius)
					.attr('fill', goal.state === 'Active' ? '#4CAF50' : '#ccc')
					.attr('stroke', 'var(--muted)')
					.attr('stroke-width', 1.5);

				// Optional: Goal number/index inside circles
				goalGroup
					.append('text')
					.attr('x', x)
					.attr('y', y)
					.attr('text-anchor', 'middle')
					.attr('stroke', 'var(--text-foreground)')
					.attr('dominant-baseline', 'central')
					.attr('font-size', isRoot ? '10px' : '9px')
					.attr('font-weight', 'bold')
					.text(i + 1);
			});
		});

		// Add text labels to nodes (centered inside now)
		node
			.append('text')
			.attr('text-anchor', 'middle')
			.attr('dominant-baseline', 'central') // Center vertically
			.text((d) => {
				const isRoot = d.data.type === 'root';
				const isRouteNode = d.data.type === 'node';
				// Truncate text if too long based on node size
				const maxLength = isRoot ? 12 : isRouteNode ? 8 : 10;
				return d.data.name.length > maxLength
					? d.data.name.substring(0, maxLength) + '...'
					: d.data.name;
			})
			.attr('font-size', (d) =>
				d.data.type === 'root' ? '14px' : d.data.type === 'node' ? '12px' : '12px'
			)
			.attr('font-weight', (d) => (d.data.type === 'root' ? 'bold' : 'normal'))
			.attr('fill', (d) => (d.data.type === 'node' || d.data.type === 'root' ? '#fff' : '#333')); // Light text on dark backgrounds

		simulation.on('tick', () => {
			node.attr('transform', (d) => `translate(${d.x}, ${d.y})`);
			link
				.attr('x1', (d) => d.source.x ?? 0)
				.attr('y1', (d) => d.source.y ?? 0)
				.attr('x2', (d) => d.target.x ?? 0)
				.attr('y2', (d) => d.target.y ?? 0);
		});
	}
}

// ============================================================================

export const [get, init] = createContext<Galaxy>();

// ============================================================================
