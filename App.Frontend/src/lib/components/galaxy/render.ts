// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Draws a `GalaxyGraph` into an <svg>.
//
//   const renderer = new GalaxyRenderer<MyNode>();
//   renderer.onSelect((node) => ...);
//   <svg {@attach renderer.attach(graph)}></svg>
//   renderer.focus(id);
//
// Knows nothing about goals or progress: placement comes from `./layouts`,
// looks come from `./theme`.
// ============================================================================

import * as d3 from 'd3';
import type { Attachment } from 'svelte/attachments';
import config from './config';
import { NODE_FONT, radiusOf, roleOf } from './geometry';
import { LAYOUTS, type Hierarchy, type Placement } from './layouts';
import { STATUS_STYLES } from './theme';
import type { GalaxyGraph, GalaxyNode, SimLink, SimNode } from './types';

// ============================================================================
// Public API
// ============================================================================

export class GalaxyRenderer<TMeta = unknown> {
	private selectHandler?: (meta: TMeta) => void;
	private session?: Session;

	/** Called with the domain object of a node when it is clicked (or activated with the keyboard). */
	onSelect(handler: (meta: TMeta) => void): void {
		this.selectHandler = handler;
	}

	/**
	 * For `<svg {@attach renderer.attach(graph)}>`. Svelte re-runs it when
	 * `graph` changes, which tears the old drawing down first.
	 */
	attach(graph: GalaxyGraph<TMeta>): Attachment<SVGElement> {
		return (element) => this.mount(element, graph);
	}

	/** Same as `attach`, for when you're not in a template. Returns the cleanup. */
	mount(element: SVGElement, graph: GalaxyGraph<TMeta>): () => void {
		this.session?.dispose();
		const session = createSession(element, graph, (meta) => this.selectHandler?.(meta));
		this.session = session;

		return () => {
			session.dispose();
			if (this.session === session) this.session = undefined;
		};
	}

	/** Flies the camera to a node and pulses it. `id` is `GalaxyNode.id`. */
	focus(id: string): void {
		if (!this.session?.focus(id)) {
			console.warn(`Galaxy: can't focus "${id}" (nothing is mounted, or the graph has no such node).`);
		}
	}
}

// ============================================================================
// Session: one graph drawn into one <svg>
// ============================================================================

interface Session {
	/** @returns whether the node exists */
	focus(id: string): boolean;
	/** Safe to call more than once. */
	dispose(): void;
}

type Layer = d3.Selection<SVGGElement, unknown, null, undefined>;
type NodeSelection<TMeta> = d3.Selection<SVGGElement, SimNode<TMeta>, SVGGElement, unknown>;

