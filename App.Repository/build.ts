import { Glob } from "bun";

// Find all *.exe.ts files
const glob = new Glob("**/*.exe.ts");
const exeFiles = Array.from(glob.scanSync({ cwd: "./src", absolute: false }));

// Build each as a native Alpine executable
for (const file of exeFiles) {
  const name = file.replace(/\.exe\.ts$/, "").replace(/\//g, "-");
  console.log(`Building: src/${file} -> build/${name}`);

  // Compile to native executable for Alpine (--compile bundles + creates executable)
  const proc = Bun.spawn(
    [
      "bun",
			"--bun",
      "build",
      "--compile",
      "--minify",
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

  console.log(`✓ Built: build/${name}`);
}
