<script lang="ts">
	import * as d3 from "d3";
	import type { CursusNode, HierarchyNode, HierarchyLink, Goal } from "./d3utils.svelte";
	import teleport from "$lib/hooks/teleport.svelte";
	// import * as Breadcrumb from "$lib/components/breadcrumb";
	// import Slash from "@lucide/svelte/icons/slash";

	let { data } = $props<{ data: CursusNode }>();

	let svg: SVGElement;
	let width = $state(1000); // Default width
	let height = $state(800); // Default height
	let currentPath = $state<string[]>(["root"]);
	let selectedNode = $state<HierarchyNode | null>(null);
	let mainG: d3.Selection<SVGGElement, unknown, null, undefined>;
	let showPopup = $state(false);
	let popupX = $state(0);
	let popupY = $state(0);
	let currentRoot = $state<string>("root");
	let breadcrumbs = $derived.by(() => {
		return currentPath.map((id, index) => {
			const nodeName = findNodeName(data, id);
			return { id, name: nodeName || id, index };
		});
	});

	// Find node name in hierarchy
	function findNodeName(rootNode: CursusNode, id: string): string | null {
		if (rootNode.id === id) return rootNode.name;

		for (const child of rootNode.children) {
			const name = findNodeName(child, id);
			if (name) return name;
		}

		return null;
	}

	// Find node by ID in hierarchy
	function findNode(rootNode: CursusNode, id: string): CursusNode | null {
		if (rootNode.id === id) return rootNode;

		for (const child of rootNode.children) {
			const node = findNode(child, id);
			if (node) return node;
		}

		return null;
	}

	// Prepare visualization data
	function prepareVisualizationData(
		rootNode: CursusNode,
		maxDepth: number = 4,
	): { nodes: HierarchyNode[]; links: HierarchyLink[] } {
		const nodes: HierarchyNode[] = [];
		const links: HierarchyLink[] = [];
		const visited = new Set<string>();

		function processNode(
			node: CursusNode,
			depth: number = 0,
			parent: string | null = null,
		): void {
			if (visited.has(node.id)) return;
			visited.add(node.id);

			const hierarchyNode: HierarchyNode = {
				...node,
				depth,
				isRouteNode: false,
				parentId: parent,
				isRoot: !parent && node.id === currentRoot,
			};
			nodes.push(hierarchyNode);

			if (parent) {
				links.push({
					source: parent,
					target: node.id,
				});
			}

			// Process children if within max depth, otherwise add route nodes
			if (depth < maxDepth) {
				node.children.forEach((child) => {
					processNode(child, depth + 1, node.id);
				});
			} else if (node.children.length > 0) {
				// Add a route node if there are children beyond max depth
				const routeNodeId = `route-${node.id}`;
				nodes.push({
					id: routeNodeId,
					name: "More...",
					state: 1,
					goals: [],
					isRouteNode: true,
					depth: depth + 1,
					parentId: node.id,
				});

				links.push({
					source: node.id,
					target: routeNodeId,
				});
			}
		}

		const startNode = findNode(rootNode, currentRoot);
		if (startNode) {
			processNode(startNode);
		}

		return { nodes, links };
	}

	// Set initial positions based on tree layout
	function setInitialPositions(nodes: HierarchyNode[], links: HierarchyLink[]) {
		// Create a mapping of node IDs to nodes for quick lookup
		const nodeMap: Record<string, HierarchyNode> = {};
		nodes.forEach((node) => {
			nodeMap[node.id] = node;
		});

		// Build a hierarchical tree structure from the nodes and links
		const rootId = nodes.find((n) => !n.parentId)?.id;
		if (!rootId) return;

		const childrenMap: Record<string, string[]> = {};
		links.forEach((link) => {
			const sourceId = typeof link.source === "string" ? link.source : link.source.id;
			const targetId = typeof link.target === "string" ? link.target : link.target.id;

			if (!childrenMap[sourceId]) {
				childrenMap[sourceId] = [];
			}
			childrenMap[sourceId].push(targetId);
		});

		// Create a simple radial layout
		const radius = Math.min(width, height) * 0.4;
		const centerX = width / 2;
		const centerY = height / 2;

		// Position root node in center
		nodeMap[rootId].x = centerX;
		nodeMap[rootId].y = centerY;

		// Function to position children around their parent
		function positionChildren(
			nodeId: string,
			depth: number,
			startAngle: number,
			endAngle: number,
		) {
			const children = childrenMap[nodeId] || [];
			if (children.length === 0) return;

			const angleStep = (endAngle - startAngle) / children.length;
			const nodeRadius = radius * (depth / 4); // The deeper, the further from center

			children.forEach((childId, i) => {
				const angle = startAngle + i * angleStep;
				const childNode = nodeMap[childId];

				// Position child
				childNode.x = centerX + nodeRadius * Math.cos(angle);
				childNode.y = centerY + nodeRadius * Math.sin(angle);

				// Position grandchildren
				const childSectionSize = angleStep * 0.9; // Slightly smaller to create separation
				positionChildren(
					childId,
					depth + 1,
					angle - childSectionSize / 2,
					angle + childSectionSize / 2,
				);
			});
		}

		// Start positioning from the root, covering the full circle (0 to 2π)
		positionChildren(rootId, 1, 0, 2 * Math.PI);
	}

	function initGraph() {
		if (!svg) return;

		// Clear previous elements
		d3.select(svg).selectAll("*").remove();

		const { nodes, links } = prepareVisualizationData(data, 2);

		// Apply initial positions based on a tree layout
		setInitialPositions(nodes, links);

		// Create SVG and define the force simulation
		const svgSelection = d3.select(svg);

		// Create a group that can be transformed for zoom
		mainG = svgSelection.append("g") as d3.Selection<
			SVGGElement,
			unknown,
			null,
			undefined
		>;

		// Add zoom behavior
		const zoom = d3
			.zoom<SVGElement, unknown>()
			.scaleExtent([0.1, 3])
			.on("zoom", (event) => {
				mainG.attr("transform", event.transform);
			});

		svgSelection.call(zoom);

		// Create a nodeMap for quick access
		const nodeMap: Record<string, HierarchyNode> = {};
		nodes.forEach((node) => {
			nodeMap[node.id] = node;
		});

		// Create the force simulation with improved parameters
		const simulation = d3
			.forceSimulation<HierarchyNode>(nodes)
			.force(
				"link",
				d3
					.forceLink<HierarchyNode, HierarchyLink>(links)
					.id((d) => d.id)
					.distance((d) => {
						// Make parent-child links have consistent distances
						// Route nodes can be closer to their parents
						const source = typeof d.source === "string" ? d.source : d.source.id;
						const target = typeof d.target === "string" ? d.target : d.target.id;
						const isRouteNode = nodeMap[target]?.isRouteNode;

						// Increase link distances for better spacing
						return isRouteNode ? 80 : 180;
					})
					.strength(0.7),
			) // Stronger link forces for more structured layout
			.force("charge", d3.forceManyBody().strength(-1200).distanceMax(800)) // Stronger repulsion for better spacing
			.force("center", d3.forceCenter(width / 2, height / 2).strength(0.05))
			.force("collide", d3.forceCollide().radius(70).strength(0.8)) // Larger collision radius
			.alpha(1) // Start with high energy
			.alphaDecay(0.02); // Slow decay to allow more organization time

		// Create the link elements
		const link = mainG
			.selectAll("line")
			.data(links)
			.enter()
			.append("line")
			.attr("stroke", "var(--border)")
			.attr("stroke-opacity", 0.6)
			.attr("stroke-width", 2);

		// Create the node elements
		const node = mainG
			.selectAll(".node")
			.data(nodes)
			.enter()
			.append("g")
			.attr("class", "node")
			.on("click", (event, d) => handleNodeClick(event, d))
			.call(
				d3
					.drag<SVGGElement, HierarchyNode>()
					.on("start", dragstarted)
					.on("drag", dragged)
					.on("end", dragended),
			);

		// Add background circle (group boundary)
		node
			.append("circle")
			.attr("class", "node-boundary")
			.attr("r", (d) => {
				if (d.isRouteNode) return 30; // Larger route nodes
				if (d.isRoot) return 60; // Much larger root node
				return 45; // Larger regular nodes
			})
			.attr("stroke", (d) => (d.isRoot ? "var(--primary)" : "#ddd"))
			.attr("stroke-width", (d) => (d.isRoot ? 3 : 1))
			.attr("stroke-dasharray", (d) => (d.isRouteNode ? "3,3" : "none"))
			.attr("opacity", 0.7);

		// Add main central node circle
		node
			.append("circle")
			.attr("class", "node-center")
			.attr("r", (d) => {
				if (d.isRouteNode) return 25; // Larger route nodes
				if (d.isRoot) return 40; // Much larger root node
				return 30; // Larger regular nodes
			})
			.attr("fill", (d) => {
				if (d.isRouteNode) return "var(--primary)";
				if (d.isRoot) return "var(--primary)";
				return d.state === 1 ? "#4CAF50" : "#ccc";
			})
			.attr("stroke", "var(--border)")
			.attr("stroke-width", 1.5);

		// Add individual goal circles around main node
		node.each(function (d) {
			if (d.isRouteNode || !d.goals.length) return; // Skip route nodes or nodes with no goals

			const numGoals = d.goals.length;
			if (numGoals <= 1) return; // Skip nodes with only one goal

			// Larger goal circles and positioning
			const goalRadius = d.isRoot ? 15 : 12;
			const circleRadius = d.isRoot ? 45 : 35; // Placement circle radius

			const goalGroup = d3
				.select(this)
				.append("g")
				.attr("class", "goal-group")
				.attr("pointer-events", "none"); // Let events pass through to parent

			// Draw individual goals as circles around the node
			d.goals.forEach((goal, i) => {
				const angle = (2 * Math.PI * i) / numGoals;
				const x = circleRadius * Math.cos(angle);
				const y = circleRadius * Math.sin(angle);

				// Goal circle
				goalGroup
					.append("circle")
					.attr("cx", x)
					.attr("cy", y)
					.attr("r", goalRadius)
					.attr("fill", goal.state === 1 ? "#4CAF50" : "#ccc")
					.attr("stroke", "var(--muted)")
					.attr("stroke-width", 1.5);

				// Optional: Goal number/index inside circles
				goalGroup
					.append("text")
					.attr("x", x)
					.attr("y", y)
					.attr("text-anchor", "middle")
					.attr("stroke", "var(--text-foreground)")
					.attr("dominant-baseline", "central")
					.attr("font-size", d.isRoot ? "10px" : "9px")
					.attr("font-weight", "bold")
					.text(i + 1);
			});
		});

		// Add text labels to nodes (centered inside now)
		node
			.append("text")
			.attr("text-anchor", "middle")
			.attr("dominant-baseline", "central") // Center vertically
			.text((d) => {
				// Truncate text if too long based on node size
				const maxLength = d.isRoot ? 12 : d.isRouteNode ? 8 : 0;
				return d.name.length > maxLength
					? d.name.substring(0, maxLength)
					: d.name;
			})
			.attr("font-size", (d) => (d.isRoot ? "14px" : d.isRouteNode ? "12px" : "12px"))
			.attr("font-weight", (d) => (d.isRoot ? "bold" : "normal"))
			.attr("fill", (d) => (d.isRouteNode || d.isRoot ? "#fff" : "#333")); // Light text on dark backgrounds

		// Update positions on each tick
		simulation.on("tick", () => {
			link
				.attr("x1", (d) => (d.source as HierarchyNode).x!)
				.attr("y1", (d) => (d.source as HierarchyNode).y!)
				.attr("x2", (d) => (d.target as HierarchyNode).x!)
				.attr("y2", (d) => (d.target as HierarchyNode).y!);

			node.attr("transform", (d) => `translate(${d.x}, ${d.y})`);
		});

		// Run simulation for a number of ticks before rendering to get a better layout
		simulation
			.tick(50) // Run for 50 ticks before displaying
			.alphaTarget(0)
			.restart();

		function dragstarted(
			event: d3.D3DragEvent<SVGGElement, HierarchyNode, HierarchyNode>,
			d: HierarchyNode,
		) {
			if (!event.active) simulation.alphaTarget(0.3).restart();
			d.fx = d.x;
			d.fy = d.y;
		}

		function dragged(
			event: d3.D3DragEvent<SVGGElement, HierarchyNode, HierarchyNode>,
			d: HierarchyNode,
		) {
			d.fx = event.x;
			d.fy = event.y;
		}

		function dragended(
			event: d3.D3DragEvent<SVGGElement, HierarchyNode, HierarchyNode>,
			d: HierarchyNode,
		) {
			if (!event.active) simulation.alphaTarget(0);
			d.fx = null;
			d.fy = null;
		}

		// Return cleanup function
		return () => simulation.stop();
	}

	function handleNodeClick(event: MouseEvent, node: HierarchyNode) {
		if (node.isRouteNode && node.parentId) {
			// Navigate deeper if it's a route node
			currentRoot = node.parentId.replace("route-", "");
			currentPath = [...currentPath, currentRoot];
			initGraph();
		} else {
			// Show popup for regular node
			selectedNode = node;
			popupX = event.clientX;
			popupY = event.clientY;
			showPopup = true;
		}
	}

	function navigateBack() {
		if (currentPath.length > 1) {
			currentPath = currentPath.slice(0, -1);
			currentRoot = currentPath[currentPath.length - 1];
			initGraph();
		}
	}

	function navigateToBreadcrumb(index: number) {
		if (index < currentPath.length - 1) {
			currentPath = currentPath.slice(0, index + 1);
			currentRoot = currentPath[index];
			initGraph();
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === "Escape") {
			if (showPopup) {
				// Close popup if it's open
				showPopup = false;
			} else {
				// Navigate back if no popup is open
				navigateBack();
			}
		}
	}

	function closePopup() {
		showPopup = false;
	}

	function handleResize() {
		// Update dimensions safely
		if (typeof window !== "undefined") {
			width = window.innerWidth;
			height = window.innerHeight;
			initGraph();
		}
	}

	// Initialize graph on mount and handle window resizing
	$effect(() => {
		// Only run in the browser, not on server
		if (typeof window !== "undefined") {
			// Set the dimensions based on window size
			width = window.innerWidth;
			height = window.innerHeight;

			const cleanup = initGraph();

			window.addEventListener("keydown", handleKeydown);
			window.addEventListener("resize", handleResize);

			return () => {
				cleanup?.();
				window.removeEventListener("keydown", handleKeydown);
				window.removeEventListener("resize", handleResize);
			};
		}
		return;
	});
