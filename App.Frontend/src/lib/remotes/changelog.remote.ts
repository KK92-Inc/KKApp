import { prerender } from "$app/server";

export const getChangelog = prerender(async () => {
	return (await import('$static/changelog.json')).default;
});
