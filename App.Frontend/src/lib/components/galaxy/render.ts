// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
/* eslint-disable @typescript-eslint/no-this-alias */
// ============================================================================

import * as d3 from 'd3';
import config from './config';
import type { GalaxyNode, SimLink, SimNode } from './types';

// ============================================================================
// Sizing
// ============================================================================

const RADII = {
	root: { outer: 58, inner: 40 },
	parent: { outer: 38, inner: 26 },
	group: { outer: 50, inner: 26 },
	lone: { outer: 32, inner: 22 },
} as const;

const CHOICE_DOT = 10;
const CHOICE_ORBIT = 30;
type SizeKey = keyof typeof RADII;

function sizeKey<TMeta>(d: d3.HierarchyNode<GalaxyNode<TMeta>>): SizeKey {
	if (d.depth === 0) return 'root';
	if (d.children) return 'parent';
	if (d.data.items.length > 1) return 'group';
	return 'lone';
}

// ============================================================================
// GalaxyRenderer — imperative D3 engine, framework-agnostic AND data-agnostic
// ============================================================================

/**
 * Owns the D3 force-graph rendering pipeline for a single `<svg>` element.
 * Knows nothing about goals, cursi, or state — it only understands
 * `GalaxyNode`/`GalaxyItem`. Any domain data gets adapted into that shape
 * before it reaches here (see `./adapters`).
 */
export class GalaxyRenderer<TMeta = unknown> {
	private element?: SVGElement;
	private zoomBehavior?: d3.ZoomBehavior<SVGElement, unknown>;
	private simNodes: SimNode<TMeta>[] = [];
	private nodeG?: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>;
	private width = 0;
	private height = 0;

	private singleClickHandler?: (meta: TMeta) => void;
	private groupClickHandler?: (metas: TMeta[]) => void;

	public onSingleClick(callback: (meta: TMeta) => void) {
		this.singleClickHandler = callback;
	}

	public onGroupClick(callback: (metas: TMeta[]) => void) {
		this.groupClickHandler = callback;
	}

	/**
	 * Mounts the renderer onto an SVG element for the given tree.
	 * Returns a cleanup function (for use inside a Svelte {@attach}).
	 */
	mount(element: SVGElement, tree: GalaxyNode<TMeta>): () => void {
		const controller = new AbortController();
		const style = getComputedStyle(document.documentElement);
		const header = parseFloat(style.getPropertyValue('--header-height'));

		this.width = window.innerWidth;
		this.height = window.innerHeight - 1 - header;
		document.body.style.overflow = 'hidden';
		this.render(element, tree);

		window.addEventListener(
			'resize',
			() => {
				this.width = window.innerWidth;
				this.height = window.innerHeight;
				this.render(element, tree);
			},
			{ signal: controller.signal }
		);

		return () => {
			document.body.style.overflow = '';
			controller.abort();
		};
	}

	// =========================================================================
	// Render pipeline
	// =========================================================================

	private render(element: SVGElement, tree: GalaxyNode<TMeta>): void {
		this.element = element;
		const svg = d3.select<SVGElement, unknown>(element);
		svg.selectAll('*').remove();

		svg
			.attr('width', this.width)
			.attr('height', this.height)
			.attr('viewBox', `${-this.width / 2} ${-this.height / 2} ${this.width} ${this.height}`);

		const canvas = svg.append('g');

		this.zoomBehavior = d3.zoom<SVGElement, unknown>()
			.scaleExtent([0.05, 4])
			.on('zoom', ({ transform }) => canvas.attr('transform', transform.toString()));

		svg.call(this.zoomBehavior);

		const root = d3.hierarchy<GalaxyNode<TMeta>>(tree, (d) => d.children);
		this.simNodes = root.descendants() as unknown as SimNode<TMeta>[];
		const links = root.links() as unknown as SimLink<TMeta>[];

		const simulation = this.buildSimulation(this.simNodes, links);
		const linkSel = this.renderLinks(canvas, links);
		this.nodeG = this.renderNodes(canvas, this.simNodes, simulation);

		simulation.on('tick', () => {
			linkSel
				.attr('x1', (d) => (d.source as SimNode<TMeta>).x ?? 0)
				.attr('y1', (d) => (d.source as SimNode<TMeta>).y ?? 0)
				.attr('x2', (d) => (d.target as SimNode<TMeta>).x ?? 0)
				.attr('y2', (d) => (d.target as SimNode<TMeta>).y ?? 0);

			this.nodeG!.attr('transform', (d: SimNode<TMeta>) => `translate(${d.x ?? 0},${d.y ?? 0})`);
		});
	}

