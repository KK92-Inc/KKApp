# Controller Architecture Design

> Date: 2026-02-09 | Author: Copilot

## Problem Statement

The API controllers had several issues:

1. **`UserController`** was a god-controller — it housed general user queries, current-user
   operations (profile, notifications, SSE stream, spotlights), and was becoming hard to navigate.
2. **`AccountController`** had the wrong class name (`UserCursusController`) and was a placeholder
   with mismatched route semantics.
3. **`UserCursusController`** was a stub that didn't properly expose user session operations.
4. **`SubscriptionController`** had confusing parameter names (`goalId` was used for cursus and
   project IDs too) and injected services it never used.
5. There was no way to query user sessions (UserCursus, UserGoal, UserProject) by both their
   **composite key** (userId + entityId) and their **entity ID** (the UserCursus/UserGoal/UserProject
   primary key).

---

## Thinking Process

### Step 1: Identify the Responsibilities

Looking at the domain model, there are four distinct concerns for "user-facing" controllers:

| Concern | What it does | Who uses it |
|---------|-------------|-------------|
| **User management** | CRUD on User entities | Staff / admin |
| **Account** | "Who am I?" — profile, notifications, events | The authenticated user |
| **User sessions** | Read user enrollment state for cursi, goals, projects | Anyone with access |
| **Subscriptions** | Subscribe/unsubscribe (write operations) | The user or staff |

Mixing reads and writes for sessions into one controller would be fine for small APIs, but here
the session read patterns are complex (dual-key lookups, nested resources like track/members) while
subscriptions are simple POST/DELETE toggles. Keeping them separate means each controller stays
focused.

### Step 2: The Dual-Key Routing Problem

User session entities like `UserCursus` have two natural identifiers:

- **Composite key**: "Show me user X's enrollment in cursus Y" → `GET /users/{userId}/cursus/{cursusId}`
- **Entity ID**: "Show me UserCursus with ID Z" → `GET /user-cursus/{id}`

Both are useful depending on what the caller already knows. The frontend typically navigates from
a user profile page (has userId) and picks a cursus, so the composite route is natural. But when
linking from a notification or activity log, you already have the UserCursus ID.

**ASP.NET Core solution**: Use the controller `[Route]` attribute for the nested prefix, and
absolute routes (starting with `/`) on individual actions for the direct-access pattern:

```csharp
[Route("users/{userId:guid}/cursus")]   // nested prefix
public class UserCursusController
{
    [HttpGet]                            // → GET /users/{userId}/cursus
    [HttpGet("{cursusId:guid}")]         // → GET /users/{userId}/cursus/{cursusId}
    [HttpGet("/user-cursus/{id:guid}")]  // → GET /user-cursus/{id}  (absolute)
}
```

When a route template starts with `/`, ASP.NET Core treats it as absolute — the controller's
`[Route]` prefix is ignored for that action. This lets one controller own both URL shapes without
any hack.

### Step 3: Session Controllers per Entity Type

Considered putting all three (cursus, goal, project) in one controller, but:

- **UserProject** has sub-resources (members, transactions, reviews) that goals and cursi don't.
- **UserCursus** has a track/progress sub-resource.
- Separate controllers keep each file < 120 lines and easy to scan.

### Step 4: Subscription Controller Cleanup

The old `SubscriptionController` had:

- `goalId` used as the parameter name even for cursus and project routes (copy-paste).
- Five injected services (`IUserService`, `IProjectService`, `IGoalService`, `ICursusService`)
  that were never used — only `ISubscriptionService` was called.
