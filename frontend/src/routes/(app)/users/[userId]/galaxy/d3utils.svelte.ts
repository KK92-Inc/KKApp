export interface Goal {
	id: string;
	title: string;
	state: 0 | 1;
}

export interface CursusNode {
	id: string;
	name: string;
	state: 0 | 1;
	goals: Goal[];
	children: CursusNode[];
}

export interface HierarchyNode extends d3.SimulationNodeDatum {
	id: string;
	name: string;
	state: 0 | 1;
	goals: Goal[];
	children?: HierarchyNode[];
	depth?: number;
	x?: number;
	y?: number;
	fx?: number | null;
	fy?: number | null;
	isRouteNode?: boolean;
	isRoot?: boolean;
	parentId?: string;
}

export interface HierarchyLink {
	source: string | HierarchyNode;
	target: string | HierarchyNode;
}

export const generateSampleData = (): CursusNode => {
	const createNode = (id: string, depth = 0, parentId = ""): CursusNode => {
		const name = `Node ${id}`;

		// Generate random number of children (0-4) unless we've reached depth 8
		const numChildren = depth >= 5 ? 0 : Math.floor(Math.random() * 5);
		const children: CursusNode[] = [];

		for (let i = 0; i < numChildren; i++) {
			const childId = `${id}-${i}`;
			children.push(createNode(childId, depth + 1, id));
		}

		// Create 1-3 goals for this node
		const numGoals = Math.max(1, Math.floor(Math.random() * 4));
		const goals = Array.from({ length: numGoals }, (_, i) => ({
			id: `goal-${id}-${i}`,
			title: `Goal ${i + 1} for ${name}`,
			state: Math.random() > 0.5 ? 1 : 0,
		}));

		return {
			id,
			name,
			state: goals.some((g) => g.state === 1) ? 1 : 0,
			goals,
			children,
		};
	};

	return createNode("root");
};
