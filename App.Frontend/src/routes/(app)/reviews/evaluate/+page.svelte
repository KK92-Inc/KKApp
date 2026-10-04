<script lang="ts">
	import { page } from "$app/state";
import * as Reviews from "$lib/remotes/review.remote"


const reviews = await Reviews.getPage({
	reviewerId: page.data.session.userId,
	notStatus: "Finished"
});

</script>

{JSON.stringify(reviews.data)}

