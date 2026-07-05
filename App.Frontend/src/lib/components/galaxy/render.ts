import * as d3 from 'd3';
import config from './config';
import type { NodeDatum, SimLink, SimNode, GoalState } from './types';

// ============================================================================
// Sizing
// ============================================================================

const RADII = {
	root: { outer: 58, inner: 40 },
	parent: { outer: 38, inner: 26 },
	group: { outer: 50, inner: 26 }, // orbit + dot = 44 ≤ outer = 50
	lone: { outer: 32, inner: 22 },
} as const;

const CHOICE_DOT = 10;
const CHOICE_ORBIT = 30;
type SizeKey = keyof typeof RADII;

function sizeKey(d: d3.HierarchyNode<NodeDatum>): SizeKey {
	if (d.data.type === 'root') return 'root';
	if (d.children) return 'parent';
	if (d.data.choices.length > 1) return 'group';
	return 'lone';
}

// ============================================================================
// Colours & labels
// ============================================================================

/** White on coloured fills; muted theme text on empty (locked) fills. */
function stateTextColor(state: GoalState, isUnlocked: boolean): string {
	return state === null && !isUnlocked ? 'var(--muted-foreground)' : '#fff';
}

/** Highest-priority state across a choice group's members. */
function groupState(choices: NodeDatum['choices']): { state: GoalState; isUnlocked: boolean } {
	const order: Partial<Record<string, number>> = { Completed: 4, Active: 3, Awaiting: 2, Inactive: 1 };
	const best = choices.reduce((a, b) =>
		(order[b.state ?? ''] ?? 0) > (order[a.state ?? ''] ?? 0) ? b : a
	);
	return { state: best.state, isUnlocked: choices.some((c) => c.isUnlocked) };
}

/**
 * Lines to display inside a node:
 * - Standalone goal            → its name
 * - Choice group, one chosen   → that choice's name only
 * - Choice group, none chosen  → all choice names (multi-line)
 */
function getLabel(d: NodeDatum): string[] {
	if (d.type === 'root' || d.choices.length <= 1) {
		return [d.choices[0]?.name ?? d.name];
	}
	const chosen = d.choices.find((c) => c.state !== null);
	return chosen ? [chosen.name] : d.choices.map((c) => c.name);
}

// ============================================================================
// GalaxyRenderer — imperative D3 engine, framework-agnostic
// ============================================================================

/**
 * Owns the D3 force-graph rendering pipeline for a single `<svg>` element.
 * Has no Svelte dependency: it's plain state + DOM manipulation, so it's
 * usable and testable independently of any component.
 */
export class GalaxyRenderer {
	private element?: SVGElement;
	private zoomBehavior?: d3.ZoomBehavior<SVGElement, unknown>;
	private simNodes: SimNode[] = [];
	private nodeG?: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>;
	private width = 0;
	private height = 0;

