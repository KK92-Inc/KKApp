// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { unified } from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import remarkRehype from 'remark-rehype';
import rehypeKatex from 'rehype-katex';
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize';
import rehypeStringify from 'rehype-stringify';
import rehypeShikiFromHighlighter from '@shikijs/rehype/core';
import { createHighlighterCoreSync } from 'shiki/core';
import { createJavaScriptRegexEngine } from 'shiki/engine/javascript';
import type { Schema } from 'hast-util-sanitize';

import c from 'shiki/langs/c.mjs';
import cpp from 'shiki/langs/cpp.mjs';
import ts from 'shiki/langs/typescript.mjs';
import js from 'shiki/langs/javascript.mjs';
import php from 'shiki/langs/php.mjs';
import css from 'shiki/langs/css.mjs';
import json from 'shiki/langs/json.mjs';
import bash from 'shiki/langs/bash.mjs';
import python from 'shiki/langs/python.mjs';
import markdown from 'shiki/langs/markdown.mjs';
import sql from 'shiki/langs/sql.mjs';
import csharp from 'shiki/langs/csharp.mjs';
import dark from 'shiki/themes/github-dark.mjs';

// ============================================================================
// Sanitization schema
// Extends the default safe schema with elements/attributes needed by
// KaTeX (MathML) and Shiki (inline styles on <pre>/<code>/<span>).
// ============================================================================

const KATEX_MATHML_TAGS = [
	'math',
	'semantics',
	'mrow',
	'mi',
	'mo',
	'mn',
	'ms',
	'mtext',
	'mfrac',
	'msqrt',
	'mroot',
	'msup',
	'msub',
	'msubsup',
	'munder',
	'mover',
	'munderover',
	'mtable',
	'mtr',
	'mtd',
	'mspace',
	'annotation',
	'menclose',
	'mglyph',
	'mpadded',
	'mphantom',
	'mstyle',
	'merror',
	'mlabeledtr',
	'mmultiscripts',
	'mprescripts'
];

const KATEX_MATHML_ATTRS = [
	'mathvariant',
	'encoding',
	'xmlns',
	'display',
	'fence',
	'stretchy',
	'symmetric',
	'lspace',
	'rspace',
	'movablelimits',
	'accent',
	'accentunder',
	'columnalign',
	'columnspacing',
	'rowspacing',
	'columnlines',
	'rowlines',
	'frame',
	'framespacing',
	'rowalign',
	'width',
	'height',
	'depth',
	'voffset',
	'linethickness',
	'scriptlevel',
	'minsize',
	'maxsize',
	'separator'
];

const sanitizeSchema: Schema = {
	...defaultSchema,
	tagNames: [...(defaultSchema.tagNames ?? []), ...KATEX_MATHML_TAGS],
	attributes: {
		...defaultSchema.attributes,
		// Allow class everywhere (KaTeX .katex-*, Shiki .shiki etc.)
		'*': [...(defaultSchema.attributes?.['*'] ?? []), 'className', 'class', 'ariaHidden', 'style'],
		// KaTeX MathML attributes on all MathML elements
		...Object.fromEntries(
			KATEX_MATHML_TAGS.map((tag) => [
				tag,
				[...(defaultSchema.attributes?.[tag] ?? []), ...KATEX_MATHML_ATTRS]
			])
		),
		// Shiki uses style & data-* on code/pre/span
		code: [...(defaultSchema.attributes?.['code'] ?? []), 'style', 'data*'],
		pre: [...(defaultSchema.attributes?.['pre'] ?? []), 'style', 'data*', 'tabindex'],
		span: [...(defaultSchema.attributes?.['span'] ?? []), 'style', 'data*']
	}
};

// ============================================================================

const highlighter = createHighlighterCoreSync({
	themes: [dark],
	langs: [c, cpp, ts, js, php, css, json, bash, python, markdown, sql, csharp],
	engine: createJavaScriptRegexEngine()
});

const processor = unified()
	.use(remarkParse)
	.use(remarkGfm)
	.use(remarkMath)
	.use(remarkRehype, { allowDangerousHtml: false })
	.use(rehypeKatex)
	//@ts-expect-error TODO: Check why this complains
	.use(rehypeShikiFromHighlighter, highlighter, {
		theme: 'github-dark',
		fallbackLanguage: 'text'
	})
	.use(rehypeSanitize, sanitizeSchema)
	.use(rehypeStringify);

export const Markdown = {
	/**
	 * Render a markdown string to sanitized HTML. Fully SSR-safe.
	 *
	 * Supports GFM, LaTeX/KaTeX math, and syntax highlighted code blocks (shiki).
	 * Output is sanitized via rehype-sanitize to prevent XSS.
	 */
	render: async (source: string) => String(await processor.process(source))
};
