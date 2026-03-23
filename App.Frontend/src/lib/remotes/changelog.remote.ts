import { prerender } from "$app/server";

export const get = prerender(async () => {
	return (await import('$static/changelog.json')).default;
});
