// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { query } from "$app/server";
import { Filters } from "$lib/api";
import * as Reviews from "$lib/remotes/review.remote";

// ============================================================================

export const getData = query(Filters.id, async (reviewId) => {
	const review = await Reviews.get(reviewId);
	// A review *might* be attached to a round, otherwise it its advisory.
	return [review, review.roundId ? await Reviews.getRound(review.roundId) : null];
});