	private buildSimulation(nodes: SimNode<TMeta>[], links: SimLink<TMeta>[]) {
		return d3
			.forceSimulation<SimNode<TMeta>>(nodes)
			.force('link', d3.forceLink<SimNode<TMeta>, SimLink<TMeta>>(links).distance(config.link.distance).strength(config.link.strength).iterations(config.link.iterations))
			.force('charge', d3.forceManyBody<SimNode<TMeta>>().strength(config.charge.strength).distanceMax(config.charge.distanceMax))
			.force('center', d3.forceCenter<SimNode<TMeta>>(0, 0))
			.force('collide', d3.forceCollide<SimNode<TMeta>>((d) => RADII[sizeKey(d)].outer + config.collision.padding).strength(config.collision.strength))
			.alphaMin(config.simulation.alphaMin)
			.alphaDecay(config.simulation.alphaDecay)
			.velocityDecay(config.simulation.velocityDecay);
	}

	private renderLinks(canvas: d3.Selection<SVGGElement, unknown, null, undefined>, links: SimLink<TMeta>[]) {
		return canvas
			.append('g')
			.attr('fill', 'none')
			.attr('stroke', 'var(--border)')
			.attr('stroke-opacity', 0.8)
			.attr('stroke-width', 1.5)
			.selectAll<SVGLineElement, SimLink<TMeta>>('line')
			.data(links)
			.join('line');
	}

	private renderNodes(
		canvas: d3.Selection<SVGGElement, unknown, null, undefined>,
		nodes: SimNode<TMeta>[],
		simulation: d3.Simulation<SimNode<TMeta>, SimLink<TMeta>>
	) {
		const nodeG = canvas
			.append('g')
			.selectAll<SVGGElement, SimNode<TMeta>>('g')
			.data(nodes)
			.join('g')
			.attr('cursor', 'pointer')
			.call(this.buildDrag(simulation));

		this.appendRings(nodeG);
		this.appendCores(nodeG);
		this.appendChoiceDots(nodeG);
		this.appendLabels(nodeG);
		this.attachNodeClick(nodeG);

		return nodeG;
	}

	// =========================================================================
	// Node visual layers — everything here reads pre-resolved data only
	// =========================================================================

	private appendRings(sel: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>) {
		sel.append('circle')
			.attr('r', (d) => RADII[sizeKey(d)].outer)
			.attr('fill', 'none')
			.attr('stroke', (d) => (sizeKey(d) === 'group' ? 'var(--border)' : 'none'))
			.attr('stroke-width', (d) => (d.depth === 0 ? 2.5 : 1))
			.attr('stroke-opacity', 0.8);
	}

	private appendCores(sel: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>) {
		sel.append('circle')
			.attr('class', 'core')
			.attr('r', (d) => RADII[sizeKey(d)].inner)
			.attr('fill', (d) => d.data.color)
			.attr('stroke', 'var(--border)')
			.attr('stroke-width', 1.5)
			.on('mouseenter', function () {
				d3.select(this)
					.transition().duration(120)
					.attr('stroke', 'var(--ring)')
					.attr('stroke-width', 2.5);
			})
			.on('mouseleave', function () {
				d3.select(this)
					.transition().duration(150)
					.attr('stroke', 'var(--border)')
					.attr('stroke-width', 1.5);
			});
	}

	private appendChoiceDots(sel: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>) {
		const self = this;
		sel.each(function (d) {
			if (d.data.items.length <= 1) return;

			const g = d3.select<SVGGElement, SimNode<TMeta>>(this).append('g').attr('class', 'items');
			const n = d.data.items.length;

			d.data.items.forEach((item, i) => {
				const angle = (2 * Math.PI * i) / n - Math.PI / 2;
				const cx = CHOICE_ORBIT * Math.cos(angle);
				const cy = CHOICE_ORBIT * Math.sin(angle);

				g.append('circle')
					.attr('cx', cx).attr('cy', cy)
					.attr('r', CHOICE_DOT)
					.attr('fill', item.color)
					.attr('stroke', 'var(--border)')
					.attr('stroke-width', 1)
					.attr('cursor', 'pointer')
					.on('mouseenter', function () {
						d3.select(this)
							.transition().duration(120)
							.attr('r', CHOICE_DOT * 1.15)
							.attr('stroke', 'var(--ring)')
							.attr('stroke-width', 1.5);
					})
					.on('mouseleave', function () {
						d3.select(this)
							.transition().duration(150)
							.attr('r', CHOICE_DOT)
							.attr('stroke', 'var(--border)')
							.attr('stroke-width', 1);
					})
					.on('click', function (event) {
						event.stopPropagation();
						self.pulse(d3.select<SVGCircleElement, unknown>(this), 1);
						self.singleClickHandler?.(item.meta);
					});

				g.append('text')
					.attr('x', cx).attr('y', cy)
					.attr('text-anchor', 'middle')
					.attr('dominant-baseline', 'central')
					.attr('fill', item.textColor)
					.attr('font-size', '8px')
					.attr('font-weight', 'bold')
					.attr('pointer-events', 'none')
					.text(i + 1);
			});
		});
	}

