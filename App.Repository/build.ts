import { Glob, $ } from "bun";
import { parseArgs } from "util";

// ============================================================================
// Parse CLI arguments
// ============================================================================

const { values } = parseArgs({
	args: Bun.argv.slice(2),
	options: {
		docker: { type: "boolean", default: false },
		tag: { type: "string", default: "latest" },
	},
});

// ============================================================================
// Build executables
// ============================================================================

// Find all *.exe.ts files
const glob = new Glob("**/*.exe.ts");
const exeFiles = Array.from(glob.scanSync({ cwd: "./src", absolute: false }));

// Build each as a native executable
for (const file of exeFiles) {
	const name = file.replace(/\.exe\.ts$/, "").replace(/\//g, "-");
	console.log(`Building: src/${file} -> output/${name}`);

	const proc = Bun.spawn(
		[
			"bun",
			"--bun",
			"build",
			"--compile",
			"--minify",
			"--target=bun-linux-x64",
			`./src/${file}`,
			"--outfile",
			`./output/${name}`,
		],
		{ stdout: "inherit", stderr: "inherit" }
	);

	await proc.exited;

	if (proc.exitCode !== 0) {
		console.error(`Failed to compile ${file}`);
		process.exit(1);
	}

	console.log(`✓ Built: output/${name}`);
}

// ============================================================================
// Build Docker images (optional)
// ============================================================================

if (values.docker) {
	const tag = values.tag;
	console.log(`\nBuilding Docker images with tag: ${tag}`);

	// Build SSH server image
	console.log("\n📦 Building SSH server image...");
	const sshBuild = await $`docker build -f Dockerfile.ssh -t git-ssh:${tag} .`.nothrow();
	if (sshBuild.exitCode !== 0) {
		console.error("Failed to build SSH Docker image");
		process.exit(1);
	}
	console.log(`✓ Built: git-ssh:${tag}`);

	// Build API server image
	console.log("\n📦 Building API server image...");
	const apiBuild = await $`docker build -f Dockerfile.api -t git-api:${tag} .`.nothrow();
	if (apiBuild.exitCode !== 0) {
		console.error("Failed to build API Docker image");
		process.exit(1);
	}
	console.log(`✓ Built: git-api:${tag}`);

	console.log("\n✅ All Docker images built successfully!");
}
