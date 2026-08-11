import { redirect } from "@sveltejs/kit";
import * as Workspace from "$lib/remotes/workspace.remote";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async () => {
	const space = await Workspace.current();
	redirect(303, `/workspace/${space.id}`);
};
