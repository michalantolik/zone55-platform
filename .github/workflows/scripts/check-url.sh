#!/usr/bin/env bash
set -euo pipefail

url="$1"
name="$2"
expected_status="${3:-200}"

attempts="${CHECK_URL_ATTEMPTS:-60}"
delay_seconds="${CHECK_URL_DELAY_SECONDS:-10}"
connect_timeout_seconds="${CHECK_URL_CONNECT_TIMEOUT_SECONDS:-10}"
max_time_seconds="${CHECK_URL_MAX_TIME_SECONDS:-30}"
response_file="$(mktemp)"
trap 'rm -f "$response_file"' EXIT

echo "Checking ${name}: ${url}"
echo "Expected HTTP status: ${expected_status}"
echo "Attempts: ${attempts}"
echo "Delay seconds: ${delay_seconds}"
echo "Connect timeout seconds: ${connect_timeout_seconds}"
echo "Maximum request time seconds: ${max_time_seconds}"

for i in $(seq 1 "$attempts"); do
  : > "$response_file"

  status_code=$(curl \
    --location \
    --silent \
    --show-error \
    --connect-timeout "$connect_timeout_seconds" \
    --max-time "$max_time_seconds" \
    --output "$response_file" \
    --write-out "%{http_code}" \
    "$url" || true)

  status_code="${status_code:-000}"
  echo "Attempt $i/${attempts} - HTTP $status_code"

  if [ "$status_code" = "$expected_status" ]; then
    echo "${name} is healthy."
    exit 0
  fi

  if [ -s "$response_file" ]; then
    echo "Response body:"
    head -c 4000 "$response_file" || true
    echo ""
  fi

  if [ "$i" -lt "$attempts" ]; then
    echo "${name} is not ready yet. Waiting ${delay_seconds} seconds..."
    sleep "$delay_seconds"
  fi
done

echo "${name} did not become healthy in time."
echo "Last response body:"
cat "$response_file" || true
exit 1
