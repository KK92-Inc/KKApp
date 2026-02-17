import { Dialog as DialogPrimitive } from "bits-ui";

import Title from "./dialog-title.svelte";
import Footer from "./dialog-footer.svelte";
import Header from "./dialog-header.svelte";
import Overlay from "./dialog-overlay.svelte";
import Content from "./dialog-content.svelte";
import Description from "./dialog-description.svelte";
import Trigger from "./dialog-trigger.svelte";
import Close from "./dialog-close.svelte";
import Provider from "./dialog-provider.svelte";
import { get as getDialogContext } from "./actions/context.svelte.js";

const Root = DialogPrimitive.Root;
const Portal = DialogPrimitive.Portal;

/** Retrieve the DialogActionContext from the nearest DialogProvider */
const useDialog = getDialogContext;

export {
	Root,
	Title,
	Portal,
	Footer,
	Header,
	Trigger,
	Overlay,
	Content,
	Description,
	Close,
	Provider,
	useDialog,
	//
	Root as Dialog,
	Title as DialogTitle,
	Portal as DialogPortal,
	Footer as DialogFooter,
	Header as DialogHeader,
	Trigger as DialogTrigger,
	Overlay as DialogOverlay,
	Content as DialogContent,
	Description as DialogDescription,
	Close as DialogClose,
	Provider as DialogProvider,
};
