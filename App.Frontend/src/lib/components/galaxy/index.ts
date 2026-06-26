import { v4 as uuidv4 } from 'uuid';

export type EntityObjectState = 'Active' | 'Inactive' | 'Awaiting' | 'Completed';

export interface UserCursusTrackNodeDO {
	goalId: string;
	name: string;
	slug: string;
	isUnlocked: boolean;
	parentGoalId: string | null;
	choiceGroup: string | null;
	state: EntityObjectState;
}

export interface UserCursusTrackDO {
	cursusId: string;
	name: string;
	completionMode: string;
	nodes: UserCursusTrackNodeDO[];
}

export interface D3NodeDatum {
    id: string;
	name: string;
	type: 'root' | 'node' | 'leaf';
	goals: UserCursusTrackNodeDO[];
	children: D3NodeDatum[];
}

/**
 * Generates a mock tree with a realistic center-to-edge progression path.
 */
export function generateMockCursusData(): UserCursusTrackDO {
	const nodes: UserCursusTrackNodeDO[] = [];

	const createNode = (name: string, parentId: string | null, depth: number, pathState: 'Completed' | 'Active' | 'Locked') => {
		if (depth > 5) return;

		// 40% chance it's a choice group (multiple nodes), 60% chance it's a single goal
		const isChoiceGroup = Math.random() > 0.6;
		const choiceGroup = isChoiceGroup ? uuidv4() : null;
		const numGoals = isChoiceGroup ? 3 : 1;

		let primaryGoalId = "";

		for (let i = 1; i <= numGoals; i++) {
			const goalId = uuidv4();
			if (i === 1) primaryGoalId = goalId;

            // Map our logical path to your entity states
            let state: EntityObjectState = 'Inactive';
            if (pathState === 'Completed') state = 'Completed';
            else if (pathState === 'Active') state = 'Active';

			nodes.push({
				goalId: goalId,
				name: i === 1 ? name : `${name} (Alt ${i})`,
				slug: `slug-${goalId}`,
				isUnlocked: pathState !== 'Locked', // Locked items are not unlocked
				parentGoalId: parentId,
				choiceGroup: choiceGroup,
				state: state
			});
		}

		if (depth < 5) {
			const numChildren = Math.floor(Math.random() * 3) + 2; // 2 to 4 children
			for (let c = 1; c <= numChildren; c++) {

                // Progression logic: Center-out flow
                let childPathState: 'Completed' | 'Active' | 'Locked' = 'Locked';

                if (pathState === 'Completed') {
                    // If parent is complete, child can be anything. Creates a "frontier".
                    const rand = Math.random();
                    if (rand > 0.5) childPathState = 'Completed';
                    else if (rand > 0.2) childPathState = 'Active';
                }

				createNode(depth === 1 ? `Branch ${c}` : `Sector ${depth}-${c}`, primaryGoalId, depth + 1, childPathState);
			}
		}
	};

	// Start the root as completed
	createNode("My Awesome G...", null, 1, 'Completed');

	return {
		cursusId: uuidv4(),
		name: "Mock Cursus",
		completionMode: "FreeStyle",
		nodes
	};
}

/**
 * Transforms flat list to D3 Hierarchy, perfectly handling Singletons vs ChoiceGroups
 */
export function transformApiToD3Hierarchy(cursus: UserCursusTrackDO): D3NodeDatum {
	const nodeMap = new Map<string, D3NodeDatum>();
	const groupedGoals = new Map<string, UserCursusTrackNodeDO[]>();

	// 1. Group by choiceGroup, fallback to goalId if null
	cursus.nodes.forEach(node => {
		const groupId = node.choiceGroup || node.goalId;
		if (!groupedGoals.has(groupId)) groupedGoals.set(groupId, []);
		groupedGoals.get(groupId)!.push(node);
	});

	// 2. Create base D3 nodes
	groupedGoals.forEach((goals, groupId) => {
		nodeMap.set(groupId, {
            id: groupId,
			name: goals[0].name,
			type: 'node',
			goals: goals,
			children: []
		});
	});

	let rootId: string | null = null;

	// 3. Connect parents to children
	groupedGoals.forEach((goals, groupId) => {
		const parentId = goals[0].parentGoalId;
		if (!parentId) {
			rootId = groupId;
		} else {
			const parentGroup = Array.from(groupedGoals.entries())
                .find(([_, gList]) => gList.some(g => g.goalId === parentId))?.[0];

			if (parentGroup && nodeMap.has(parentGroup)) {
				nodeMap.get(parentGroup)!.children.push(nodeMap.get(groupId)!);
			}
		}
	});

	const rootNode = nodeMap.get(rootId!);
	if (rootNode) rootNode.type = 'root';

	return rootNode!;
}
