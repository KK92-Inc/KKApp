// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { driver } from "driver.js";
import { createContext } from "svelte";
import { SvelteMap } from "svelte/reactivity";
import * as Reviews from "$lib/remotes/review.remote";
import { useDialog } from "$lib/components/dialog";
import { Problem } from "$lib/api";

// ============================================================================

export type Annotation = Extract<components['schemas']['AnnotationData'], { $type: "Comment" }>

export class Context {
	constructor(public readonly reviewId: () => string) { }

	private dialog = useDialog();

	/** Final Conclusion made for the review if it is passable. */
	public pass = $state(false);
	/** Final Conclusion made for the review. */
	public conclusion = $state("");
	/** Key: File Path; Value: Annotations on that file. */
	public annotations = new SvelteMap<string, Annotation[]>([]);

	public addAnnotation(path: string, comment: Annotation) {
		const existing = this.annotations.get(path) ?? [];
		// Optional: filter out any existing comment on the exact same row before adding a new one
		const filtered = existing.filter((a) => a.rowStart !== comment.rowStart);
		this.annotations.set(path, [...filtered, comment]);
	}

	/** Remove an annotation by its starting row */
	public removeAnnotation(path: string, rowStart: number) {
		const existing = this.annotations.get(path);
		if (!existing) return;

		const updated = existing.filter((a) => a.rowStart !== rowStart);
		if (updated.length === 0) {
			this.annotations.delete(path); // Clean up map entry if empty
		} else {
			this.annotations.set(path, updated);
		}
	}

	public async cancel(review: components['schemas']['ReviewDO']) {
		const confirm = this.dialog.confirm(
			"Are you sure you want to cancel this review ?",
			"If this review is a part of round it will be available to someone else. Otherwise you can safely cancel this review."
		)

		if (!await confirm) return;
		await Problem.try(() => Reviews.stop(review.id));
	}

	public finish() {

	}

	public start() {

	}
}

// ============================================================================

export const startTour = () => {
	const driverObj = driver({
		showProgress: true,
		animate: true,
		doneBtnText: 'Got it!',
		nextBtnText: 'Next',
		prevBtnText: 'Back',
		steps: [
			{
				popover: {
					title: 'Welcome to the Review Portal!',
					description: 'Take a quick walkthrough to help you comfortably evaluate this submission.',
					side: 'top',
					align: 'center'
				}
			},
			{
				element: '#file-explorer-sidebar',
				popover: {
					title: '1. Project Files Explorer',
					description: 'Browse the submission repository tree here. Click any file to view its code content.',
					side: 'right',
					align: 'start'
				}
			},
			{
				element: '#view-rubric-btn',
				popover: {
					title: '2. Subject & Grading Rubric',
					description: 'Open this drawer anytime to check assignment instructions and grading criteria side-by-side.',
					side: 'bottom',
					align: 'start'
				}
			},
			{
				element: '#action-finish',
				popover: {
					title: '3. Submit Your Review',
					description: 'When you are satisfied with your feedback, click Finish to submit your review and finalize the evaluation.',
					side: 'bottom',
					align: 'end'
				}
			},
			{
				element: '#action-cancel',
				popover: {
					title: 'Exit Without Saving',
					description: 'In the rare case you need to cancel, you can do so by clicking here. Please use this sparingly and be considered of the other parties time.',
					side: 'bottom',
					align: 'end'
				}
			},
			{
				element: '#help-button',
				popover: {
					title: 'Need a refresher?',
					description: 'You can click this Help button whenever you need to restart this walkthrough guide.',
					side: 'bottom',
					align: 'end'
				}
			}
		]
	});

	driverObj.drive();
};


// ============================================================================

export const [getContext, setContext] = createContext<Context>();
