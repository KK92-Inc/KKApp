<script lang="ts">
  import Markdown from '$lib/components/markdown/markdown.svelte';

  let value = $state('# Hello\n\nThis is **markdown** with `code`.\n\n$$\nE = mc^2\n$$\n\n```typescript\nconst x: number = 42;\n```');
</script>

<Markdown {value} />
