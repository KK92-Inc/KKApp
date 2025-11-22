// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

type Level = (typeof LEVELS)[number];
const LEVELS = ['debug', 'info', 'warn', 'error'] as const;
const LEVEL = (Bun.env['LOG_TYPE']?.toLowerCase() as Level) ?? LEVELS['0'];
const date = new Intl.DateTimeFormat('en-US', {
	hour: 'numeric',
	minute: '2-digit',
	second: '2-digit',
	hour12: true
});

function log(type: Level, css: Bun.ColorInput, std: Bun.BunFile, ...args: unknown[]) {
	if (LEVELS.indexOf(type) < LEVELS.indexOf(LEVEL)) return;

	const tty = std === Bun.stdout ? process.stdout?.isTTY : process.stderr?.isTTY;
	const start = tty ? (Bun.color(css, 'ansi') ?? '') : '';
	const gray = tty ? (Bun.color('grey', 'ansi') ?? '') : '';

	const safe = (v: unknown) => {
		if (typeof v === 'string') return v;
		if (v instanceof Error) return v.stack ?? v.message;
		try {
			return JSON.stringify(v);
		} catch {
			return String(v);
		}
	};

	const payload = args.map(safe).join(' ');
	std.write(
		`${gray}${date.format(Date.now())} ${start}[${type}]${gray}: ${payload}\n`
	);
}

// ============================================================================

const dbg = (...args: unknown[]) => log('debug', '#9b59b6', Bun.stdout, ...args);
const inf = (...args: unknown[]) => log('info', '#2ecc71', Bun.stdout, ...args);
const wrn = (...args: unknown[]) => log('warn', '#f39c12', Bun.stdout, ...args);
const err = (...args: unknown[]) => log('info', '#e74c3c', Bun.stderr, ...args);

// ============================================================================

export const Log = {
	dbg,
	inf,
	wrn,
	err
};