function createSession<TMeta>(
	element: SVGElement,
	graph: GalaxyGraph<TMeta>,
	onSelect: (meta: TMeta) => void
): Session {
	const svg = d3.select<SVGElement, unknown>(element);
	svg.selectAll('*').remove();
	svg.append('style').text(STYLES);

	// Back to front: guides, links, nodes.
	const canvas = svg.append('g');
	const guideLayer = canvas.append('g');
	const linkLayer = canvas.append('g');
	const nodeLayer = canvas.append('g');

	const hierarchy = toHierarchy(graph.root);
	const links = drawLinks(linkLayer, hierarchy.links);
	const nodes = drawNodes(nodeLayer, hierarchy.nodes, onSelect);

	const redraw = (): void => {
		const path = placement.path;
		if (path) links.attr('d', (link) => path(link));
		nodes.attr('transform', (node) => `translate(${node.x ?? 0},${node.y ?? 0})`);
	};

	const placement: Placement<TMeta> = LAYOUTS[graph.layout](hierarchy, redraw);
	if (!placement.path) linkLayer.remove();
	drawGuides(guideLayer, placement.guides);
	if (placement.drag) nodes.call(placement.drag);

	const zoom = d3
		.zoom<SVGElement, unknown>()
		.scaleExtent([config.zoom.min, config.zoom.max])
		.on('zoom', ({ transform }) => canvas.attr('transform', transform.toString()));
	svg.call(zoom);

	let width = 0;
	let height = 0;
	let fitted = false;

	/** Frames the whole graph (including ring guides), never magnified past 1:1. */
	const fit = (): void => {
		let x0 = Infinity;
		let y0 = Infinity;
		let x1 = -Infinity;
		let y1 = -Infinity;
		for (const node of hierarchy.nodes) {
			const r = radiusOf(node);
			x0 = Math.min(x0, (node.x ?? 0) - r);
			y0 = Math.min(y0, (node.y ?? 0) - r);
			x1 = Math.max(x1, (node.x ?? 0) + r);
			y1 = Math.max(y1, (node.y ?? 0) + r);
		}
		const outermost = placement.guides[placement.guides.length - 1] ?? 0;
		x0 = Math.min(x0, -outermost);
		y0 = Math.min(y0, -outermost);
		x1 = Math.max(x1, outermost);
		y1 = Math.max(y1, outermost);

		const pad = config.zoom.fitPadding * 2;
		const scale = Math.max(config.zoom.min, Math.min(1, (width - pad) / (x1 - x0), (height - pad) / (y1 - y0)));
		svg.call(zoom.transform, d3.zoomIdentity.scale(scale).translate(-(x0 + x1) / 2, -(y0 + y1) / 2));
	};

	// Sized by the element itself (CSS decides how big it is). Resizing only
	// re-centers the view: it never re-runs the layout.
	const resize = (): void => {
		const rect = element.getBoundingClientRect();
		if (rect.width === 0 || rect.height === 0) return; // hidden: wait for a real size
		width = rect.width;
		height = rect.height;
		svg.attr('viewBox', `${-width / 2} ${-height / 2} ${width} ${height}`);
		if (!fitted) {
			fitted = true;
			fit();
		}
	};

	const observer = new ResizeObserver(resize);
	observer.observe(element);

	redraw();
	resize();

	let disposed = false;
	return {
		focus(id) {
			const target = hierarchy.nodes.find((node) => node.data.id === id);
			if (!target) return false;

			const view = d3.zoomIdentity.scale(config.zoom.focusScale).translate(-(target.x ?? 0), -(target.y ?? 0));
			svg.transition().duration(config.zoom.focusDuration).call(zoom.transform, view);
			nodes.filter((node) => node === target).each(function () {
				pulse(this);
			});
			return true;
		},

		dispose() {
			if (disposed) return;
			disposed = true;
			observer.disconnect();
			placement.stop();
			svg.on('.zoom', null).interrupt();
			svg.selectAll('*').remove();
		}
	};
}

function toHierarchy<TMeta>(root: GalaxyNode<TMeta>): Hierarchy<TMeta> {
	const top = d3.hierarchy(root, (node) => node.children) as SimNode<TMeta>;

	return {
		root: top,
		nodes: top.descendants() as SimNode<TMeta>[],
		links: top.links() as unknown as SimLink<TMeta>[]
	};
}

// ============================================================================
// Drawing
// ============================================================================

/**
 * Interaction styling lives in CSS (hover, focus, pulse) rather than in d3
 * transitions; status styling (fill, dash, ...) is set per node from the theme.
 */
const STYLES = `
.galaxy-node .core { stroke: var(--border); stroke-width: 1.5; transition: stroke .12s, stroke-width .12s; }
.galaxy-node.is-goal { cursor: pointer; outline: none; }
.galaxy-node.is-goal:hover .core,
.galaxy-node.is-goal:focus-visible .core { stroke: var(--ring); stroke-width: 3; }
.galaxy-node.is-pulsing .core { animation: galaxy-pulse .45s ease-out; }
@keyframes galaxy-pulse { 35% { stroke: var(--ring); stroke-width: 6; } }
`;

function drawGuides(layer: Layer, radii: readonly number[]): void {
	layer
		.selectAll('circle')
		.data(radii)
		.join('circle')
		.attr('r', (radius) => radius)
		.attr('fill', 'none')
		.attr('stroke', config.guide.color)
		.attr('stroke-opacity', config.guide.opacity)
		.attr('stroke-width', config.guide.width);
}

function drawLinks<TMeta>(layer: Layer, data: SimLink<TMeta>[]) {
	return layer
		.attr('fill', 'none')
		.attr('stroke-opacity', 0.8)
		.attr('stroke-width', 1.5)
		.selectAll<SVGPathElement, SimLink<TMeta>>('path')
		.data(data)
		.join('path')
		.attr('stroke', (link) => STATUS_STYLES[link.target.data.status].link);
}

