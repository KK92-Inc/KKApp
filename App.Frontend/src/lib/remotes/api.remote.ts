// ============================================================================
// AUTO-GENERATED REMOTE FUNCTIONS
// Do not edit manually. Re-run generate-remotes.ts to update.
// ============================================================================

import * as v from 'valibot';
import { query, command, form, getRequestEvent } from '$app/server';
import { Problem } from './index.svelte.js'; // Adjust path based on location

// --- SCHEMAS ---
export const AnnotationDataSchema = v.union([v.lazy(() => AnnotationDataTextAnnotationDataSchema), v.lazy(() => AnnotationDataDrawingAnnotationDataSchema), v.lazy(() => AnnotationDataSuggestionAnnotationDataSchema)]);

export const AnnotationDataDrawingAnnotationDataSchema = v.any();

export const AnnotationDataSuggestionAnnotationDataSchema = v.any();

export const AnnotationDataTextAnnotationDataSchema = v.any();

export const AnnotationDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'filePath': v.string(), 'data': v.union([v.any(), v.lazy(() => AnnotationDataSchema)]), 'reviewId': v.pipe(v.string(), v.uuid()), 'author': v.lazy(() => UserLightDOSchema) });

export const CompletionModeSchema = v.picklist(['Ring', 'FreeStyle']);

export const CursusDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'description': v.string(), 'slug': v.string(), 'variant': v.lazy(() => CursusVariantSchema), 'completionMode': v.lazy(() => CompletionModeSchema), 'workspace': v.lazy(() => WorkspaceDOSchema) });

export const CursusTrackDOSchema = v.object({ 'cursusId': v.pipe(v.string(), v.uuid()), 'variant': v.lazy(() => CursusVariantSchema), 'completionMode': v.lazy(() => CompletionModeSchema), 'nodes': v.optional(v.array(v.lazy(() => CursusTrackNodeDOSchema))) });

export const CursusTrackNodeDOSchema = v.object({ 'goal': v.lazy(() => GoalLightDOSchema), 'choiceGroup': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))), 'children': v.optional(v.array(v.lazy(() => CursusTrackNodeDOSchema))) });

export const CursusTrackNodeDTOSchema = v.object({ 'goalId': v.pipe(v.string(), v.uuid()), 'parentId': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))), 'group': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))) });

export const CursusVariantSchema = v.picklist(['Dynamic', 'Static', 'Partial']);

export const EntityObjectStateSchema = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

export const EntityOwnershipSchema = v.picklist(['User', 'Organization']);

export const GitDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'owner': v.string(), 'ownership': v.lazy(() => EntityOwnershipSchema) });

export const GoalDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'description': v.string(), 'slug': v.string(), 'active': v.boolean(), 'public': v.boolean(), 'deprecated': v.boolean(), 'workspace': v.lazy(() => WorkspaceDOSchema) });

export const GoalLightDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'slug': v.string(), 'active': v.boolean(), 'deprecated': v.boolean() });

export const MemberDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'entityId': v.pipe(v.string(), v.uuid()), 'entityType': v.lazy(() => MemberEntityTypeSchema), 'userId': v.pipe(v.string(), v.uuid()), 'role': v.lazy(() => MemberRoleSchema), 'leftAt': v.nullable(v.string()), 'user': v.lazy(() => UserLightDOSchema) });

export const MemberEntityTypeSchema = v.picklist(['Workspace', 'UserProject']);

export const MemberRoleSchema = v.picklist(['Pending', 'Member', 'Leader']);

export const NotificationDataSchema = v.union([v.lazy(() => NotificationDataMessageDOSchema), v.lazy(() => NotificationDataProjectInviteDataSchema)]);

export const NotificationDataMessageDOSchema = v.any();

export const NotificationDataProjectInviteDataSchema = v.any();

export const NotificationDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'data': v.lazy(() => NotificationDataSchema), 'descriptor': v.lazy(() => NotificationMetaSchema), 'readAt': v.nullable(v.string()), 'resourceId': v.nullable(v.pipe(v.string(), v.uuid())) });

export const NotificationMetaSchema = v.number();

export const OrderSchema = v.picklist(['Ascending', 'Descending']);

