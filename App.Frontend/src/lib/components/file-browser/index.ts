import Root from "./file-browser.svelte";

export interface FileNode {
	type: "-" | "d",
	name: string;
	path: string;
}

export {
	Root,
	//
	Root as Browser,
};
