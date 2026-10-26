// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';
import type { components } from '$lib/api/api';

// ============================================================================
// Schemas
// ============================================================================

const PageSchema = v.object({
	name: v.optional(v.string()),
	...Filters.pagination
});

const RegistrationsSchema = v.object({
	id: Filters.id,
	email: v.optional(v.string()),
	...Filters.pagination
});

// ============================================================================
// Queries & Commands
// ============================================================================

/** Paginated response for the check-ins, the newest start first. */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();

	const { response, data, error } = await locals.api.GET('/checkins', {
		params: {
			query: {
				'filter[name]': params.name || undefined,
				'sort[by]': 'StartsAt',
				'sort[order]': 'Descending',
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

type CreateCheckIn = components['schemas']['PostApplyEventRequestDTO'];
/** Create a check-in */
export const create = command('unchecked', async (body: CreateCheckIn) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/checkins', { body });

	if (error || !data) Problem.throw(error);
	return data;
});

type UpdateCheckIn = { id: string } & components['schemas']['PatchApplyEventRequestDTO'];
/** Update a check-in, omitted fields are left alone */
export const update = command('unchecked', async ({ id, ...body }: UpdateCheckIn) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH('/checkins/{id}', {
		params: { path: { id } },
		body
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Delete a check-in, the backend refuses once anyone was verified as present */
export const remove = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/checkins/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

/** Who signed up for a check-in, optionally narrowed down by (a part of) their email. */
export const getRegistrations = query(RegistrationsSchema, async (params) => {
	const { locals } = getRequestEvent();

	const { response, data, error } = await locals.api.GET('/checkins/{id}/registrations', {
		params: {
			path: { id: params.id },
			query: {
				'filter[email]': params.email || undefined,
				'sort[by]': 'Email',
				'sort[order]': 'Ascending',
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

type Attendance = { id: string; registrationId: string; attended: boolean };
/** Record, or take back, that a staff member saw someone at the check-in. */
export const setAttendance = command('unchecked', async ({ id, registrationId, attended }: Attendance) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PUT('/checkins/{id}/registrations/{registrationId}/attendance', {
		params: { path: { id, registrationId } },
		body: { attended }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

type Unregister = { id: string; registrationId: string };
/** Remove a sign-up, the backend refuses once the person was verified as present */
export const unregister = command('unchecked', async ({ id, registrationId }: Unregister) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/checkins/{id}/registrations/{registrationId}', {
		params: { path: { id, registrationId } }
	});

	if (error) Problem.throw(error);
});