export const PatchApplicationRequestDTOSchema = v.object({ 'name': v.optional(v.nullable(v.string())), 'description': v.optional(v.nullable(v.string())), 'redirectUris': v.optional(v.nullable(v.array(v.string()))) });

export const PatchGoalRequestDTOSchema = v.object({ 'name': v.optional(v.nullable(v.string())), 'description': v.optional(v.nullable(v.string())), 'active': v.optional(v.nullable(v.boolean())), 'public': v.optional(v.nullable(v.boolean())), 'projects': v.array(v.pipe(v.string(), v.uuid())) });

export const PatchProjectRequestDTOSchema = v.object({ 'name': v.optional(v.nullable(v.string())), 'description': v.optional(v.nullable(v.string())), 'active': v.optional(v.nullable(v.boolean())), 'public': v.optional(v.nullable(v.boolean())), 'maxMembers': v.optional(v.nullable(v.union([v.number(), v.string()]))) });

export const PatchRubricEntityRequestDTOSchema = v.object({ 'name': v.optional(v.nullable(v.string())), 'markdown': v.optional(v.nullable(v.string())), 'public': v.optional(v.nullable(v.boolean())), 'enabled': v.optional(v.nullable(v.boolean())) });

export const PatchUserDetailsRequestDTOSchema = v.object({ 'markdown': v.optional(v.nullable(v.string())), 'firstName': v.optional(v.nullable(v.string())), 'lastName': v.optional(v.nullable(v.string())), 'enabledNotifications': v.optional(v.union([v.any(), v.lazy(() => NotificationMetaSchema)])), 'githubUrl': v.optional(v.nullable(v.string())), 'linkedinUrl': v.optional(v.nullable(v.string())), 'redditUrl': v.optional(v.nullable(v.string())), 'websiteUrl': v.optional(v.nullable(v.string())) });

export const PatchUserRequestDTOSchema = v.object({ 'displayName': v.optional(v.nullable(v.string())), 'avatarUrl': v.optional(v.nullable(v.string())), 'details': v.optional(v.union([v.any(), v.lazy(() => PatchUserDetailsRequestDTOSchema)])) });

export const Point2DSchema = v.object({ 'x': v.union([v.number(), v.string()]), 'y': v.union([v.number(), v.string()]) });

export const PostApplicationRequestDTOSchema = v.object({ 'name': v.string(), 'enabled': v.boolean(), 'description': v.string(), 'redirectUris': v.optional(v.array(v.string())) });

export const PostCursusRequestDTOSchema = v.object({ 'name': v.string(), 'description': v.string(), 'active': v.optional(v.boolean()), 'public': v.optional(v.boolean()), 'variant': v.optional(v.lazy(() => CursusVariantSchema)), 'completionMode': v.optional(v.lazy(() => CompletionModeSchema)) });

export const PostCursusTrackRequestDTOSchema = v.object({ 'nodes': v.array(v.lazy(() => CursusTrackNodeDTOSchema)) });

export const PostGoalRequestDTOSchema = v.object({ 'name': v.string(), 'description': v.string(), 'active': v.optional(v.boolean()), 'public': v.optional(v.boolean()), 'projects': v.array(v.pipe(v.string(), v.uuid())) });

export const PostProjectRequestDTOSchema = v.object({ 'name': v.string(), 'description': v.string(), 'active': v.boolean(), 'public': v.boolean(), 'maxMembers': v.union([v.number(), v.string()]), 'files': v.array(v.lazy(() => ProjectInitialFilesRequestDTOSchema)) });

export const PostReviewRequestDTOSchema = v.object({ 'userProjectId': v.pipe(v.string(), v.uuid()), 'ref': v.string() });

export const PostRubricEntityRequestDTOSchema = v.object({ 'name': v.string(), 'markdown': v.optional(v.nullable(v.string())), 'public': v.optional(v.boolean()), 'projectId': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))), 'enabled': v.optional(v.boolean()), 'variants': v.array(v.lazy(() => RubricVariantDTOSchema)) });

export const PostSshKeyRequestDTOSchema = v.object({ 'title': v.string(), 'publicKey': v.string() });

