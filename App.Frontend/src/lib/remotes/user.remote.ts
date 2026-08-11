// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { S3Client } from "bun";
import { query, command, getRequestEvent } from '$app/server';
import { EntityObjectState, EntityType, Filters, paginate, Problem } from '$lib/api';
import { avatars } from '$lib/s3';
import { env } from '$env/dynamic/public';

// ============================================================================
// Schemas
// ============================================================================

const AvatarInput = v.union([
	v.pipe(
		v.instance(File),
		v.minSize(1, 'File is empty'),
		v.maxSize(5 * 1024 * 1024, 'File too large'),
		v.mimeType(['image/png', 'image/jpeg', 'image/gif'], 'Invalid file type')
	),
	v.pipe(v.string(), v.url())
]);

const OptionalText = v.optional(
	v.pipe(
		v.string(),
		v.transform((value) => value.trim()),
		v.transform((value) => (value.length > 0 ? value : null))
	)
);

const DetailsSchema = v.object({
	markdown: OptionalText,
	firstName: OptionalText,
	lastName: OptionalText,
	// TODO: This will require a lot more work that I don't have time for.
	// enabledNotifications: v.optional(v.nullable(v.number())),
	githubUrl: OptionalText,
	linkedinUrl: OptionalText,
	redditUrl: OptionalText,
	websiteUrl: OptionalText
});

const UpdateSchema = v.object({
	userId: Filters.id,
	displayName: OptionalText,
	avatarUrl: v.optional(v.nullable(AvatarInput)),
	details: v.optional(v.nullable(DetailsSchema))
});

const PageSchema = v.object({
	login: v.optional(v.string()),
	display: v.optional(v.string()),
	...Filters.sort,
	...Filters.pagination
});

const EligiblePageSchema = v.object({
	login: v.optional(v.string()),
	display: v.optional(v.string()),
	userId: v.optional(Filters.id),
	type: EntityType,
	id: Filters.id,
	...Filters.sort,
	...Filters.pagination
});

// ============================================================================
// Queries & Commands
// ============================================================================

/** Paginated response for all users */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/users', {
		params: {
			query: {
				'filter[login]': params.login,
				'filter[display]': params.display,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/** Paginated response for all eligible users */
export const getEligiblePage = query(EligiblePageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/users/eligible', {
		params: {
			query: {
				'filter[login]': params.login,
				'filter[display]': params.display,
				'filter[user_id]': params.userId,
				'type[id]': params.id,
				'type[entity]': params.type,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/** Get a single user */
export const get = query(Filters.id, async (userId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/users/{userId}', {
		params: { path: { userId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Update a user's profile */
export const update = command(UpdateSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { avatarUrl, userId, ...rest } = params;

	let avatar: string | null | undefined = undefined;

	if (avatarUrl instanceof File) {
		await avatars.write(userId, avatarUrl);
		avatar = `${env.PUBLIC_S3_ENDPOINT}/avatars/${userId}?v=${Date.now()}`;
	} else if (typeof avatarUrl === 'string') {
		avatar = avatarUrl;
	} else if (avatarUrl === null) {
		avatar = null;
		await avatars.delete(userId).catch(() => { });
	}

	const { error, data } = await locals.api.PATCH('/users/{userId}', {
		params: { path: { userId } },
		body: {
			...(avatar !== undefined ? { avatarUrl: avatar } : {}),
			...rest
		}
	});

	if (error || !data) Problem.throw(error);
	return data;
});
