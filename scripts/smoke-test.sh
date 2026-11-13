#!/usr/bin/env bash
# API smoke test for RacingApp.
# Usage: ./scripts/smoke-test.sh [baseUrl]   (default http://localhost:5080)
# Start the API first:  dotnet run --project src/RacingApp.Api
set -euo pipefail

BASE="${1:-http://localhost:5080}"
PASS=0
FAIL=0

check() {
  local name="$1" expected="$2" actual="$3"
  if [ "$actual" = "$expected" ]; then
    PASS=$((PASS + 1)); echo "PASS  $name ($actual)"
  else
    FAIL=$((FAIL + 1)); echo "FAIL  $name (expected $expected, got $actual)"
  fi
}

login_token() {
  curl -s -X POST "$BASE/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$1\",\"password\":\"Passw0rd!\"}" \
    | python3 -c "import sys,json;print(json.load(sys.stdin).get('token',''))"
}

echo "== 1. API reachable =="
code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/swagger/index.html")
check "GET /swagger" 200 "$code"

echo "== 2. Login all seeded roles =="
ADMIN=$(login_token admin@racing.app)
OWNER=$(login_token owner@racing.app)
TRAINER=$(login_token trainer@racing.app)
VET=$(login_token vet@racing.app)
FAN=$(login_token fan@racing.app)
for role in ADMIN OWNER TRAINER VET FAN; do
  [ -n "${!role}" ] && check "login $role" ok ok || check "login $role" ok missing
done

echo "== 3. Public/list endpoints =="
code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/racetracks")
check "GET /api/racetracks (anon)" 200 "$code"
code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/meetings")
check "GET /api/meetings (anon)" 200 "$code"
code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/racers?search=B")
check "GET /api/racers?search (anon)" 200 "$code"

echo "== 4. Role-restricted endpoints =="
code=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $FAN" "$BASE/api/injuries")
check "GET /api/injuries as Racegoer (expect 403)" 403 "$code"
code=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $VET" "$BASE/api/injuries")
check "GET /api/injuries as Vet" 200 "$code"
code=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $OWNER" "$BASE/api/racers/mine")
check "GET /api/racers/mine as Owner" 200 "$code"
code=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $TRAINER" "$BASE/api/racers/mine")
check "GET /api/racers/mine as Trainer" 200 "$code"

echo "== 5. TrackAdmin creates meeting + race + entry + result =="
MEETING=$(curl -s -X POST "$BASE/api/meetings" -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"racetrackId":1,"date":"2030-01-01","gatesOpen":"11:30:00"}')
MEETING_ID=$(echo "$MEETING" | python3 -c "import sys,json;print(json.load(sys.stdin).get('id',''))")
[ -n "$MEETING_ID" ] && check "POST /api/meetings" ok ok || check "POST /api/meetings" ok "failed: $MEETING"

RACE=$(curl -s -X POST "$BASE/api/races" -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d "{\"raceMeetingId\":$MEETING_ID,\"raceNumber\":1,\"distanceMeters\":500,\"grade\":\"A5\",\"prizeMoney\":1000,\"scheduledTime\":\"12:00:00\"}")
RACE_ID=$(echo "$RACE" | python3 -c "import sys,json;print(json.load(sys.stdin).get('id',''))")
[ -n "$RACE_ID" ] && check "POST /api/races" ok ok || check "POST /api/races" ok "failed: $RACE"

ENTRY=$(curl -s -X POST "$BASE/api/entries" -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d "{\"raceId\":$RACE_ID,\"racerId\":1,\"trapNumber\":1,\"weightKg\":29.5}")
ENTRY_ID=$(echo "$ENTRY" | python3 -c "import sys,json;print(json.load(sys.stdin).get('id',''))")
[ -n "$ENTRY_ID" ] && check "POST /api/entries" ok ok || check "POST /api/entries" ok "failed: $ENTRY"

code=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE/api/entries/$ENTRY_ID/result" \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"finishPosition":1,"raceTimeSeconds":29.10}')
check "PUT /api/entries/$ENTRY_ID/result" 204 "$code"

echo "== 6. Vet records injury + treatment =="
INJURY=$(curl -s -X POST "$BASE/api/injuries" -H "Authorization: Bearer $VET" -H "Content-Type: application/json" \
  -d '{"racerId":1,"description":"Smoke-test strain","bodyPart":"Left hind","severity":"Minor"}')
INJURY_ID=$(echo "$INJURY" | python3 -c "import sys,json;print(json.load(sys.stdin).get('id',''))")
[ -n "$INJURY_ID" ] && check "POST /api/injuries" ok ok || check "POST /api/injuries" ok "failed: $INJURY"

code=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/treatments" -H "Authorization: Bearer $VET" -H "Content-Type: application/json" \
  -d "{\"injuryId\":$INJURY_ID,\"dateAdministered\":\"2030-01-01T00:00:00Z\",\"description\":\"Smoke-test treatment\",\"medication\":\"Test-med\",\"dosage\":\"5ml\",\"withdrawalPeriodDays\":7}")
check "POST /api/treatments" 200 "$code"

echo
echo "RESULT: $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ]
