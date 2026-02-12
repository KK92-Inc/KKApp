import { getRequestEvent, query } from "$app/server";
import { error } from "@sveltejs/kit";

export const getWorkspace = query(async () => {
	const { locals } = getRequestEvent();
	const { data, error: err } = await locals.api.GET("/workspace/current");
	if (!data || err) error(500);
	return data;
});
