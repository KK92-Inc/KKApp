// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { S3Client } from "bun";
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';
import { avatars } from '$lib/s3';
import { S3_ENDPOINT } from '$env/static/private';

// ============================================================================

const PageSchema = v.object({
	login: v.optional(v.string()),
	display: v.optional(v.string()),
	...Filters.sort,
	...Filters.pagination
});
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

/** Get a single user */
export const get = query(Filters.id, async (userId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/users/{userId}', {
		params: { path: { userId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

const DetailsSchema = v.object({
	markdown: v.optional(v.nullable(v.pipe(v.string(), v.maxLength(16384)))),
	firstName: v.optional(v.nullable(v.pipe(v.string(), v.minLength(1), v.maxLength(100)))),
	lastName: v.optional(v.nullable(v.pipe(v.string(), v.minLength(1), v.maxLength(100)))),
	enabledNotifications: v.optional(v.number()),
	githubUrl: v.optional(v.nullable(v.pipe(v.string(), v.url()))),
	linkedinUrl: v.optional(v.nullable(v.pipe(v.string(), v.url()))),
	redditUrl: v.optional(v.nullable(v.pipe(v.string(), v.url()))),
	websiteUrl: v.optional(v.nullable(v.pipe(v.string(), v.url())))
});

const AvatarInput = v.union([
	v.pipe(
		v.instance(File),
		v.minSize(1, 'File is empty'),
		v.maxSize(5 * 1024 * 1024, 'File too large'),
		v.mimeType(['image/png', 'image/jpeg', 'image/gif'], 'Invalid file type')
	),
	v.pipe(v.string(), v.url()) // unchanged — existing avatar URL passed back as-is
]);

const UpdateSchema = v.object({
	userId: Filters.id,
	displayName: v.optional(v.nullable(v.pipe(v.string(), v.minLength(1), v.maxLength(100)))),
	avatarUrl: v.optional(v.nullable(AvatarInput)),
	details: v.optional(v.nullable(DetailsSchema))
});

/** Update a user's profile */
export const update = command(UpdateSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { avatarUrl, userId, ...rest } = params;

	let avatar: string | null | undefined = undefined;

	if (avatarUrl instanceof File) {
		await avatars.write(userId, avatarUrl);
		avatar = `${S3_ENDPOINT}/avatars/${userId}`; // stable, no expiry
	} else if (typeof avatarUrl === 'string') {
		avatar = avatarUrl;
	} else if (avatarUrl === null) {
		avatar = null;
		await avatars.delete(userId).catch(() => {});
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
