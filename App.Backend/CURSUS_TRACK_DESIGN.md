# Cursus Track Design

> Design document for the cursus track system — how tracks are stored, how user
> state is computed, and how static vs dynamic cursus types differ.

## Problem Statement

A **cursus** (curriculum) contains a **track**: a hierarchical tree of **goals**
that students progress through. We need to:

1. Store the track structure (which goals, in what order, with what hierarchy)
2. Support **fixed** tracks (predefined path) and **dynamic** tracks (user-driven)
3. Track user progress without storing state *inside* the track, so that
   modifying the cursus doesn't corrupt existing user progress

## Approaches Considered

### Option A: Pure JSONB

Store the entire track as a JSON tree in `Cursus.Track`.

| Pros | Cons |
|------|------|
| Simple schema | No FK constraints to goals |
| Easy to read/write hierarchical data | Can't query "which cursus uses goal X?" |
| Flexible structure | No referential integrity — deleted goals leave orphans |
| | EF Core migration/index limitations on JSONB |

### Option B: Pure Relational (`rel_cursus_goal` table)

Store each goal-in-a-cursus as a row with an adjacency list for hierarchy.

| Pros | Cons |
|------|------|
| Referential integrity (FK to `tbl_goals`) | Slightly more complex tree assembly |
| Easy joins and queries | Recursive CTEs needed for deep trees |
| Indexes on `(cursus_id, parent_goal_id)` | |
| Queryable: "find all cursus containing goal X" | |

### Option C: Hybrid

Use relational for the canonical track, keep JSONB as a cached/computed
representation. This adds complexity without clear benefit.

### Decision: **Option B — Pure Relational**

The relational approach wins because:
- **Referential integrity** prevents orphaned goal references
- **Queryability** is essential (e.g. "if I deprecate goal X, which cursus are affected?")
- **User state computation** becomes a simple join (see below)
- The tree is typically shallow (2–4 levels), so adjacency list is efficient
- The existing `Cursus.Track` JSONB column is marked `[Obsolete]` and kept for
  migration compatibility only

---

## Data Model

### `rel_cursus_goal` (new relation table)

```
┌──────────────────┬──────────┬──────────────────────────────────────────┐
│ Column           │ Type     │ Description                              │
├──────────────────┼──────────┼──────────────────────────────────────────┤
│ cursus_id (PK)   │ uuid FK  │ References tbl_cursus.id                 │
│ goal_id (PK)     │ uuid FK  │ References tbl_goals.id                  │
│ position         │ int      │ Ordering among siblings                  │
│ parent_goal_id   │ uuid?    │ Another goal_id in the same cursus (null │
│                  │          │ for root-level nodes)                    │
│ created_at       │ timestamptz │ Inherited from BaseTimestampEntity    │
│ updated_at       │ timestamptz │ Inherited from BaseTimestampEntity    │
└──────────────────┴──────────┴──────────────────────────────────────────┘
```

**Composite PK**: `(cursus_id, goal_id)` — a goal appears at most once per cursus.

**Index**: `(cursus_id, parent_goal_id)` — fast child lookups.

### Tree structure (adjacency list)

```
Root goals:      parent_goal_id = NULL
Child goals:     parent_goal_id = <some goal_id in the same cursus>
Sibling order:   ORDER BY position
```

Example track:

```
Cursus: "Web Development"
├── [pos=0] Goal: "HTML Basics"             (parent: null)
│   ├── [pos=0] Goal: "CSS Fundamentals"    (parent: HTML Basics)
│   └── [pos=1] Goal: "JS Fundamentals"     (parent: HTML Basics)
│       └── [pos=0] Goal: "DOM Manipulation"(parent: JS Fundamentals)
└── [pos=1] Goal: "Git & CLI"              (parent: null)
```

### `tbl_cursus` changes

Added column:
- `variant` (`CursusVariant` enum: `Fixed` | `Dynamic`) — defaults to `Fixed`

The existing `track` JSONB column is marked `[Obsolete]` and will be removed
in a future migration.

---

## Cursus Variants

### Fixed (`CursusVariant.Fixed`)

- Track is defined by `rel_cursus_goal` entries
- The `POST /cursus/{id}/track` endpoint does a full replacement of the track
- Users follow the predefined path
- When the track changes, only **future** nodes are affected for existing users

### Dynamic (`CursusVariant.Dynamic`)

- No predefined track in `rel_cursus_goal`
- Users build their own path by subscribing to goals freely
- The `POST /cursus/{id}/track` endpoint returns `400 Bad Request`
- User progress is tracked purely through `tbl_user_goal` records

---

## User State Computation

### Principle: Compute, don't store

User progress is **never stored inside the track**. Instead, it's computed
on-the-fly by joining the cursus track with the user's goal completions:

```sql
-- For a given user + cursus, compute track state:
SELECT
    cg.goal_id,
    cg.position,
    cg.parent_goal_id,
    g.name,
    COALESCE(ug.state, 'Inactive') AS user_state
FROM rel_cursus_goal cg
JOIN tbl_goals g ON g.id = cg.goal_id
LEFT JOIN tbl_user_goal ug
    ON ug.goal_id = cg.goal_id
    AND ug.user_id = :userId
WHERE cg.cursus_id = :cursusId
ORDER BY cg.position;
```

