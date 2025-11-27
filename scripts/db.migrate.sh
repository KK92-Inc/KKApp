#!/usr/bin/env bash
set -euo pipefail

# Allow passing the migration name as an argument, otherwise prompt the user
if [ $# -ge 1 ]; then
	MIGRATION_NAME="$*"
else
	read -r -p "Enter migration name: " MIGRATION_NAME
fi

if [ -z "${MIGRATION_NAME// }" ]; then
	echo "Error: migration name cannot be empty." >&2
	exit 1
fi

dotnet ef migrations add "$MIGRATION_NAME" --project "./src/Backend/Backend.API.Infrastructure"
