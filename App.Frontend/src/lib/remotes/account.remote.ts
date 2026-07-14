// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { command, form, getRequestEvent, query } from '$app/server';
import { Keycloak } from '$lib/auth';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const PageSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	read: v.optional(v.boolean()),
	variant: v.optional(v.number()),
	notVariant: v.optional(v.number()),
});

const KeySchema = v.object({
	title: v.string(),
	key: v.string(),
});

// ============================================================================

/** Remote to sign-in */
export const login = form(() => Keycloak.signIn());
/** Remote to sign-out */
export const logout = form(async () => await Keycloak.signOut());

// ============================================================================

/** Get a single goal */
export const get = query(async () => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/account");
	if (error || !data) Problem.throw(error)
	return data;
});

/** Paginated response for all notifications */
export const getNotificationPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET("/account/notifications", {
		params: {
			query: {
				"filter[read]": params.read,
				"filter[variant]": params.variant,
				"filter[not[variant]]": params.notVariant,
				"page[index]": params.page,
				"page[size]": params.size,
				"sort[by]": params.sortBy,
				"sort[order]": params.sort,
			}
		}
	});

	if (error) Problem.throw(error)
	return paginate(data, response);
});

// ============================================================================
// SSH
// ============================================================================

export const getKeys = query(async () => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/account/ssh-keys");
	if (error || !data) Problem.throw(error)
	return data;
});

export const addKey = command(KeySchema, async ({ key, title }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/account/ssh-keys", {
		body: { title, publicKey: key }
	});

	if (error) Problem.throw(error)
});

export const deleteKey = command(v.string(), async (fingerprint) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE("/account/ssh-keys/{fingerprint}", {
		params: { path: { fingerprint }}
	});

	if (error) Problem.throw(error)
});

// ============================================================================
// Spotlight
// ============================================================================

export const getSpotlights = query(async () => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/account/spotlights");
	if (error || !data) Problem.throw(error)
	return data;
});

export const dismissSpotlight = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE("/account/spotlights/{id}", {
		params: { path: { id }}
	});

	if (error) Problem.throw(error)
});