	private appendLabels(sel: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>) {
		const self = this;

		sel.append('text')
			.attr('text-anchor', 'middle')
			.attr('font-weight', (d) => (d.depth === 0 || d.children ? 'bold' : 'normal'))
			.attr('pointer-events', 'none')
			.attr('fill', (d) => d.data.textColor)
			.each(function (d) {
				const el = this as SVGTextElement;
				const lines = d.data.label;
				const inner = RADII[sizeKey(d)].inner;
				const base = d.depth === 0 ? 13 : 10;

				el.setAttribute('font-size', `${base}px`);
				el.setAttribute('dominant-baseline', 'central');

				if (lines.length === 1) {
					el.textContent = lines[0];
				} else {
					el.textContent = "Multiple";
				}

				self.fitTextToCircle(el, inner);
			});
	}

	private fitTextToCircle(el: SVGTextElement, innerRadius: number): void {
		const max = innerRadius * 1.75;
		let size = parseFloat(el.getAttribute('font-size') ?? '10');

		try {
			for (let i = 0; i < 20 && size > 5; i++) {
				const { width, height } = el.getBBox();
				if (width <= max && height <= max) break;
				size = Math.max(5, size * 0.88);
				el.setAttribute('font-size', `${size.toFixed(1)}px`);
			}
		} catch {
			// getBBox unavailable
		}
	}

	// =========================================================================
	// Interactivity / Focus
	// =========================================================================

	/**
	 * Flashes a circle's stroke to `var(--ring)` then fades back to the
	 * border color/width it should rest at. Used for both node cores and
	 * choice dots — pass the resting stroke width for whichever you're
	 * pulsing (1.5 for cores, 1 for dots).
	 */
	private pulse(circle: d3.Selection<SVGCircleElement, any, any, any>, restStrokeWidth = 1.5) {
		circle
			.transition().duration(80)
			.attr('stroke', 'var(--ring)')
			.attr('stroke-width', restStrokeWidth + 1.5)
			.transition().duration(350)
			.attr('stroke', 'var(--border)')
			.attr('stroke-width', restStrokeWidth);
	}

	private attachNodeClick(sel: d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>) {
		const self = this;
		sel.on('click', function (event: PointerEvent, d: SimNode<TMeta>) {
			self.pulse(d3.select<SVGGElement, SimNode<TMeta>>(this).select<SVGCircleElement>('circle.core'));

			const items = d.data.items;
			if (items.length > 1) {
				self.groupClickHandler?.(items.map((i) => i.meta));
			} else if (items.length === 1) {
				self.singleClickHandler?.(items[0].meta);
			}
		});
	}

	/** Zooms to and pulses the node containing the item with the given id. */
	focus(itemId: string): void {
		if (!this.element || !this.zoomBehavior || !this.simNodes.length || !this.nodeG) {
			console.warn('Galaxy is not fully rendered yet.');
			return;
		}

		const targetNode = this.simNodes.find((n) => n.data.items.some((i) => i.id === itemId));
		if (!targetNode) {
			console.warn(`Item "${itemId}" not found in the current tree.`);
			return;
		}

		const svg = d3.select(this.element);
		const scale = 1.8;
		const transform = d3.zoomIdentity.scale(scale).translate(-(targetNode.x ?? 0), -(targetNode.y ?? 0));
		svg.transition().duration(750).call(this.zoomBehavior.transform, transform);

		const nodeElement = this.nodeG.filter((d) => d === targetNode).node();
		if (nodeElement) {
			this.pulse(d3.select<SVGGElement, SimNode<TMeta>>(nodeElement).select<SVGCircleElement>('circle.core'));
		}
	}

	private buildDrag(simulation: d3.Simulation<SimNode<TMeta>, SimLink<TMeta>>) {
		return d3.drag<SVGGElement, SimNode<TMeta>>()
			.on('start', (event, d) => {
				if (!event.active) simulation.alphaTarget(config.drag.startAlphaTarget).restart();
				d.fx = d.x;
				d.fy = d.y;
			})
			.on('drag', (event, d) => {
				d.fx = event.x;
				d.fy = event.y;
			})
			.on('end', (event, d) => {
				if (!event.active) simulation.alphaTarget(config.drag.endAlphaTarget);
				d.fx = null;
				d.fy = null;
			});
	}
}
