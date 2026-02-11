// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// Remark & Rehype plugins
import remarkGfm from "remark-gfm";
// import remarkCollapse from "remark-collapse";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
// Shiki syntax highlighting
import rehypeShikiFromHighlighter from "@shikijs/rehype/core";
import { createHighlighterCoreSync } from "shiki/core";
import { createJavaScriptRegexEngine } from "shiki/engine/javascript";
import c from "shiki/langs/c.mjs";
import cpp from "shiki/langs/cpp.mjs";
import ts from "shiki/langs/typescript.mjs";
import js from "shiki/langs/javascript.mjs";
import php from "shiki/langs/php.mjs";
import css from "shiki/langs/css.mjs";
// import cs from "shiki/langs/csharp.mjs";
import dark from "shiki/themes/github-dark.mjs";
// import light from "shiki/themes/github-light.mjs";
// Markdown
import type { Plugin } from "svelte-exmarkdown";

// ============================================================================
// Plugins
// ============================================================================

const shikiPlugin = {
	rehypePlugin: [
		rehypeShikiFromHighlighter,
		createHighlighterCoreSync({
			themes: [dark],
			langs: [ts, c, cpp, js, php, css],
			engine: createJavaScriptRegexEngine(),
		}),
		{ theme: "github-dark" },
	],
} satisfies Plugin;

export const plugins: Plugin[] = [
	{ remarkPlugin: [remarkGfm] },
	// { remarkPlugin: [remarkCollapse, { test: "summary" }] },
	{ remarkPlugin: [remarkMath], rehypePlugin: [rehypeKatex] },
	shikiPlugin,
];

// ============================================================================
// Context Class
// ============================================================================

export default class {}
