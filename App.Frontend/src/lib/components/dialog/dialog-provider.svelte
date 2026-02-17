<!-- @component Dialog Provider — renders programmatic action dialogs. Place at app root. -->
<script lang="ts">
	import type { Snippet } from "svelte";
	import { DialogActionContext, init } from "./actions/context.svelte.js";
	import DialogConfirm from "./actions/dialog-confirm.svelte";
	import DialogAlert from "./actions/dialog-alert.svelte";

	interface Props {
		children?: Snippet;
	}

	const { children }: Props = $props();
	const ctx = init(new DialogActionContext());
</script>

{@render children?.()}

<DialogConfirm {ctx} />
<DialogAlert {ctx} />