	/**
	 * Mounts the renderer onto an SVG element for the given tree.
	 * Returns a cleanup function (for use inside a Svelte {@attach}).
	 */
	mount(element: SVGElement, tree: NodeDatum): () => void {
		const controller = new AbortController();
		const style = getComputedStyle(document.documentElement);
		const header = parseFloat(style.getPropertyValue('--header-height'));

		this.width = window.innerWidth;
		this.height = window.innerHeight - 1 - header; // +1 for border
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

	private singleClickHandler?: (goal: any) => void;
	private groupClickHandler?: (goals: any[]) => void;

	public onSingleClick(callback: (goal: any) => void) {
		this.singleClickHandler = callback;
	}

	public onGroupClick(callback: (goals: any[]) => void) {
		this.groupClickHandler = callback;
	}

	// =========================================================================
	// Render pipeline
	// =========================================================================

	private render(element: SVGElement, tree: NodeDatum): void {
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

		const root = d3.hierarchy<NodeDatum>(tree, (d) => d.children);
		this.simNodes = root.descendants() as unknown as SimNode[];
		const links = root.links() as unknown as SimLink[];

		const simulation = this.buildSimulation(this.simNodes, links);
		const linkSel = this.renderLinks(canvas, links);
		this.nodeG = this.renderNodes(canvas, this.simNodes, simulation);

		simulation.on('tick', () => {
			linkSel
				.attr('x1', (d) => (d.source as SimNode).x ?? 0)
				.attr('y1', (d) => (d.source as SimNode).y ?? 0)
				.attr('x2', (d) => (d.target as SimNode).x ?? 0)
				.attr('y2', (d) => (d.target as SimNode).y ?? 0);

			this.nodeG!.attr('transform', (d: SimNode) => `translate(${d.x ?? 0},${d.y ?? 0})`);
		});
	}

	private buildSimulation(nodes: SimNode[], links: SimLink[]) {
		return d3
			.forceSimulation<SimNode>(nodes)
			.force('link', d3.forceLink<SimNode, SimLink>(links).distance(config.link.distance).strength(config.link.strength).iterations(config.link.iterations))
			.force('charge', d3.forceManyBody<SimNode>().strength(config.charge.strength).distanceMax(config.charge.distanceMax))
			.force('center', d3.forceCenter<SimNode>(0, 0))
			.force('collide', d3.forceCollide<SimNode>((d) => RADII[sizeKey(d)].outer + config.collision.padding).strength(config.collision.strength))
			.alphaMin(config.simulation.alphaMin)
			.alphaDecay(config.simulation.alphaDecay)
			.velocityDecay(config.simulation.velocityDecay);
	}

	private renderLinks(canvas: d3.Selection<SVGGElement, unknown, null, undefined>, links: SimLink[]) {
		return canvas
			.append('g')
			.attr('fill', 'none')
			.attr('stroke', 'var(--border)')
			.attr('stroke-opacity', 0.8)
			.attr('stroke-width', 1.5)
			.selectAll<SVGLineElement, SimLink>('line')
			.data(links)
			.join('line');
	}

	private renderNodes(
		canvas: d3.Selection<SVGGElement, unknown, null, undefined>,
		nodes: SimNode[],
		simulation: d3.Simulation<SimNode, SimLink>
	) {
		const nodeG = canvas
			.append('g')
			.selectAll<SVGGElement, SimNode>('g')
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
	// Node visual layers
	// =========================================================================

	private appendRings(sel: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>) {
		sel.append('circle')
			.attr('r', (d) => RADII[sizeKey(d)].outer)
			.attr('fill', 'none')
			.attr('stroke', (d) => (sizeKey(d) === 'group' ? 'var(--border)' : 'none'))
			.attr('stroke-width', (d) => (d.data.type === 'root' ? 2.5 : 1))
			.attr('stroke-opacity', 0.8);
	}

	private appendCores(sel: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>) {
		sel.append('circle')
			.attr('class', 'core')
			.attr('r', (d) => RADII[sizeKey(d)].inner)
			.attr('fill', (d) => {
				const { state, isUnlocked } =
					d.data.choices.length > 1
						? groupState(d.data.choices)
						: { state: d.data.choices[0]?.state ?? null, isUnlocked: d.data.choices[0]?.isUnlocked ?? false };
				return config.colors[state] ?? (isUnlocked ? 'var(--primary)' : 'var(--card)')
			})
			.attr('stroke', 'var(--border)')
			.attr('stroke-width', 1.5);
	}

	private appendChoiceDots(sel: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>) {
		sel.each(function (d) {
			if (d.data.choices.length <= 1) return;

			const g = d3.select<SVGGElement, SimNode>(this).append('g').attr('class', 'choices');
			const n = d.data.choices.length;

			d.data.choices.forEach((choice, i) => {
				const angle = (2 * Math.PI * i) / n - Math.PI / 2;
				const cx = CHOICE_ORBIT * Math.cos(angle);
				const cy = CHOICE_ORBIT * Math.sin(angle);

				g.append('circle')
					.attr('cx', cx).attr('cy', cy)
					.attr('r', CHOICE_DOT)
					.attr('fill', config.colors[choice.state] ?? (choice.isUnlocked ? 'var(--primary)' : 'var(--card)'))
					.attr('stroke', 'var(--border)')
					.attr('stroke-width', 1)
					.attr('cursor', 'pointer')
					// .on('click', (event) => {
					// 	event.stopPropagation();
					// 	console.log(`[Choice] ${choice.name}`);
					// });

				g.append('text')
					.attr('x', cx).attr('y', cy)
					.attr('text-anchor', 'middle')
					.attr('dominant-baseline', 'central')
					.attr('fill', stateTextColor(choice.state, choice.isUnlocked))
					.attr('font-size', '8px')
					.attr('font-weight', 'bold')
					.attr('pointer-events', 'none')
					.text(i + 1);
			});
		});
	}

	private appendLabels(sel: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>) {
		// eslint-disable-next-line @typescript-eslint/no-this-alias
		const self = this;

		sel.append('text')
			.attr('text-anchor', 'middle')
			.attr('font-weight', (d) => (d.data.type === 'root' || d.children ? 'bold' : 'normal'))
			.attr('pointer-events', 'none')
			.each(function (d) {
				const el = this as SVGTextElement;
				const lines = getLabel(d.data);
				const inner = RADII[sizeKey(d)].inner;
				const base = d.data.type === 'root' ? 13 : 10;

				el.setAttribute('font-size', `${base}px`);

				if (lines.length === 1) {
					el.setAttribute('dominant-baseline', 'central');
					el.textContent = lines[0];
				} else {
					lines.forEach((line, i) => {
						const ts = document.createElementNS('http://www.w3.org/2000/svg', 'tspan');
						ts.setAttribute('x', '0');
						ts.setAttribute('dy', i === 0 ? `${-((lines.length - 1) * 0.55)}em` : '1.1em');
						ts.textContent = line;
						el.appendChild(ts);
					});
				}

				self.fitTextToCircle(el, inner);

				const { state, isUnlocked } =
					d.data.choices.length > 1
						? groupState(d.data.choices)
						: { state: d.data.choices[0]?.state ?? null, isUnlocked: d.data.choices[0]?.isUnlocked ?? false };

				el.setAttribute('fill', d.data.type === 'root' ? '#fff' : stateTextColor(state, isUnlocked));
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

	private attachNodeClick(sel: d3.Selection<SVGGElement, SimNode, SVGGElement, unknown>) {
		sel.on('click', function (event: PointerEvent, d: SimNode) {
			console.log(d.data)
			// if (d.data.choices.length > 1) {
			// 	console.log('[Choice group]', d.data.choices.map((c) => c.name));
			// } else {
			// 	const lel = d.data.children
			// 	console.log('[Goal]', d.data.choices[0]?.name ?? d.data.name);
			// }

			d3.select<SVGGElement, SimNode>(this)
				.select<SVGCircleElement>('circle.core')
				.transition().duration(80)
				.attr('stroke', 'var(--ring)')
				.attr('stroke-width', 3)
				.transition().duration(350)
				.attr('stroke', 'var(--border)')
				.attr('stroke-width', 1.5);
		});
	}

	focus(goalId: string): void {
		if (!this.element || !this.zoomBehavior || !this.simNodes.length || !this.nodeG) {
			console.warn('Galaxy is not fully rendered yet.');
			return;
		}

		const targetNode = this.simNodes.find((n) => n.data.choices.some((c) => c.goalId === goalId));

		if (!targetNode) {
			console.warn(`Goal ID ${goalId} not found in the current tree.`);
			return;
		}

		const svg = d3.select(this.element);
		const scale = 1.8;

		const transform = d3.zoomIdentity
			.scale(scale)
			.translate(-(targetNode.x ?? 0), -(targetNode.y ?? 0));

		svg.transition().duration(750).call(this.zoomBehavior.transform, transform);

		const nodeElement = this.nodeG.filter((d) => d === targetNode).node();

		if (nodeElement) {
			d3.select<SVGGElement, SimNode>(nodeElement)
				.select<SVGCircleElement>('circle.core')
				.transition().duration(80)
				.attr('stroke', 'var(--ring)')
				.attr('stroke-width', 3)
				.transition().duration(350)
				.attr('stroke', 'var(--border)')
				.attr('stroke-width', 1.5);

			const specificChoice = targetNode.data.choices.find((c) => c.goalId === goalId);
			if (targetNode.data.choices.length > 1) {
				console.log('[Focused Choice group]', targetNode.data.choices.map((c) => c.name));
				console.log(`[Focused Specific Target] ${specificChoice?.name}`);
			} else {
				console.log('[Focused Goal]', targetNode.data.choices[0]?.name ?? targetNode.data.name);
			}
		}
	}

	private buildDrag(simulation: d3.Simulation<SimNode, SimLink>) {
		return d3.drag<SVGGElement, SimNode>()
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
