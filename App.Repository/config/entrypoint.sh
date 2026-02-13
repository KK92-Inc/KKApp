#!/bin/sh
# ============================================================================
# Entrypoint for git-ssh container
#
# sshd's AuthorizedKeysCommand runs with a sanitized environment, so
# Aspire-injected env vars (e.g. ConnectionStrings__db) are NOT available
# to the auth binary. We persist them to a file that the auth binary reads.
# ============================================================================
# TODO: Check if we really need *all* of them ?
printenv > /etc/aspire-env
chmod 644 /etc/aspire-env

exec /usr/sbin/sshd -D -e
