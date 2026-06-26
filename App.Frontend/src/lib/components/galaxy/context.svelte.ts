// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as d3 from 'd3';
import { createContext } from 'svelte';
import type { Attachment } from 'svelte/attachments';
import { transformApiToD3Hierarchy, type D3NodeDatum, type UserCursusTrackDO } from './index';

// ============================================================================

export default class Galaxy {
	public svg = $state<SVGElement>();
	public width: number = $state(800);
	public height: number = $state(600);
	public tree = $state<D3NodeDatum>();

	constructor(data: UserCursusTrackDO) {
		// Transform the flat API payload on instantiation
		this.tree = transformApiToD3Hierarchy(data);
	}

	public attachment(): Attachment<SVGElement> {
		const controller = new AbortController();

		return (element) => {
			this.svg = element;
			this.width = element.parentElement?.clientWidth || window.innerWidth;
			this.height = element.parentElement?.clientHeight || window.innerHeight;

			this.render();

			const handleResize = () => {
				this.width = this.svg?.parentElement?.clientWidth || window.innerWidth;
				this.height = this.svg?.parentElement?.clientHeight || window.innerHeight;
				this.svg?.setAttribute('viewBox', `0 0 ${this.width} ${this.height}`);
			};

			window.addEventListener('resize', handleResize, { signal: controller.signal });

			return () => {
				controller.abort();
			};
		};
	}

