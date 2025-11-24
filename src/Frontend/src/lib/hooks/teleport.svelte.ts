import type { Attachment } from "svelte/attachments";

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
