// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// Discriminated union matching backend Rule types
export type RuleType =
	| 'all_of' | 'any_of' | 'not'
	| 'has_cursus' | 'has_project' | 'is_member'
	| 'min_days_registered' | 'min_projects_completed'
	| 'min_reviews_completed' | 'same_timezone';

export interface BaseRule {
	$type: RuleType;
	description?: string;
}

// Composite rules
export interface AllOfRule extends BaseRule {
	$type: 'all_of';
	rules: Rule[];
}

export interface AnyOfRule extends BaseRule {
	$type: 'any_of';
	rules: Rule[];
}

export interface NotRule extends BaseRule {
	$type: 'not';
	rule: Rule;
}

// Leaf rules
export interface HasCursusRule extends BaseRule {
	$type: 'has_cursus';
	cursusId: string;
}

export interface HasProjectRule extends BaseRule {
	$type: 'has_project';
	projectId: string;
}

export interface IsMemberRule extends BaseRule {
	$type: 'is_member';
}

export interface MinDaysRegisteredRule extends BaseRule {
	$type: 'min_days_registered';
	days: number;
}

export interface MinProjectsCompletedRule extends BaseRule {
	$type: 'min_projects_completed';
	count: number;
}

export interface MinReviewsCompletedRule extends BaseRule {
	$type: 'min_reviews_completed';
	count: number;
}

export interface SameTimezoneRule extends BaseRule {
	$type: 'same_timezone';
}

export type CompositeRule = AllOfRule | AnyOfRule | NotRule;
export type LeafRule = HasCursusRule | HasProjectRule | IsMemberRule
	| MinDaysRegisteredRule | MinProjectsCompletedRule | MinReviewsCompletedRule
	| SameTimezoneRule;
export type Rule = CompositeRule | LeafRule;

// UI metadata for each rule type
export const RULE_METADATA: Record<RuleType, {
	label: string;
	icon: string;
	color: string;
	isComposite: boolean;
	description: string;
}> = {
	all_of: {
		label: 'All Of',
		icon: 'layers',
		color: '#3b82f6',
		isComposite: true,
		description: 'All child rules must pass'
	},
	any_of: {
		label: 'Any Of',
		icon: 'git-branch',
		color: '#10b981',
		isComposite: true,
		description: 'At least one child rule must pass'
	},
	not: {
		label: 'Not',
		icon: 'x-circle',
		color: '#ef4444',
		isComposite: true,
		description: 'The child rule must NOT pass'
	},
	has_cursus: {
		label: 'Has Cursus',
		icon: 'graduation-cap',
		color: '#8b5cf6',
		isComposite: false,
		description: 'User must be enrolled in specific cursus'
	},
	has_project: {
		label: 'Has Project',
		icon: 'folder',
		color: '#f59e0b',
		isComposite: false,
		description: 'User must have completed specific project'
	},
	is_member: {
		label: 'Is Member',
		icon: 'users',
		color: '#06b6d4',
		isComposite: false,
		description: 'User must be a member'
	},
	min_days_registered: {
		label: 'Min Days Registered',
		icon: 'calendar-days',
		color: '#84cc16',
		isComposite: false,
		description: 'Minimum days since registration'
	},
	min_projects_completed: {
		label: 'Min Projects Completed',
		icon: 'check-circle',
		color: '#22c55e',
		isComposite: false,
		description: 'Minimum number of projects completed'
	},
	min_reviews_completed: {
		label: 'Min Reviews Completed',
		icon: 'star',
		color: '#f97316',
		isComposite: false,
		description: 'Minimum number of reviews completed'
	},
	same_timezone: {
		label: 'Same Timezone',
		icon: 'clock',
		color: '#6366f1',
		isComposite: false,
		description: 'Must be in same timezone'
	}
};

export function createDefaultRule(type: RuleType): Rule {
	switch (type) {
		case 'all_of':
			return { $type: 'all_of', rules: [] };
		case 'any_of':
			return { $type: 'any_of', rules: [] };
		case 'not':
			return { $type: 'not', rule: { $type: 'is_member' } };
		case 'has_cursus':
			return { $type: 'has_cursus', cursusId: '' };
		case 'has_project':
			return { $type: 'has_project', projectId: '' };
		case 'is_member':
			return { $type: 'is_member' };
		case 'min_days_registered':
			return { $type: 'min_days_registered', days: 7 };
		case 'min_projects_completed':
			return { $type: 'min_projects_completed', count: 1 };
		case 'min_reviews_completed':
			return { $type: 'min_reviews_completed', count: 1 };
		case 'same_timezone':
			return { $type: 'same_timezone' };
		default:
			throw new Error(`Unknown rule type: ${type}`);
	}
}