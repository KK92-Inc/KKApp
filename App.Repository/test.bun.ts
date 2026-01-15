import { SQL } from "bun";

try {
	const sql = new SQL({
		url: "postgresql://postgres:postgres@localhost:52843/peeru-db",
	});
	await sql.connect();

	const result = await sql<{ login: string }[]>`
			SELECT u.login
			FROM tbl_ssh_key k
			JOIN tbl_user u ON k.user_id = u.id
			WHERE k.fingerprint = 'SHA256:WVzjBLz4E64ogZikYIXKVNj9XtsXhvXZ1BZg1zjOUm0'
		`;

	console.log(result.at(0));
	process.exit(1)
} catch (error) {
	console.error(error)
}