export const ProblemDetailsSchema = v.object({ 'type': v.optional(v.nullable(v.string())), 'title': v.optional(v.nullable(v.string())), 'status': v.optional(v.nullable(v.union([v.number(), v.string()]))), 'detail': v.optional(v.nullable(v.string())), 'instance': v.optional(v.nullable(v.string())) });

export const ProjectDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'gitInfo': v.lazy(() => GitDOSchema), 'workspace': v.lazy(() => WorkspaceDOSchema), 'name': v.string(), 'description': v.string(), 'slug': v.string(), 'active': v.boolean(), 'public': v.boolean(), 'deprecated': v.boolean(), 'maxMembers': v.union([v.number(), v.string()]) });

export const ProjectInitialFilesRequestDTOSchema = v.object({ 'path': v.string(), 'content': v.string() });

export const ProjectLightDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'description': v.string(), 'slug': v.string(), 'active': v.boolean(), 'public': v.boolean(), 'deprecated': v.boolean(), 'maxMembers': v.union([v.number(), v.string()]) });

export const PutRubricVariantsRequestDTOSchema = v.object({ 'variants': v.array(v.lazy(() => RubricVariantDTOSchema)) });

export const ReviewDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'kind': v.lazy(() => ReviewKindsSchema), 'state': v.lazy(() => ReviewStateSchema), 'userProjectId': v.pipe(v.string(), v.uuid()), 'reviewer': v.union([v.any(), v.lazy(() => UserLightDOSchema)]), 'rubric': v.lazy(() => RubricLightDOSchema) });

export const ReviewKindsSchema = v.string();

export const ReviewProgressDOSchema = v.object({ 'rubric': v.lazy(() => RubricLightDOSchema), 'variants': v.array(v.lazy(() => ReviewVariantProgressDOSchema)) });

export const ReviewStateSchema = v.picklist(['Pending', 'InProgress', 'Finished', 'Cancelled']);

export const ReviewVariantProgressDOSchema = v.object({ 'kind': v.lazy(() => ReviewKindsSchema), 'required': v.union([v.number(), v.string()]), 'finished': v.union([v.number(), v.string()]), 'active': v.union([v.number(), v.string()]) });

export const RubricDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'slug': v.string(), 'public': v.boolean(), 'enabled': v.boolean(), 'variants': v.array(v.lazy(() => RubricVariantDOSchema)), 'projectId': v.nullable(v.pipe(v.string(), v.uuid())), 'creator': v.lazy(() => UserLightDOSchema), 'gitInfo': v.union([v.any(), v.lazy(() => GitDOSchema)]) });

export const RubricLightDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'name': v.string(), 'slug': v.string(), 'public': v.boolean(), 'enabled': v.boolean(), 'gitInfo': v.union([v.any(), v.lazy(() => GitDOSchema)]) });

export const RubricVariantDOSchema = v.object({ 'kind': v.lazy(() => ReviewKindsSchema), 'requires': v.union([v.number(), v.string()]) });

export const RubricVariantDTOSchema = v.object({ 'kind': v.lazy(() => ReviewKindsSchema), 'required': v.union([v.number(), v.string()]) });

export const SpotlightNotificationDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'title': v.string(), 'description': v.string(), 'actionText': v.string(), 'href': v.string(), 'backgroundUrl': v.string(), 'startsAt': v.string(), 'endsAt': v.nullable(v.string()) });

export const SshKeyResponseDOSchema = v.object({ 'title': v.string(), 'fingerprint': v.string(), 'keyType': v.string(), 'createdAt': v.string() });

export const TextRangeSchema = v.object({ 'startLine': v.union([v.number(), v.string()]), 'startColumn': v.union([v.number(), v.string()]), 'endLine': v.union([v.number(), v.string()]), 'endColumn': v.union([v.number(), v.string()]) });

export const UserCursusDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'state': v.lazy(() => EntityObjectStateSchema), 'unlocksAt': v.nullable(v.string()), 'cursus': v.lazy(() => CursusDOSchema), 'user': v.union([v.any(), v.lazy(() => UserLightDOSchema)]) });

export const UserCursusTrackDOSchema = v.object({ 'cursusId': v.pipe(v.string(), v.uuid()), 'name': v.string(), 'completionMode': v.lazy(() => CompletionModeSchema), 'nodes': v.array(v.lazy(() => UserCursusTrackNodeDOSchema)) });

