#!/usr/bin/env bash
# ============================================================
# API Test Script — FembStockTicker
# Usage:
#   ./test-api.sh                         # uses defaults below
#   AUTH0_DOMAIN=xxx AUTH0_CLIENT_ID=yyy AUTH0_CLIENT_SECRET=zzz AUTH0_AUDIENCE=aaa ./test-api.sh
# ============================================================

set -euo pipefail

# ── Configuration ────────────────────────────────────────────
BASE_URL="${BASE_URL:-https://localhost:7236}"
AUTH0_DOMAIN="${AUTH0_DOMAIN:-}"             # e.g. dev-0a51rold.au.auth0.com
AUTH0_CLIENT_ID="${AUTH0_CLIENT_ID:-}"
AUTH0_CLIENT_SECRET="${AUTH0_CLIENT_SECRET:-}"
AUTH0_AUDIENCE="${AUTH0_AUDIENCE:-}"

# ── Helpers ──────────────────────────────────────────────────
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

pass() { echo -e "${GREEN}✓ PASS${NC} — $1"; }
fail() { echo -e "${RED}✗ FAIL${NC} — $1"; FAILURES=$((FAILURES + 1)); }
info() { echo -e "${YELLOW}➜${NC} $1"; }

FAILURES=0

expect_status() {
  local label="$1" expected="$2" actual="$3" body="$4"
  if [[ "$actual" == "$expected" ]]; then
    pass "$label (HTTP $actual)"
  else
    fail "$label — expected HTTP $expected, got HTTP $actual"
    echo "    Response body: $body"
  fi
}

# ── Step 1: Health / reachability ────────────────────────────
echo ""
echo "=================================================="
echo " FembStockTicker API Tests"
echo " Base URL: $BASE_URL"
echo "=================================================="

info "Checking API is reachable..."
reach_status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$BASE_URL/swagger/index.html" || echo "000")
if [[ "$reach_status" == "200" ]]; then
  pass "Swagger UI is reachable (HTTP 200)"
else
  echo -e "${YELLOW}! Swagger UI returned HTTP $reach_status (expected in non-dev environments)${NC}"
fi

# ── Step 2: Unauthenticated request ─────────────────────────
echo ""
info "Test: GET /api/weatherforecast without token → expect 401"
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/weatherforecast")
status=$(echo "$response" | tail -1)
body=$(echo "$response" | head -n -1)
expect_status "Unauthenticated request is rejected" "401" "$status" "$body"

# ── Step 3: Obtain Auth0 token ───────────────────────────────
echo ""
info "Obtaining Auth0 machine-to-machine token..."

if [[ -z "$AUTH0_DOMAIN" || -z "$AUTH0_CLIENT_ID" || -z "$AUTH0_CLIENT_SECRET" || -z "$AUTH0_AUDIENCE" ]]; then
  echo -e "${YELLOW}⚠ Auth0 credentials not set — skipping authenticated tests.${NC}"
  echo "  Set AUTH0_DOMAIN, AUTH0_CLIENT_ID, AUTH0_CLIENT_SECRET, AUTH0_AUDIENCE to run them."
  TOKEN=""
