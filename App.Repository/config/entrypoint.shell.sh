#!/bin/sh
# ============================================================================
# Copyright (c) 2026 - W2Inc, All Rights Reserved.
# See README.md in the project root for license information.
# ============================================================================

# Generate SSH host keys if they don't exist, will be bound to a volume!
if [ ! -f /etc/ssh/keys/ssh_host_ed25519_key ]; then
    ssh-keygen -t ed25519 -f /etc/ssh/keys/ssh_host_ed25519_key -N ""
fi

# ============================================================================

printf 'DATABASE_URL=%s\n' "$DB_URI" > /etc/aspire-env
chown -R git:git /home/git/repos
chmod 644 /etc/aspire-env

# ============================================================================

exec /usr/sbin/sshd -D -e