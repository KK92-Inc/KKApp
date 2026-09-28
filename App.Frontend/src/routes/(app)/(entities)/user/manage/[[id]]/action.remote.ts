// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Problem } from '$lib/api';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';
import { useS3Storage } from '$lib/s3';

// ============================================================================

const avatars = useS3Storage({
	bucket: 'avatars',
	width: 256,
	height: 256
});

// ============================================================================

type CreateUser = components['schemas']['PostUserRequestDTO'];
export const create = command('unchecked', async (body: CreateUser) => {
	const { locals } = getRequestEvent();
	const { avatarUrl, ...rest } = body;

	const id = Bun.randomUUIDv7();
	let thumbnailUrl: string | null = null;
	if (avatarUrl?.startsWith('data:')) {
		thumbnailUrl = await avatars.write(id, avatarUrl).catch((e) => {
			Problem.throw({ message: 'Invalid image', cause: e });
		});
	}

	const { error, data } = await locals.api.POST("/users", {
		body: { ...rest, id, thumbnail: thumbnailUrl }
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateProject = { id: string; } & components['schemas']['PatchUserRequestDTO'];
export const update = command('unchecked', async (body: UpdateProject) => {
	const { locals } = getRequestEvent();
	const { id, avatarUrl, ...rest } = body;
	let thumbnailUrl = avatarUrl;

	if (avatarUrl?.startsWith('data:')) {
		thumbnailUrl = await avatars.write(id, avatarUrl).catch((e) => {
			Problem.throw({ message: 'Invalid image', cause: e });
		});
	} else if (avatarUrl === null) {
		await avatars.delete(id);
	}

	const { error, data } = await locals.api.PATCH("/users/{userId}", {
		params: { path: { userId: id } },
    body: { ...rest, thumbnail: thumbnailUrl }
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});
