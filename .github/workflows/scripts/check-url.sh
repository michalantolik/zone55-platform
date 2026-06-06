#!/usr/bin/env bash
set -euo pipefail

url="$1"
name="$2"

echo "Checking ${name}: ${url}"

for i in {1..30}; do
  status_code=$(curl -L -s -o /tmp/check-url-response.txt -w "%{http_code}" "$url" || true)

  echo "Attempt $i/30 - HTTP $status_code"

  if [ "$status_code" = "200" ]; then
    echo "${name} is healthy."
    cat /tmp/check-url-response.txt || true
    exit 0
  fi

  echo "${name} is not ready yet. Waiting 10 seconds..."
  sleep 10
done

echo "${name} did not become healthy in time."
cat /tmp/check-url-response.txt || true
exit 1