export const UserCursusTrackNodeDOSchema = v.object({ 'goalId': v.pipe(v.string(), v.uuid()), 'name': v.string(), 'slug': v.string(), 'isUnlocked': v.boolean(), 'parentGoalId': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))), 'choiceGroup': v.optional(v.nullable(v.pipe(v.string(), v.uuid()))), 'state': v.optional(v.union([v.any(), v.lazy(() => EntityObjectStateSchema)])) });

export const UserDetailsDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'email': v.nullable(v.string()), 'markdown': v.nullable(v.string()), 'firstName': v.nullable(v.string()), 'lastName': v.nullable(v.string()), 'githubUrl': v.nullable(v.string()), 'linkedinUrl': v.nullable(v.string()), 'redditUrl': v.nullable(v.string()), 'websiteUrl': v.nullable(v.string()) });

export const UserDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'login': v.string(), 'displayName': v.optional(v.nullable(v.string())), 'avatarUrl': v.optional(v.nullable(v.string())), 'details': v.optional(v.union([v.any(), v.lazy(() => UserDetailsDOSchema)])) });

export const UserGoalDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'userId': v.pipe(v.string(), v.uuid()), 'goalId': v.pipe(v.string(), v.uuid()), 'state': v.lazy(() => EntityObjectStateSchema), 'goal': v.optional(v.lazy(() => GoalDOSchema)), 'user': v.optional(v.union([v.any(), v.lazy(() => UserLightDOSchema)])) });

export const UserLightDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'login': v.string(), 'displayName': v.nullable(v.string()), 'avatarUrl': v.nullable(v.string()) });

export const UserProjectDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'state': v.lazy(() => EntityObjectStateSchema), 'project': v.lazy(() => ProjectLightDOSchema), 'gitInfo': v.union([v.any(), v.lazy(() => GitDOSchema)]) });

export const UserProjectTransactionDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'userProjectId': v.pipe(v.string(), v.uuid()), 'userId': v.nullable(v.pipe(v.string(), v.uuid())), 'type': v.lazy(() => UserProjectTransactionVariantSchema), 'user': v.union([v.any(), v.lazy(() => UserLightDOSchema)]) });

export const UserProjectTransactionVariantSchema = v.picklist(['Started', 'MemberJoined', 'MemberLeft', 'GitCommit', 'StateChangedToInActive', 'StateChangedToActive', 'StateChangedToCompleted', 'StateChangedToAwaiting', 'MemberInvited', 'MemberUninvited', 'MemberAccepted', 'MemberDeclined', 'MemberKicked', 'LeadershipTransferred']);

export const WorkspaceDOSchema = v.object({ 'id': v.pipe(v.string(), v.uuid()), 'createdAt': v.string(), 'updatedAt': v.string(), 'owner': v.union([v.any(), v.lazy(() => UserLightDOSchema)]), 'ownership': v.lazy(() => EntityOwnershipSchema) });

