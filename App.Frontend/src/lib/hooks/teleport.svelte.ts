// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { Attachment } from "svelte/attachments";

// ============================================================================

/**
 * Teleport/portal an element from it's position to the element with the given
 * id
 * @param id The targeted element to portal to.
 * @returns An attachement you can use via {@attach teleport('foo')}
 */
export default function teleport(id: string): Attachment {
	return (element: Element) => {
		$effect(() => {
			const target = document.getElementById(id);
			if (!target || !target.parentNode) {
				return;
			}

			const parent = target.parentNode;
			const nextSibling = target.nextSibling;
			parent.replaceChild(element, target);

			return () => {
				if (element.parentNode) {
					element.parentNode.removeChild(element);

					if (nextSibling) {
						parent.insertBefore(target, nextSibling);
					} else {
						parent.appendChild(target);
					}
				}
			};
		})
	};
}