function drawNodes<TMeta>(layer: Layer, data: SimNode<TMeta>[], onSelect: (meta: TMeta) => void): NodeSelection<TMeta> {
	const isGoal = (node: SimNode<TMeta>): boolean => node.data.meta !== null;

	const nodes = layer
		.selectAll<SVGGElement, SimNode<TMeta>>('g')
		.data(data)
		.join('g')
		.attr('class', (node) => (isGoal(node) ? 'galaxy-node is-goal' : 'galaxy-node'))
		.attr('data-status', (node) => node.data.status);

	nodes.append('title').text((node) => describe(node.data));

	nodes
		.append('circle')
		.attr('class', 'core')
		.attr('r', (node) => radiusOf(node))
		.attr('fill', (node) => STATUS_STYLES[node.data.status].fill)
		.attr('stroke-dasharray', (node) => STATUS_STYLES[node.data.status].dash);

	nodes
		.filter(isGoal)
		.append('text')
		.attr('text-anchor', 'middle')
		.attr('dominant-baseline', 'central')
		.attr('pointer-events', 'none')
		.attr('fill', (node) => STATUS_STYLES[node.data.status].text)
		.attr('font-weight', (node) => (roleOf(node) === 'leaf' ? 'normal' : 'bold'))
		.each(function (node) {
			layoutLabel(this, node.data.label, radiusOf(node), NODE_FONT[roleOf(node)]);
		});

	const choose = (element: SVGGElement, node: SimNode<TMeta>): void => {
		pulse(element);
		if (node.data.meta !== null) onSelect(node.data.meta);
	};

	nodes
		.filter(isGoal)
		.attr('role', 'button')
		.attr('tabindex', 0)
		.attr('aria-label', (node) => describe(node.data))
		.on('click', function (_event, node) {
			choose(this, node);
		})
		.on('keydown', function (event: KeyboardEvent, node) {
			if (event.key !== 'Enter' && event.key !== ' ') return;
			event.preventDefault();
			choose(this, node);
		});

	return nodes;
}

/** Full label plus status, for tooltips and screen readers (the circle may have to truncate). */
function describe(node: GalaxyNode): string {
	return node.status === 'default' ? node.label : `${node.label} (${STATUS_STYLES[node.status].label})`;
}

/** Restarts the CSS pulse animation on a node. */
function pulse(element: Element): void {
	element.classList.remove('is-pulsing');
	element.classList.add('is-pulsing');
	element.addEventListener('animationend', () => {
		element.classList.remove('is-pulsing')
	}, { once: true });
}

// ============================================================================
// Labels
// ============================================================================

const MIN_FONT = 5;

/** Greedy word wrap into at most `maxLines`, ellipsizing what doesn't fit. */
function wrap(label: string, maxChars: number, maxLines = 3): string[] {
	const lines: string[] = [];
	let line = '';
	for (const word of label.split(/\s+/).filter(Boolean)) {
		const next = line ? `${line} ${word}` : word;
		if (!line || next.length <= maxChars) {
			line = next;
		} else {
			lines.push(line);
			line = word;
		}
	}
	if (line) lines.push(line);

	if (lines.length > maxLines) {
		lines.length = maxLines;
		lines[maxLines - 1] += '…';
	}
	return lines;
}

/** Wraps the label over a few centered lines, then shrinks the font until the block fits the circle. */
function layoutLabel(text: SVGTextElement, label: string, radius: number, fontSize: number): void {
	const box = radius * 1.7; // the largest square that sits comfortably inside the circle
	const lines = wrap(label, Math.max(4, Math.floor(box / (fontSize * 0.55))));

	d3.select(text)
		.selectAll('tspan')
		.data(lines)
		.join('tspan')
		.attr('x', 0)
		// First line is lifted so the whole block is centered; `em` keeps working when the font shrinks.
		.attr('dy', (_line, i) => (i === 0 ? `${-(lines.length - 1) * 0.55}em` : '1.1em'))
		.text((line) => line);

	let size = fontSize;
	text.setAttribute('font-size', `${size}px`);
	for (let i = 0; i < 20 && size > MIN_FONT; i++) {
		const { width, height } = text.getBBox();
		if (width <= box && height <= box) break;
		size = Math.max(MIN_FONT, size * 0.9);
		text.setAttribute('font-size', `${size.toFixed(1)}px`);
	}
}
