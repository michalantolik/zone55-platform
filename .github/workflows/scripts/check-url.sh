#!/usr/bin/env bash
set -euo pipefail

url="$1"
name="$2"

attempts="${CHECK_URL_ATTEMPTS:-60}"
delay_seconds="${CHECK_URL_DELAY_SECONDS:-10}"

echo "Checking ${name}: ${url}"
echo "Attempts: ${attempts}"
echo "Delay seconds: ${delay_seconds}"

for i in $(seq 1 "$attempts"); do
  status_code=$(curl -L -s -o /tmp/check-url-response.txt -w "%{http_code}" "$url" || true)

  echo "Attempt $i/${attempts} - HTTP $status_code"

  if [ "$status_code" = "200" ]; then
    echo "${name} is healthy."
    cat /tmp/check-url-response.txt || true
    exit 0
  fi

  echo "${name} is not ready yet. Waiting ${delay_seconds} seconds..."
  sleep "$delay_seconds"
done

echo "${name} did not become healthy in time."
echo "Last response body:"
cat /tmp/check-url-response.txt || true
exit 1
