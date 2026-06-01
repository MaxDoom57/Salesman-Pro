#!/bin/bash
set -e

echo "=== Downloading cloudflared ==="
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o ./cloudflared
chmod +x ./cloudflared

echo "=== CF_ACCESS_CLIENT_ID is set: ${CF_ACCESS_CLIENT_ID:0:8}... ==="
echo "=== Starting cloudflared tunnel proxy ==="

./cloudflared access tcp \
  --hostname ssms-hat-02.eposmart.com \
  --url localhost:14335 \
  --service-token-id "$CF_ACCESS_CLIENT_ID" \
  --service-token-secret "$CF_ACCESS_CLIENT_SECRET" &

echo "=== Waiting for tunnel to be ready ==="
sleep 5

echo "=== Starting Bridge App ==="
exec dotnet "Bridge App.dll" --urls "http://0.0.0.0:${PORT:-7060}"