// --- ENDPOINTS ---
export const getAccount = query(
  v.object({
    
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.GET('/account', {
      params: {
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getAccountNotifications = query(
  v.object({
    'filter[read]': v.optional(v.boolean()),
    'filter[variant]': v.optional(v.lazy(() => NotificationMetaSchema)),
    'filter[not[variant]]': v.optional(v.lazy(() => NotificationMetaSchema)),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[read]', 'filter[variant]', 'filter[not[variant]]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/account/notifications', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getAccountStream = query(
  v.object({
    
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.GET('/account/stream', {
      params: {
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getAccountSpotlights = query(
  v.object({
    'filter[id]': v.optional(v.pipe(v.string(), v.uuid())),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[id]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/account/spotlights', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteAccountSpotlightsById = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/account/spotlights/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getAccountSshKeys = query(
  v.object({
    
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.GET('/account/ssh-keys', {
      params: {
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postAccountSshKeys = form(
  v.object({ body: v.lazy(() => PostSshKeyRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.POST('/account/ssh-keys', {
      params: {
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteAccountSshKeysByFingerprint = command(
  v.object({
    'fingerprint': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['fingerprint'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getCursus = query(
  v.object({
    'filter[id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[name]': v.optional(v.string()),
    'filter[slug]': v.optional(v.string()),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[id]', 'filter[name]', 'filter[slug]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/cursus', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteCursusById = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/cursus/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getCursusById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/cursus/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getCursusByIdTrack = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/cursus/{id}/track', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postCursusByIdTrack = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostCursusTrackRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/cursus/{id}/track', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGitByIdBranches = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/git/{id}/branches', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGitByIdTreeByBranch = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'branch': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'branch'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/git/{id}/tree/{branch}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGitByIdTreeByBranchByPath = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'branch': v.string(),
    'path': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'branch', 'path'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/git/{id}/tree/{branch}/{path}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByIdProjectsByProjectIdTreeByBranch = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid()),
    'branch': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'projectId', 'branch'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{id}/projects/{projectId}/tree/{branch}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByIdProjectsByProjectIdTreeByBranchByPath = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid()),
    'branch': v.string(),
    'path': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'projectId', 'branch', 'path'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{id}/projects/{projectId}/tree/{branch}/{path}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGitByIdBlobByBranchByPath = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'branch': v.string(),
    'path': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'branch', 'path'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/git/{id}/blob/{branch}/{path}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByIdProjectsByProjectIdBlobByBranchByPath = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid()),
    'branch': v.string(),
    'path': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'projectId', 'branch', 'path'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{id}/projects/{projectId}/blob/{branch}/{path}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postGitByIdBranchesByRefByChild = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'ref': v.string(),
    'child': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'ref', 'child'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/git/{id}/branches/{ref}/{child}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteGitByIdBranchesByBranch = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'branch': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'branch'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/git/{id}/branches/{branch}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postGitByIdLock = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/git/{id}/lock', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postGitByIdUnlock = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/git/{id}/unlock', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGoals = query(
  v.object({
    'filter[name]': v.optional(v.string()),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[name]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/goals', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteGoalsById = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/goals/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGoalsById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/goals/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const patchGoalsById = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PatchGoalRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PATCH('/goals/{id}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getGoalsByIdProjects = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/goals/{id}/projects', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postGoalsByIdProjects = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.array(v.pipe(v.string(), v.uuid())) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/goals/{id}/projects', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getProjects = query(
  v.object({
    'filter[id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[name]': v.optional(v.string()),
    'filter[slug]': v.optional(v.string()),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[id]', 'filter[name]', 'filter[slug]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/projects', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteProjectsById = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/projects/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getProjectsById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/projects/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const patchProjectsById = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PatchProjectRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PATCH('/projects/{id}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getProjectsByIdRubrics = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
      if (['sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/projects/{id}/rubrics', {
      params: {
        path: pathParams as any,
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getReviews = query(
  v.object({
    'filter[user_project_id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[reviewer_id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[rubric_id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[kind]': v.optional(v.lazy(() => ReviewKindsSchema)),
    'filter[status]': v.optional(v.lazy(() => ReviewStateSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[user_project_id]', 'filter[reviewer_id]', 'filter[rubric_id]', 'filter[kind]', 'filter[status]', 'page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/reviews', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postReviews = form(
  v.object({ body: v.lazy(() => PostReviewRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.POST('/reviews', {
      params: {
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getReviewsByReviewId = query(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/reviews/{reviewId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteReviewsByReviewId = command(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/reviews/{reviewId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getReviewsByReviewIdByFileAnnotations = query(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid()),
    'file': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId', 'file'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/reviews/{reviewId}/{file}/annotations', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const putReviewsByReviewIdByFileAnnotations = command(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid()),
    'file': v.string()
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId', 'file'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PUT('/reviews/{reviewId}/{file}/annotations', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getReviewsUserProjectByUserProjectIdStatus = query(
  v.object({
    'userProjectId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userProjectId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/reviews/user-project/{userProjectId}/status', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postReviewsByReviewIdAssignByReviewerId = command(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid()),
    'reviewerId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId', 'reviewerId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/reviews/{reviewId}/assign/{reviewerId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postReviewsByReviewIdStart = command(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/reviews/{reviewId}/start', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postReviewsByReviewIdComplete = command(
  v.object({
    'reviewId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['reviewId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/reviews/{reviewId}/complete', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getRubrics = query(
  v.object({
    'filter[id]': v.optional(v.pipe(v.string(), v.uuid())),
    'filter[name]': v.optional(v.string()),
    'filter[slug]': v.optional(v.string()),
    'filter[enabled]': v.optional(v.boolean()),
    'filter[public]': v.optional(v.boolean()),
    'filter[creator_id]': v.optional(v.pipe(v.string(), v.uuid())),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()]))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[id]', 'filter[name]', 'filter[slug]', 'filter[enabled]', 'filter[public]', 'filter[creator_id]', 'sort[by]', 'sort[order]', 'page[index]', 'page[size]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/rubrics', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getRubricsById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/rubrics/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const patchRubricsById = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PatchRubricEntityRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PATCH('/rubrics/{id}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteRubricsById = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/rubrics/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const putRubricsByIdVariants = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PutRubricVariantsRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PUT('/rubrics/{id}/variants', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postSubscribeByUserIdCursusByCursusId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'cursusId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'cursusId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/subscribe/{userId}/cursus/{cursusId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteSubscribeByUserIdCursusByCursusId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'cursusId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'cursusId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/subscribe/{userId}/cursus/{cursusId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postSubscribeByUserIdGoalsByGoalId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'goalId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'goalId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/subscribe/{userId}/goals/{goalId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteSubscribeByUserIdGoalsByGoalId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'goalId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'goalId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/subscribe/{userId}/goals/{goalId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postSubscribeByUserIdProjectsByProjectId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'projectId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/subscribe/{userId}/projects/{projectId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteSubscribeByUserIdProjectsByProjectId = command(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'projectId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/subscribe/{userId}/projects/{projectId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsers = query(
  v.object({
    'filter[login]': v.optional(v.string()),
    'filter[display]': v.optional(v.string()),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['filter[login]', 'filter[display]', 'page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users', {
      params: {
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserId = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const patchUsersByUserId = form(
  v.object({ 'userId': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PatchUserRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PATCH('/users/{userId}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdCursus = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'filter[name]': v.optional(v.string()),
    'filter[state]': v.optional(v.lazy(() => EntityObjectStateSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId'].includes(key)) { pathParams[key] = value; continue; }
      if (['filter[name]', 'filter[state]', 'page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/cursus', {
      params: {
        path: pathParams as any,
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdCursusByCursusId = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'cursusId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'cursusId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/cursus/{cursusId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUserCursusById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/user-cursus/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUserCursusByIdTrack = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/user-cursus/{id}/track', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdGoals = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'filter[name]': v.optional(v.string()),
    'filter[state]': v.optional(v.lazy(() => EntityObjectStateSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId'].includes(key)) { pathParams[key] = value; continue; }
      if (['filter[name]', 'filter[state]', 'page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/goals', {
      params: {
        path: pathParams as any,
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdGoalsByGoalId = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'goalId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'goalId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/goals/{goalId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUserGoalsById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/user-goals/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdProjects = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'filter[name]': v.optional(v.string()),
    'filter[slug]': v.optional(v.string()),
    'filter[state]': v.optional(v.lazy(() => EntityObjectStateSchema)),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId'].includes(key)) { pathParams[key] = value; continue; }
      if (['filter[name]', 'filter[slug]', 'filter[state]', 'page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/projects', {
      params: {
        path: pathParams as any,
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUsersByUserIdProjectsByProjectId = query(
  v.object({
    'userId': v.pipe(v.string(), v.uuid()),
    'projectId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['userId', 'projectId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/users/{userId}/projects/{projectId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUserProjectsById = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/user-projects/{id}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getUserProjectsByIdTransactions = query(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'page[index]': v.optional(v.union([v.number(), v.string()])),
    'page[size]': v.optional(v.union([v.number(), v.string()])),
    'sort[by]': v.optional(v.string()),
    'sort[order]': v.optional(v.lazy(() => OrderSchema))
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
      if (['page[index]', 'page[size]', 'sort[by]', 'sort[order]'].includes(key)) { queryParams[key] = value; continue; }
    }

    const res = await locals.api.GET('/user-projects/{id}/transactions', {
      params: {
        path: pathParams as any,
        query: queryParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postUserProjectsByIdInviteByUserId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'userId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/user-projects/{id}/invite/{userId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteUserProjectsByIdInviteByUserId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'userId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/user-projects/{id}/invite/{userId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postUserProjectsByIdInviteAccept = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/user-projects/{id}/invite/accept', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postUserProjectsByIdInviteDecline = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/user-projects/{id}/invite/decline', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const putUserProjectsByIdMemberTransferByNewLeaderId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'newLeaderId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'newLeaderId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PUT('/user-projects/{id}/member/transfer/{newLeaderId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postUserProjectsByIdMemberLeave = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/user-projects/{id}/member/leave', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postUserProjectsByIdMemberKickByMemberId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'memberId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'memberId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/user-projects/{id}/member/kick/{memberId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getWorkspaceCurrent = query(
  v.object({
    
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.GET('/workspace/current', {
      params: {
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const getWorkspaceRoot = query(
  v.object({
    
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
    }

    const res = await locals.api.GET('/workspace/root', {
      params: {
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByWorkspaceCursus = form(
  v.object({ 'workspace': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostCursusRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['workspace'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{workspace}/cursus', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByWorkspaceGoal = form(
  v.object({ 'workspace': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostGoalRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['workspace'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{workspace}/goal', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByWorkspaceProject = form(
  v.object({ 'workspace': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostProjectRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['workspace'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{workspace}/project', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdRubric = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostRubricEntityRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/rubric', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdApplication = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PostApplicationRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/application', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const patchWorkspaceByIdApplicationByAppId = form(
  v.object({ 'id': v.pipe(v.string(), v.uuid()), 'appId': v.pipe(v.string(), v.uuid()), body: v.lazy(() => PatchApplicationRequestDTOSchema) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'appId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.PATCH('/workspace/{id}/application/{appId}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteWorkspaceByIdApplicationByAppId = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'appId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'appId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/workspace/{id}/application/{appId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdApplicationByAppIdSecretRotate = command(
  v.object({
    'id': v.pipe(v.string(), v.uuid()),
    'appId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['id', 'appId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/application/{appId}/secret/rotate', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByFromTransferCursusByTo = form(
  v.object({ 'from': v.pipe(v.string(), v.uuid()), 'to': v.pipe(v.string(), v.uuid()), body: v.array(v.pipe(v.string(), v.uuid())) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['from', 'to'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{from}/transfer/cursus/{to}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByFromTransferGoalByTo = form(
  v.object({ 'from': v.pipe(v.string(), v.uuid()), 'to': v.pipe(v.string(), v.uuid()), body: v.array(v.pipe(v.string(), v.uuid())) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['from', 'to'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{from}/transfer/goal/{to}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByFromTransferProjectByTo = form(
  v.object({ 'from': v.pipe(v.string(), v.uuid()), 'to': v.pipe(v.string(), v.uuid()), body: v.array(v.pipe(v.string(), v.uuid())) }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['from', 'to'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{from}/transfer/project/{to}', {
      params: {
        path: pathParams as any,
      },
      body: (input as any).body,
    });

    if (res.error) Problem.validate(res.error);
    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdInviteByUserId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'userId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/invite/{userId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const deleteWorkspaceByIdInviteByUserId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'userId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'userId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.DELETE('/workspace/{id}/invite/{userId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdInviteAccept = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/invite/accept', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdInviteDecline = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/invite/decline', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdMemberLeave = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/member/leave', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

export const postWorkspaceByIdMemberKickByMemberId = command(
  v.object({
    'Id': v.pipe(v.string(), v.uuid()),
    'memberId': v.pipe(v.string(), v.uuid())
  }),
  async (input) => {
    const { locals } = getRequestEvent();
    const pathParams = {};
    const queryParams = {};
    for (const [key, value] of Object.entries(input)) {
      if (['Id', 'memberId'].includes(key)) { pathParams[key] = value; continue; }
    }

    const res = await locals.api.POST('/workspace/{id}/member/kick/{memberId}', {
      params: {
        path: pathParams as any,
      },
    });

    if (res.error) Problem.throw(res.error);
    return res.data;
  }
);

