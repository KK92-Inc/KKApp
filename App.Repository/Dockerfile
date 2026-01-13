# ============================================================================
# Stage 1: Build the executables
# ============================================================================

FROM oven/bun:1.0 AS builder

WORKDIR /app

COPY package.json ./
COPY tsconfig.json ./
COPY build.ts ./
COPY src/ ./src/

# RUN bun install --no-cache --frozen-lockfile
RUN bun run build.ts

# ============================================================================
# Stage 2: Runtime container
# ============================================================================

FROM oven/bun:1.0

RUN apt-get update && apt-get install -y openssh-server git
RUN mkdir -p /var/run/sshd
RUN mkdir -p /home/git/repos

# ============================================================================

# Copy compiled executables from builder stage
COPY --from=builder /app/build/ssh-shell /home/git/shell
COPY --from=builder /app/build/ssh-auth /usr/local/bin/auth
COPY ./config/sshd_config /etc/ssh/sshd_config

# ============================================================================

RUN useradd -m -d /home/git -s /bin/bash git && passwd -d git
RUN chown root:root /usr/local/bin/auth && chmod u=rwx,go=rx /usr/local/bin/auth && chown -R git:git /home/git
RUN ssh-keygen -A

# ============================================================================

EXPOSE 22
CMD ["/usr/sbin/sshd", "-D", "-e"]