</script>

<div class="flex h-screen w-full flex-col bg-gradient-to-tr to-blue-950/10">
	<div class="relative flex-grow">
		<svg bind:this={svg} {width} {height} class="h-full w-full"></svg>
	</div>

	<!-- Node detail popup -->
	<!-- {#if showPopup && selectedNode}
		<div
			class="bg-card text-card-foreground absolute max-w-md rounded-lg p-4 shadow-lg"
			style="top: {popupY}px; left: {popupX}px; transform: translate(-50%, -100%); z-index: 1000;"
		>
			<div class="mb-2 flex items-center justify-between">
				<h3 class="text-lg font-bold">{selectedNode.name}</h3>
				<button class="text-muted-foreground hover:text-foreground" onclick={closePopup}>
					<svg
						xmlns="http://www.w3.org/2000/svg"
						class="h-5 w-5"
						fill="none"
						viewBox="0 0 24 24"
						stroke="currentColor"
					>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M6 18L18 6M6 6l12 12"
						/>
					</svg>
				</button>
			</div>
			<div class="text-sm">
				<pre class="bg-muted/20 max-h-60 overflow-auto rounded p-2">
			{JSON.stringify(selectedNode, null, 2)}
			</pre>
			</div>
		</div>
	{/if} -->
</div>

<datalist id="ice-cream-flavors">
	<option value="Chocolate"></option>
	<option value="Coconut"></option>
	<option value="Mint"></option>
	<option value="Strawberry"></option>
	<option value="Vanilla"></option>
</datalist>

<!-- <Breadcrumb.Root
	{@attach teleport("breadcrumbs")}
	class="bg-card/30 h-min cursor-pointer rounded-br-md border-r border-b p-2 backdrop-blur-[4px]"
>
	<Breadcrumb.List>
		{#each breadcrumbs as crumb, i}
			<Breadcrumb.Item>
				<Breadcrumb.Link class="text-foreground" onclick={() => navigateToBreadcrumb(i)}
					>{crumb.name}</Breadcrumb.Link
				>
			</Breadcrumb.Item>
			{#if i < breadcrumbs.length - 1}
				<Breadcrumb.Separator>
					<Slash />
				</Breadcrumb.Separator>
			{/if}
		{/each}
	</Breadcrumb.List>
</Breadcrumb.Root> -->

<div {@attach teleport("tooltip")}></div>

<style>
	:global(.node-boundary) {
		fill: var(--muted);

	}
</style>
