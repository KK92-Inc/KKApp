import { getLocalTimeZone } from "@internationalized/date";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = async ({ locals }) => {
	return {
		tz: getLocalTimeZone(),
		session: locals.session
	}
};