- Inconsistent authorization checks (some actions checked, some didn't).

Fixed by:
- Using correct parameter names (`cursusId`, `goalId`, `projectId`).
- Removing unused DI injections.
- Extracting the auth check into a `IsAllowed()` helper.
- Adding `[Tags("subscriptions")]` for OpenAPI grouping.

### Step 5: Service Layer Gaps

The existing service layer had `IDomainService<UserProject>` via `IUserProjectService` but nothing
for `UserCursus` or `UserGoal`. The controllers need:

- `FindByIdAsync(id)` — provided by `IDomainService<T>` base
- `GetAllAsync(sorting, pagination, filters)` — provided by base
- `FindByUserAndCursusAsync(userId, cursusId)` — **new**, composite lookup

Created:
- `IUserCursusService : IDomainService<UserCursus>` + `FindByUserAndCursusAsync`
- `IUserGoalService : IDomainService<UserGoal>` + `FindByUserAndGoalAsync`
- Added `FindByUserAndProjectAsync` to `IUserProjectService`
- Registered all three in DI

---

## Final Architecture

### Controller Map

```
┌─────────────────────────────────────────────────────────────────┐
│  UserController           /users                    [users]     │
│    GET /users             → list all (admin/staff)              │
│    GET /users/{id}        → get by ID                           │
├─────────────────────────────────────────────────────────────────┤
│  AccountController        /account                  [account]   │
│    GET /account           → current user profile                │
│    GET /account/notifications → paginated notifications         │
│    GET /account/stream        → SSE event stream                │
│    GET /account/spotlights    → active spotlights               │
│    DEL /account/spotlights/{id} → dismiss                       │
├─────────────────────────────────────────────────────────────────┤
│  UserCursusController                             [user-cursus] │
│    GET /users/{userId}/cursus                 → list enrollments │
│    GET /users/{userId}/cursus/{cursusId}      → by composite    │
│    GET /user-cursus/{id}                      → by entity ID    │
│    GET /user-cursus/{id}/track                → user track      │
├─────────────────────────────────────────────────────────────────┤
│  UserGoalController                              [user-goals]   │
│    GET /users/{userId}/goals                  → list subs       │
│    GET /users/{userId}/goals/{goalId}         → by composite    │
│    GET /user-goals/{id}                       → by entity ID    │
├─────────────────────────────────────────────────────────────────┤
│  UserProjectController                        [user-projects]   │
│    GET /users/{userId}/projects               → list sessions   │
│    GET /users/{userId}/projects/{projectId}   → by composite    │
│    GET /user-projects/{id}                    → by entity ID    │
│    GET /user-projects/{id}/members            → session members │
├─────────────────────────────────────────────────────────────────┤
│  SubscriptionController   /subscribe          [subscriptions]   │
│    POST /subscribe/{userId}/cursus/{cursusId}   → enroll        │
│    DEL  /subscribe/{userId}/cursus/{cursusId}   → unenroll      │
│    POST /subscribe/{userId}/goals/{goalId}      → enroll        │
│    DEL  /subscribe/{userId}/goals/{goalId}      → unenroll      │
│    POST /subscribe/{userId}/projects/{projId}   → enroll        │
│    DEL  /subscribe/{userId}/projects/{projId}   → unenroll      │
└─────────────────────────────────────────────────────────────────┘
```

### Unchanged Controllers

These controllers were not touched and remain responsible for admin CRUD on the entities themselves:

- **CursusController** (`/cursus`) — cursus CRUD + track management
- **GoalController** (`/goals`) — goal CRUD + project associations
- **ProjectController** (`/projects`) — project CRUD
- **WorkspaceController** (`/workspace`) — workspace management

### Service Layer Additions

```
Core/Services/Interface/
  ├── IUserCursusService.cs    (new)   IDomainService<UserCursus> + FindByUserAndCursusAsync
  ├── IUserGoalService.cs      (new)   IDomainService<UserGoal>   + FindByUserAndGoalAsync
  └── IUserProjectService.cs   (mod)   + FindByUserAndProjectAsync

Core/Services/Implementation/
  ├── UserCursusService.cs     (new)   BaseService<UserCursus>
  ├── UserGoalService.cs       (new)   BaseService<UserGoal>
  └── UserProjectService.cs    (mod)   + FindByUserAndProjectAsync impl
```

---

## Future Considerations

1. **PATCH endpoints on session controllers** — once state transitions become user-facing
   (e.g. marking a goal as complete from the UI), add PATCH endpoints to the session controllers.
2. **Sub-resources** — `UserProject` has transactions and reviews; those could become
   `/user-projects/{id}/transactions` and `/user-projects/{id}/reviews` on the same controller.
3. **Account settings** — `PATCH /account` for profile updates, `PATCH /account/details`
   for user details, SSH key management, etc.
4. **Authorization granularity** — the session controllers currently just use `[Authorize]`;
   consider adding resource-based authorization (e.g., users can only view their own sessions
   unless they're staff).
