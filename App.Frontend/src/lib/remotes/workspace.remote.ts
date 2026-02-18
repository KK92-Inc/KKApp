// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, getRequestEvent } from '$app/server';
import { error } from '@sveltejs/kit';
import { mutate, call } from './index.svelte.js';

// ============================================================================

export const getWorkspace = query(async () => {
        const { locals } = getRequestEvent();
        const { data, response } = await locals.api.GET('/workspace/current');
        if (!response.ok || !data) error(response.status, 'Request failed');
        return data;
});

// Transfer Operations
// ============================================================================

const transferSchema = v.object({
        from: v.pipe(v.string(), v.uuid()),
        to: v.pipe(v.string(), v.uuid()),
        ids: v.array(v.pipe(v.string(), v.uuid())),
});

/** Transfer one or more cursus from one workspace to another. */
export const transferCursus = mutate(transferSchema, async (api, body, issue) => {
        await call(
                api.POST('/workspace/{from}/transfer/cursus/{to}', {
                        params: { path: { from: body.from, to: body.to } },
                        body: body.ids,
                }),
                issue
        );
        return {};
});

/** Transfer one or more goals from one workspace to another. */
export const transferGoal = mutate(transferSchema, async (api, body, issue) => {
        await call(
                api.POST('/workspace/{from}/transfer/goal/{to}', {
                        params: { path: { from: body.from, to: body.to } },
                        body: body.ids,
                }),
                issue
        );
        return {};
});

/** Transfer one or more projects from one workspace to another. */
export const transferProject = mutate(transferSchema, async (api, body, issue) => {
        await call(
                api.POST('/workspace/{from}/transfer/project/{to}', {
                        params: { path: { from: body.from, to: body.to } },
                        body: body.ids,
                }),
                issue
        );
        return {};
});
