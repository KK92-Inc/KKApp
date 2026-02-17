<script>
	import { theme, toggleMode } from "mode-watcher";
	import { Button } from "../button";
	import { Moon, Sun } from "@lucide/svelte";
</script>

<Button onclick={toggleMode} variant="outline" size="icon" aria-label="Notifications" class="max-md:hidden">
	{#if theme.current === 'dark'}
		<Moon />
	{:else}
		<Sun />
	{/if}
</Button>
