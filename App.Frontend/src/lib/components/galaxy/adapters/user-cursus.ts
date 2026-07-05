// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for rendering a user cursus entity onto the Galaxy
// ============================================================================

import type { components } from '$lib/api/api';
import type { GalaxyItem, GalaxyNode } from '../types';
import { groupByParent } from './shared';
import config from '../config';

// ============================================================================

export type Track = components['schemas']['UserCursusTrackDO'];
export type TrackNode = components['schemas']['UserCursusTrackNodeDO'];
// Represents the grouped lookup map returned by groupByParent
type GroupMap = Map<string, Map<string | null, TrackNode[]>>;

// ============================================================================

const STATE_ORDER: Record<string, number> = {
  Inactive: 1,
  Awaiting: 2,
  Active: 3,
  Completed: 4,
};

function getColor(state?: string | null, isUnlocked?: boolean): string {
  return config.colors[state ?? ''] ?? (isUnlocked ? 'var(--chart-2)' : 'var(--card)');
}

function getTextColor(state?: string | null, isUnlocked?: boolean): string {
  return state == null && !isUnlocked ? 'var(--muted-foreground)' : '#fff';
}

function toItem(node: TrackNode): GalaxyItem<TrackNode> {
  return {
    id: node.goalId,
    label: node.name,
    color: getColor(node.state, node.isUnlocked),
    textColor: getTextColor(node.state, node.isUnlocked),
    meta: node,
  };
}

/** Highest-priority state across a cluster's members, used for the shared core. */
function dominant(members: TrackNode[]): TrackNode {
  return members.reduce((a, b) =>
    (STATE_ORDER[b.state ?? ''] ?? 0) > (STATE_ORDER[a.state ?? ''] ?? 0) ? b : a
  );
}

function cluster(clusterId: string, members: TrackNode[]): GalaxyNode<TrackNode> {
  const best = dominant(members);
  const items = members.map(toItem);
  const chosen = members.find((m) => m.state != null);
  const isUnlocked = members.some((m) => m.isUnlocked);

  return {
    id: clusterId,
    label: chosen ? [chosen.name] : members.map((m) => m.name),
    color: getColor(best.state, isUnlocked),
    textColor: getTextColor(best.state, isUnlocked),
    items,
  };
}

function build(node: TrackNode, groups: GroupMap, isRoot = false): GalaxyNode<TrackNode> {
  const item = toItem(node);
  const childGroups = groups.get(node.goalId);
  const children: GalaxyNode<TrackNode>[] = [];

  if (childGroups) {
    for (const [key, members] of childGroups) {
      if (key === null) {
        members.forEach((child) => children.push(build(child, groups)));
      } else {
        children.push(cluster(key, members));
      }
    }
  }

  return {
    id: node.goalId,
    label: [node.name],
    color: item.color,
    textColor: isRoot ? '#fff' : item.textColor,
    items: [item],
    children: children.length ? children : undefined,
  };
}

// ============================================================================

export const Adapter = {
  /**
   * Builds a generic GalaxyNode tree from a user's flat cursus-track
   * node list, resolving state to color and collapsing choice groups into
   * clustered nodes along the way.
   * @param track The user-specific track of the cursus
   * @returns A GalaxyNode entity to render
   */
  construct(track: Track): GalaxyNode<TrackNode> {
    const nodes = track.nodes ?? [];
    if (nodes.length === 0) throw new Error(`Track "${track.name ?? 'Unknown'}" has no nodes.`);

    // Note: Cast as GroupMap assuming groupByParent returns a standard Map structure
    const groups = groupByParent(nodes, {
      parentId: (n) => n.parentGoalId,
      choiceGroup: (n) => n.choiceGroup,
    }) as GroupMap;

    const root = nodes.find((n) => !n.parentGoalId);
    if (!root) throw new Error(`Track "${track.name}" has no root node.`);

    return build(root, groups, true);
  },

  /** Flattens an assembled tree back into its underlying track nodes.
   * @param tree The constructed Galaxy tree
   * @returns A flat array of TrackNodes
   */
  flatten(tree: GalaxyNode<TrackNode>): TrackNode[] {
    const out: TrackNode[] = [];
    const visit = (node: GalaxyNode<TrackNode>) => {
      node.items.forEach((i) => out.push(i.meta));
      node.children?.forEach(visit);
    };
    visit(tree);
    return out;
  }
};
