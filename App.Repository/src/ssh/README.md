# Shell access (Git Push, Pull, Fetch, ...)

The shell.exe gets pumped with these variables as for example:
```ts
{
  NODE_ENV: "development",
  USER: "user_123",
  SHELL: "/bin/bash",
  PWD: "/home/git",
  LOGNAME: "git",
  MOTD_SHOWN: "pam",
  HOME: "/home/git",
  SSH_ORIGINAL_COMMAND: "git-upload-pack '/my-new-repo.git'",
  SSH_CONNECTION: "192.168.65.1 20739 172.17.0.2 22",
  SHLVL: "0",
  SSH_CLIENT: "192.168.65.1 20739 22",
  PATH: "/usr/local/bin:/usr/bin:/bin:/usr/games",
  _: "/usr/local/bin/bun",
  TZ: undefined,
}
```
