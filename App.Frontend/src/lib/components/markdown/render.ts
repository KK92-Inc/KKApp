import { unified } from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import remarkRehype from 'remark-rehype';
import rehypeKatex from 'rehype-katex';
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize';
import rehypeStringify from 'rehype-stringify';
import rehypeShikiFromHighlighter from '@shikijs/rehype/core';
import { createHighlighterCoreSync, type ShikiTransformer } from 'shiki/core';
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

const transformerLineNumbers: ShikiTransformer = {
	name: 'line-numbers',
	line(node, line) {
		node.properties['data-line'] = String(line);
		node.children.unshift({
			type: 'element',
			tagName: 'span',
			properties: {
				className: ['line-number'],
				'data-line': String(line)
			},
			children: [{ type: 'text', value: String(line) }]
		});
	}
};

const sanitizeSchema: Schema = {
	...defaultSchema,
	attributes: {
		...defaultSchema.attributes,
		'*': [...(defaultSchema.attributes?.['*'] ?? []), 'className', 'class', 'ariaHidden', 'style'],
		code: [...(defaultSchema.attributes?.['code'] ?? []), 'style', 'data*'],
		pre: [...(defaultSchema.attributes?.['pre'] ?? []), 'style', 'data*', 'tabindex'],
		span: [...(defaultSchema.attributes?.['span'] ?? []), 'style', 'data*']
	}
};

const highlighter = createHighlighterCoreSync({
	themes: [dark],
	langs: [c, cpp, ts, js, php, css, json, bash, python, markdown, sql, csharp],
	engine: createJavaScriptRegexEngine()
});

// render.ts - update inside Markdown.highlightLines
export const Markdown = {
	highlightLines: (source: string, lang: string) => {
		const normalized = lang.trim().toLowerCase();
		const langs = highlighter.getLoadedLanguages();
		const targetLang = langs.includes(normalized) ? normalized : 'text';

		const html = highlighter.codeToHtml(source, {
			lang: targetLang,
			theme: 'github-dark',
			transformers: [transformerLineNumbers]
		});

		const preMatch = html.match(/^<pre([^>]*)><code>([\s\S]*)<\/code><\/pre>$/);
		if (!preMatch) {
			return { preClass: '', preStyle: '', lines: [html] };
		}

		const [, preAttrs, innerCode] = preMatch;
		const classMatch = preAttrs.match(/class="([^"]*)"/);
		const styleMatch = preAttrs.match(/style="([^"]*)"/);

		const lines = innerCode.split('\n');
		// Remove empty trailing line from splitting ends with \n
		if (lines.length > 0 && lines[lines.length - 1] === '') {
			lines.pop();
		}

		return {
			preClass: classMatch ? classMatch[1] : '',
			preStyle: styleMatch ? styleMatch[1] : '',
			lines
		};
	}
};
