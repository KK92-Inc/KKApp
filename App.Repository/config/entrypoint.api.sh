#!/bin/sh
# ============================================================================
# Copyright (c) 2026 - W2Inc, All Rights Reserved.
# See README.md in the project root for license information.
# ============================================================================

printf 'DATABASE_URL=%s\n' "$DB_URI" > /etc/aspire-env
chmod 644 /etc/aspire-env
chown -R git:git /home/git/repos

exec /usr/local/bin/api