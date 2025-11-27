import { getUsers } from "$lib/remotes/user.remote";
import type { PageServerLoad } from "./$types";

export const load: PageServerLoad = async ({ }) => {
	return { users: getUsers({ }) };
};
