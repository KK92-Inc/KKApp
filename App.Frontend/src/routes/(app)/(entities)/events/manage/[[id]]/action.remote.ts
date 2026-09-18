// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as Event from "$lib/remotes/events.remote";
import { Filters, Problem } from '$lib/api';
import type { components } from '$lib/api/api';
import { command, getRequestEvent, query } from '$app/server';
import { useS3Storage } from '$lib/s3';
import { page } from "$app/state";
import { error } from "@sveltejs/kit";

// ============================================================================

const events = useS3Storage({
  bucket: 'events',
  width: 1600,
  height: 600
});

// ============================================================================

export const load = query(Filters.id, async (id) => {
  const { locals } = getRequestEvent();
	const event = await Event.get(id);

	// Check access
	const staff = locals.session.roles.includes("staff");
	if (event.userId === locals.session.userId && !staff)
		error(403);
	return event;
});

// ============================================================================


type CreateEvent = components['schemas']['PostEventRequestDTO'];
export const create = command('unchecked', async (body: CreateEvent) => {
  const { locals } = getRequestEvent();
  const { thumbnail, ...rest } = body;

  const id = Bun.randomUUIDv7();
  let thumbnailUrl: string | null = null;
  if (thumbnail?.startsWith('data:')) {
    thumbnailUrl = await events.write(id, thumbnail).catch((e) => {
      Problem.throw({ message: 'Invalid image', cause: e });
    });
  }

  const { error, data } = await locals.api.POST('/events', {
    body: { ...rest, id, thumbnail: thumbnailUrl }
  });

  if (error || !data) {
    if (thumbnailUrl) await events.delete(id);
    Problem.throw(error);
  }

  return data;
});

type UpdateCursus = { id: string } & components['schemas']['PatchEventRequestDTO'];
export const update = command('unchecked', async (body: UpdateCursus) => {
  const { locals } = getRequestEvent();
  const { id, thumbnail, ...rest } = body;
  let thumbnailUrl = thumbnail;

  if (thumbnail?.startsWith('data:')) {
    thumbnailUrl = await events.write(id, thumbnail).catch((e) => {
      Problem.throw({ message: 'Invalid image', cause: e });
    });
  } else if (thumbnail === null) {
    await events.delete(id);
  }

  const { error, data } = await locals.api.PATCH('/events/{id}', {
    params: { path: { id } },
    body: { ...rest, thumbnail: thumbnailUrl }
  });

  if (error || !data) Problem.throw(error);
  return data;
});