	private render() {
		if (!this.svg || !this.tree) return;
		const svg = d3.select(this.svg);
		svg.selectAll('*').remove();

		// Set dimensions
		svg.attr('width', '100%')
			.attr('height', '100%')
			.attr('viewBox', `0 0 ${this.width} ${this.height}`);

		// 1. Setup Pan and Zoom
		const mainG = svg.append('g');
		const zoom = d3.zoom<SVGElement, unknown>()
			.scaleExtent([0.1, 4])
			.on('zoom', (event) => mainG.attr('transform', event.transform));

		svg.call(zoom);
		svg.call(zoom.transform, d3.zoomIdentity.translate(this.width / 2, this.height / 2).scale(0.85));

		// 2. Setup Hierarchy & Forces
		const hierarchy = d3.hierarchy(this.tree);
		const links = hierarchy.links();
		const nodes = hierarchy.descendants();

		const simulation = d3.forceSimulation(nodes as d3.SimulationNodeDatum[])
			.force('link', d3.forceLink(links).distance(150).strength(1))
			.force('charge', d3.forceManyBody().strength(-900))
			.force('collide', d3.forceCollide().radius(70).iterations(2))
			.force('center', d3.forceCenter(0, 0));

		// Helper function to dynamically theme elements based on granular progression states
		const getStateTheme = (goal: any) => {
			if (!goal || !goal.isUnlocked || goal.state === 'Inactive') {
				return { bg: '#27272a', stroke: '#454545', text: '#71717a', accent: '#52525b' }; // Locked / Grey
			}
			switch (goal.state) {
				case 'Completed':
					return { bg: '#22c55e', stroke: '#16a34a', text: '#ffffff', accent: '#4ade80' }; // Emerald Green
				case 'Active':
					return { bg: '#3b82f6', stroke: '#2563eb', text: '#ffffff', accent: '#60a5fa' }; // Electric Blue
				case 'Awaiting':
				case null:
				case undefined:
					return { bg: '#f59e0b', stroke: '#d97706', text: '#1e293b', accent: '#fbbf24' }; // Amber Gold (Subscribable)
				default:
					return { bg: '#27272a', stroke: '#3f3f46', text: '#a1a1aa', accent: '#52525b' };
			}
		};

		// 3. Draw Links (Edges)
		const link = mainG.append('g')
			.attr('stroke', '#3f3f46')
			.attr('stroke-width', 2)
			.selectAll('line')
			.data(links)
			.join('line');

		// 4. Draw Nodes (Vertices)
		const node = mainG.append('g')
			.selectAll('g')
			.data(nodes)
			.join('g')
			.call(this.drag(simulation));

		// -- Background Outer Ring (Choice Groups only; Forced off for Root) --
		node.append('circle')
			.filter((d: any) => d.data.type !== 'root' && d.data.goals.length > 1)
			.attr('r', 48)
			.attr('fill', '#1e1e24')
			.attr('stroke', '#3f3f46')
			.attr('stroke-width', 2);

		// -- Center Core Circle --
		node.append('circle')
			.attr('r', (d: any) => d.data.type === 'root' ? 55 : 35)
			.attr('fill', (d: any) => {
				// Rule: Root or Singletons adopt their explicit state color directly
				if (d.data.type === 'root' || d.data.goals.length === 1) {
					return getStateTheme(d.data.goals[0]).bg;
				}
				// Choice groups maintain a neutral hollow core layout
				return '#111115';
			})
			.attr('stroke', (d: any) => {
				if (d.data.type === 'root') return '#ffffff'; // Make root completely distinct
				if (d.data.goals.length === 1) return getStateTheme(d.data.goals[0]).stroke;
				return '#52525b';
			})
			.attr('stroke-width', (d: any) => d.data.type === 'root' ? 4 : 2)
			.style('box-shadow', '0 4px 6px -1px rgb(0 0 0 / 0.5)');

		// -- Goal Sub-Circles (Choice Groups only; Handled outward on the perimeter) --
		node.each(function (d: any) {
			// Skip completely for Singletons or the designated Root node
			if (d.data.type === 'root' || d.data.goals.length <= 1) return;

			const group = d3.select(this);
			const goals = d.data.goals;
			const radius = 48; // Snaps exactly to the outer ring geometry
			const subCircleRadius = 11;

			goals.forEach((goal: any, i: number) => {
				const angle = (2 * Math.PI * i) / goals.length - (Math.PI / 2);
				const cx = Math.cos(angle) * radius;
				const cy = Math.sin(angle) * radius;

				const theme = getStateTheme(goal);

				group.append('circle')
					.attr('cx', cx)
					.attr('cy', cy)
					.attr('r', subCircleRadius)
					.attr('fill', theme.bg)
					.attr('stroke', '#111115')
					.attr('stroke-width', 2);

				group.append('text')
					.attr('x', cx)
					.attr('y', cy)
					.attr('text-anchor', 'middle')
					.attr('dominant-baseline', 'central')
					.attr('fill', theme.text)
					.attr('font-size', '10px')
					.attr('font-weight', 'bold')
					.text(i + 1);
			});
		});

		// -- Main Internal Node Text Label --
		node.append('text')
			.attr('text-anchor', 'middle')
			.attr('dominant-baseline', 'central')
			.attr('fill', (d: any) => {
				if (d.data.type === 'root' || d.data.goals.length === 1) {
					return getStateTheme(d.data.goals[0]).text;
				}
				return '#ffffff';
			})
			.attr('font-size', (d: any) => d.data.type === 'root' ? '14px' : '11px')
			.attr('font-weight', 'bold')
			.text((d: any) => d.data.name.length > 12 ? d.data.name.substring(0, 11) + '…' : d.data.name);

		// 5. Dynamic edge and node repositioning on clock tick
		simulation.on('tick', () => {
			link.attr('x1', (d: any) => d.source.x)
				.attr('y1', (d: any) => d.source.y)
				.attr('x2', (d: any) => d.target.x)
				.attr('y2', (d: any) => d.target.y);

			node.attr('transform', (d: any) => `translate(${d.x},${d.y})`);
		});
	}

	private drag(simulation: d3.Simulation<any, any>) {
		return d3.drag<SVGGElement, any>()
			.on('start', (event, d) => {
				if (!event.active) simulation.alphaTarget(0.3).restart();
				d.fx = d.x;
				d.fy = d.y;
			})
			.on('drag', (event, d) => {
				d.fx = event.x;
				d.fy = event.y;
			})
			.on('end', (event, d) => {
				if (!event.active) simulation.alphaTarget(0);
				d.fx = null;
				d.fy = null;
			});
	}
}

export const [get, init] = createContext<Galaxy>();
