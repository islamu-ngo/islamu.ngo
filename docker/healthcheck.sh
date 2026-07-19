# ABOUTME: Provides the shared ISLAMU container liveness probe without curl or wget.
# ABOUTME: Uses Bash /dev/tcp and accepts only an HTTP 200 status from the first response line.
set -euo pipefail

host="${ISLAMU_HEALTHCHECK_HOST:-127.0.0.1}"
port="${ISLAMU_HEALTHCHECK_PORT:-8080}"
path="${ISLAMU_HEALTHCHECK_PATH:-/alive}"

if [[ "$path" != /* ]]; then
  echo "healthcheck error: invalid path"
  exit 1
fi

if ! exec 3<>"/dev/tcp/$host/$port"; then
  echo "healthcheck error: connection failed"
  exit 1
fi

printf 'GET %s HTTP/1.1\r\nHost: %s\r\nConnection: close\r\n\r\n' "$path" "$host" >&3

if ! IFS= read -r -t "${ISLAMU_HEALTHCHECK_TIMEOUT_SECONDS:-5}" status_line <&3; then
  echo "healthcheck error: no response"
  exit 1
fi

status_line="${status_line%$'\r'}"
if [[ "$status_line" != HTTP/*' 200 '* ]]; then
  echo "healthcheck error: unexpected status"
  exit 1
fi

echo "healthcheck ok"
