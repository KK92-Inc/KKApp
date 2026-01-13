FROM oven/bun:1.0

# Install OpenSSH Server and Git
RUN apt-get update && apt-get install -y openssh-server git \
    && mkdir /var/run/sshd \
    && mkdir -p /home/git/repos

# Create the 'git' user (this is the ONLY user people connect as)
RUN useradd -m -d /home/git -s /bin/bash git \
    && passwd -d git

# Setup directories for the "database" of keys (for testing)
# In production, this would be your API or Database connection
COPY gitlab-shell.ts /home/git/gitlab-shell.ts
COPY sshd_config /etc/ssh/sshd_config
# We need a script that acts as the "Database Lookup"
COPY auth-lookup.ts /usr/local/bin/auth-lookup.ts

# Fix permissions
RUN chmod +x /usr/local/bin/auth-lookup.ts \
    && chown -R git:git /home/git

# Generate Host Keys (for the server itself)
RUN ssh-keygen -A

EXPOSE 22

CMD ["/usr/sbin/sshd", "-D", "-e"]
