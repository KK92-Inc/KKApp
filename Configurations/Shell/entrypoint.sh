#!/bin/sh
# ============================================================================
# Copyright (c) 2026 - W2Inc, All Rights Reserved.
# See README.md in the project root for license information.
# ============================================================================
set -eu

# Generate SSH host keys if they don't exist, will be bound to a volume!
if [ ! -f /etc/ssh/keys/ssh_host_ed25519_key ]; then
    ssh-keygen -t ed25519 -f /etc/ssh/keys/ssh_host_ed25519_key -N ""
fi

# ============================================================================
# Persist connection info for anything sshd execs (AuthorizedKeysCommand,
# ForceCommand shell) since OpenSSH strips the environment on exec.
# ============================================================================

{
    printf 'DATABASE_URL=%s\n' "$DB_URI"
    printf 'VALKEY_URL=%s\n' "$VALKEY_URI"
    printf 'KC_ORIGIN=%s\n' "$KC_ORIGIN"
    printf 'KC_SECRET=%s\n' "$KC_SECRET"
    printf 'REPOSITORY_DIRECTORY=%s\n' "$REPOSITORY_DIRECTORY"
} > /etc/sshenv

# Make it readable to auth (runs as nobody) and shell (runs as git)
chown root:sshenv /etc/sshenv
chmod 640 /etc/sshenv

# Fixing ownership on the actual git repository storage. As volumes are
# created by root.
chown -R git:git /home/git/repos

# ============================================================================

exec /usr/sbin/sshd -D -e