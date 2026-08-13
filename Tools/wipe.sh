podman ps -a --format "{{.ID}} {{.Names}}" | grep -v "dagster" | awk '{print $1}' | xargs -r podman rm -f;
podman volume ls --format "{{.Name}}" | grep -v "dagster" | xargs -r podman volume rm;
podman system prune -f