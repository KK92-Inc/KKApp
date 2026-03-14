import { getLocalTimeZone } from "@internationalized/date";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = async ({ locals }) => {
	return {
		locale: "en-us", // TODO: Configure locale
		tz: getLocalTimeZone(),
		session: locals.session
	}
};
