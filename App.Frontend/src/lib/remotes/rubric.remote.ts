// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api';

// ============================================================================

const PageSchema = v.object({
	id: v.optional(Filters.id),
	name: v.optional(v.string()),
	slug: v.optional(v.string()),
	enabled: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	creatorId: v.optional(Filters.id),
	kind: v.optional(v.number()),
	...Filters.pagination,
	...Filters.sort,
});

const UpdateSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	markdown: v.optional(v.string()),
	public: v.optional(v.boolean()),
	enabled: v.optional(v.boolean())
});

const VariantSchema = v.object({
	kind: v.number(), // Flag
	required: v.pipe(v.number(), v.minValue(0), v.maxValue(100))
});

const SetVariantsSchema = v.object({
	id: Filters.id,
	variants: v.pipe(v.array(VariantSchema), v.minLength(1))
});

// ============================================================================

/** Get a single rubric */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/rubrics/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for all rubrics */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/rubrics', {
		params: {
			query: {
				'filter[id]': params.id,
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'filter[enabled]': params.enabled,
				'filter[public]': params.public,
				'filter[creator_id]': params.creatorId,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Update a rubric's own fields (use setVariants to change its variants) */
export const update = command(UpdateSchema, async ({ id, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH('/rubrics/{id}', {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Replace the review variant requirements for a rubric */
export const setVariants = command(SetVariantsSchema, async ({ id, variants }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PUT('/rubrics/{id}/variants', {
		params: { path: { id } },
		body: { variants }
	});

	if (error || !data) Problem.throw(error);
	return data;
});