else
  token_response=$(curl -s --request POST \
    --url "https://${AUTH0_DOMAIN}/oauth/token" \
    --header "content-type: application/json" \
    --data "{
      \"client_id\": \"${AUTH0_CLIENT_ID}\",
      \"client_secret\": \"${AUTH0_CLIENT_SECRET}\",
      \"audience\": \"${AUTH0_AUDIENCE}\",
      \"grant_type\": \"client_credentials\"
    }")

  TOKEN=$(echo "$token_response" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

  if [[ -z "$TOKEN" ]]; then
    fail "Failed to obtain Auth0 token"
    echo "    Response: $token_response"
  else
    echo "    Response: $TOKEN"
    pass "Auth0 token obtained"
  fi
fi

# ── Step 4: Authenticated requests ───────────────────────────
if [[ -n "${TOKEN:-}" ]]; then
  echo ""
  info "Test: GET /api/weatherforecast with valid token → expect 200"
  response=$(curl -s -w "\n%{http_code}" \
    -H "Authorization: Bearer $TOKEN" \
    "$BASE_URL/api/weatherforecast")
  status=$(echo "$response" | tail -1)
  body=$(echo "$response" | head -n -1)
  expect_status "Authenticated request succeeds" "200" "$status" "$body"

  # Validate response shape
  if [[ "$status" == "200" ]]; then
    if echo "$body" | grep -q '"date"' && echo "$body" | grep -q '"temperatureC"'; then
      pass "Response contains expected WeatherForecast fields (date, temperatureC)"
    else
      fail "Response shape unexpected — missing expected fields"
      echo "    Body: $body"
    fi

    item_count=$(echo "$body" | grep -o '"date"' | wc -l | tr -d ' ')
    if [[ "$item_count" -eq 5 ]]; then
      pass "Default response contains 5 forecasts"
    else
      fail "Expected 5 forecasts, got $item_count"
    fi
  fi

  echo ""
  info "Test: GET /api/weatherforecast?days=3 → expect 200 with 3 items"
  response=$(curl -s -w "\n%{http_code}" \
    -H "Authorization: Bearer $TOKEN" \
    "$BASE_URL/api/weatherforecast?days=3")
  status=$(echo "$response" | tail -1)
  body=$(echo "$response" | head -n -1)
  expect_status "Query param ?days=3 is accepted" "200" "$status" "$body"

  if [[ "$status" == "200" ]]; then
    item_count=$(echo "$body" | grep -o '"date"' | wc -l | tr -d ' ')
    if [[ "$item_count" -eq 3 ]]; then
      pass "Response contains 3 forecasts when days=3"
    else
      fail "Expected 3 forecasts, got $item_count"
    fi
  fi

  echo ""
  info "Test: GET /api/weatherforecast?days=10 → expect 200 with 10 items"
  response=$(curl -s -w "\n%{http_code}" \
    -H "Authorization: Bearer $TOKEN" \
    "$BASE_URL/api/weatherforecast?days=10")
  status=$(echo "$response" | tail -1)
  body=$(echo "$response" | head -n -1)
  expect_status "Query param ?days=10 is accepted" "200" "$status" "$body"

  if [[ "$status" == "200" ]]; then
    item_count=$(echo "$body" | grep -o '"date"' | wc -l | tr -d ' ')
    if [[ "$item_count" -eq 10 ]]; then
      pass "Response contains 10 forecasts when days=10"
    else
      fail "Expected 10 forecasts, got $item_count"
    fi
  fi

  echo ""
  info "Test: GET /api/weatherforecast with tampered token → expect 401"
  response=$(curl -s -w "\n%{http_code}" \
    -H "Authorization: Bearer ${TOKEN}INVALID" \
    "$BASE_URL/api/weatherforecast")
  status=$(echo "$response" | tail -1)
  body=$(echo "$response" | head -n -1)
  expect_status "Tampered token is rejected" "401" "$status" "$body"
fi

# ── Step 5: Correlation-ID header ────────────────────────────
echo ""
info "Test: Response includes X-Correlation-Id header"
headers=$(curl -s -o /dev/null -D - "$BASE_URL/api/weatherforecast" 2>/dev/null || true)
if echo "$headers" | grep -qi "x-correlation-id"; then
  pass "X-Correlation-Id header is present"
else
  echo -e "${YELLOW}! X-Correlation-Id not found in response headers (may only appear on authenticated requests)${NC}"
fi

# ── Summary ──────────────────────────────────────────────────
echo ""
echo "=================================================="
if [[ "$FAILURES" -eq 0 ]]; then
  echo -e "${GREEN}All tests passed!${NC}"
else
  echo -e "${RED}$FAILURES test(s) failed.${NC}"
fi
echo "=================================================="

exit "$FAILURES"
