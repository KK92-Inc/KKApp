// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { driver } from "driver.js";
import { createContext } from "svelte";
import { SvelteMap } from "svelte/reactivity";
import { untrack } from "svelte";
import { goto, refreshAll } from "$app/navigation";
import { Problem } from "$lib/api";
import * as Review from "$lib/review";
import * as Reviews from "$lib/remotes/review.remote";
import { useDialog } from "$lib/components/dialog";

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
		// NOTE(W2): Technically it is ... fine ?
		// Filter out any existing comment on the exact same row before adding a new one
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

	public reset() {
		this.pass = false;
		this.conclusion = "";
		this.annotations.clear();
	}

	public get comments(): Annotation[] {
		return [...this.annotations.values()].flat();
	}

	public async cancel(review: components['schemas']['ReviewDO']) {
		const { title, message } = Review.CONFIRM.cancel();
		if (!await this.dialog.confirm(title, message)) return;
		let cancelled = false;
		await Problem.try(async () => {
			await Reviews.stop(review.id);
			cancelled = true;
		});
		await refreshAll({ includeLoadFunctions: false });
		if (cancelled)
			await goto(review.roundId ? `/reviews/round/${review.roundId}` : '/reviews');
	}

	/**
	 * Submit the verdict, the comments made on the files and the conclusion.
	 * @returns true if the review was finished.
	 */
	public async finish(review: components['schemas']['ReviewDO']): Promise<boolean> {
		const result = await Problem.try(() =>
			Reviews.finish({
				id: review.id,
				passed: this.pass,
				annotations: Review.toAnnotations(this.comments, this.conclusion)
			})
		);

		await refreshAll({ includeLoadFunctions: false });

		if (!result) return false;
		this.reset();
		await goto(review.roundId ? `/reviews/round/${review.roundId}` : '/reviews/recent');
		return true;
	}

	public async start(review: components['schemas']['ReviewDO']) {
		const { title, message } = Review.CONFIRM.start(review.userProject.name);
		if (!await this.dialog.confirm(title, message)) return;
		await Problem.try(() => Reviews.start(review.id));
		await refreshAll({ includeLoadFunctions: false });
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