### Why this works when the track changes

| Scenario | What happens |
|----------|-------------|
| Goal A is replaced by Goal E at position 0 | User's `UserGoal` for A remains (historical record). Goal E appears as `Inactive`. No data corruption. |
| Goal B (completed) is removed from track | User still has `UserGoal` for B. It simply doesn't appear in the current track view. Credit is preserved. |
| New Goal F is inserted between B and C | F appears as `Inactive`. B and C states are unchanged. |
| Goal ordering changes | Only `position` values change. User state is unaffected. |

The `tbl_user_goal` table acts as an **event log** of the user's achievements.
The track is just a *lens* through which we view those achievements.

### UserCursus.Track (JSONB)

The existing `UserCursus.Track` JSONB column can serve as a **cache** of the
computed state for performance. It should be recomputed when:
- The cursus track changes
- The user completes/starts a goal

This is optional and can be implemented as a background job or on-demand
computation with Redis caching.

---

## API Endpoints

### `POST /cursus/{id}/track` — Set/replace track

**Request:**
```json
{
  "nodes": [
    { "goalId": "...", "position": 0, "parentGoalId": null },
    { "goalId": "...", "position": 0, "parentGoalId": "<goal-id-above>" },
    { "goalId": "...", "position": 1, "parentGoalId": "<goal-id-above>" }
  ]
}
```

**Validations:**
- Cursus must exist and be `Fixed` variant
- All `goalId` values must reference existing goals
- All `parentGoalId` values must reference a `goalId` within the same request
- No duplicate `goalId` values
- Full replacement: existing track is wiped and replaced atomically

**Response:** `200 OK` with `CursusTrackDO` (nested tree format)

### `GET /cursus/{id}/track` — Get track

**Response:** `200 OK`
```json
{
  "cursusId": "...",
  "variant": "Fixed",
  "nodes": [
    {
      "goal": { "id": "...", "name": "HTML Basics", "slug": "html-basics", "active": true, "deprecated": false },
      "position": 0,
      "children": [
        {
          "goal": { "id": "...", "name": "CSS Fundamentals", ... },
          "position": 0,
          "children": []
        }
      ]
    }
  ]
}
```

The response uses a **nested tree** format (easier for frontends to render)
while the request uses a **flat list** format (easier to validate and process).

---

## Files Changed / Created

### New files
| File | Purpose |
|------|---------|
| `Domain/Relations/CursusGoal.cs` | Relation entity for cursus ↔ goal hierarchy |
| `Models/Requests/Cursus/PostCursusTrackRequestDTO.cs` | Request DTO for setting a track |
| `Models/Responses/Entities/Goals/GoalLightDO.cs` | Lightweight goal response for nested use |
| `Models/Responses/Entities/Cursus/CursusTrackDO.cs` | Track response with nested tree |
| `Models/Responses/Entities/Cursus/CursusTrackNodeDO.cs` | Individual node in the track tree |

### Modified files
| File | Change |
|------|--------|
| `Domain/Entities/Cursus.cs` | Added `Variant` column, marked `Track` as `[Obsolete]` |
| `Database/DatabaseContext.cs` | Registered `DbSet<CursusGoal>` |
| `Core/Services/Interface/ICursusService.cs` | Added `SetTrackAsync` and `GetTrackAsync` |
| `Core/Services/Implementation/CursusService.cs` | Implemented track methods with transaction |
| `API/Controllers/CursusController.cs` | Added `GET/POST /cursus/{id}/track` endpoints |
| `Models/Requests/Cursus/PostCursusRequestDTO.cs` | Replaced `Track` with `Variant` |
| `Models/Responses/Entities/Cursus/CursusDO.cs` | Added `Variant` to response |
| `API/Controllers/WorkspaceController.cs` | Updated cursus creation to use `Variant` |

---

## Migration Required

After these changes, you'll need to create a new EF Core migration:

```bash
cd App.Migrations
dotnet ef migrations add AddCursusTrackRelation \
  --context DatabaseContext \
  --project ../App.Backend/Database
```

This will generate a migration that:
1. Creates the `rel_cursus_goal` table with composite PK
2. Adds the `variant` column to `tbl_cursus`
3. Creates the index on `(cursus_id, parent_goal_id)`

---

## Future Considerations

- **User track state endpoint**: `GET /cursus/{id}/track/me` that joins with
  `UserGoal` to return the computed state per node
- **Track diffing**: When a track changes, compute what changed for notification
  purposes (e.g. "Goal X was replaced by Goal Y in your cursus")
- **Redis caching**: Cache the computed user track state with a TTL, invalidated
  on track changes or goal completions
- **Remove JSONB columns**: Once migration is complete and all consumers are
  updated, drop `Cursus.Track` and `UserCursus.Track` JSONB columns
