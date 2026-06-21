╔══════════════════════════════════════════════════════════════╗
║  Tasia Sync Server — WebSocket Relay                        ║
║  Version 1.0.0 — Protocol v1                                ║
╚══════════════════════════════════════════════════════════════╝

PURPOSE:
  External WebSocket relay for TasiaBotFriends multiplayer sync.
  Host publishes Tasia state → server → friends receive and render.
  No AI, no game logic, no secrets — just relay.

═══════════════════════════════════════════════════════════════
QUICK START
═══════════════════════════════════════════════════════════════

With Docker:
  docker compose up -d

With Python directly:
  pip install websockets
  python server.py

Server runs on ws://0.0.0.0:24222/

═══════════════════════════════════════════════════════════════
HEALTH CHECK
═══════════════════════════════════════════════════════════════

  curl http://localhost:24222/health

Returns:
  {"status":"ok","uptime":123.4,"rooms":0,"clients":0,"protocolVersion":1}

═══════════════════════════════════════════════════════════════
CONNECTION
═══════════════════════════════════════════════════════════════

Host:
  ws://server:24222/ws?room=marty-test-001&role=host&token=optional

Client:
  ws://server:24222/ws?room=marty-test-001&role=client&token=optional

═══════════════════════════════════════════════════════════════
PROTOCOL
═══════════════════════════════════════════════════════════════

All messages are JSON.

Host → Server:
  {"type":"STATE_UPDATE","protocolVersion":1,"modVersion":"1.0.0",
   "roomId":"...","timestamp":123,"tasia":{...}}

  {"type":"VOICE_LINE","protocolVersion":1,"roomId":"...",
   "timestamp":123,"lineId":"...","speaker":"Tasia","text":"..."}

Client → Server:
  {"type":"PING"}

Server → Client:
  {"type":"STATE_UPDATE", ...}  (relayed from host)
  {"type":"VOICE_LINE", ...}    (relayed from host)
  {"type":"HOST_LEFT", ...}
  {"type":"PONG", ...}
  {"type":"CLIENT_HELLO", ...}
  {"type":"ERROR", ...}

═══════════════════════════════════════════════════════════════
TASIA STATE FORMAT
═══════════════════════════════════════════════════════════════

{
  "type": "STATE_UPDATE",
  "protocolVersion": 1,
  "modVersion": "1.0.0",
  "roomId": "marty-test-001",
  "timestamp": 123456789.0,
  "tasia": {
    "active": true,
    "position": {"x": 1.2, "y": 0.0, "z": 5.4},
    "rotationY": 180.0,
    "mode": "COLLECT",
    "intent": "CARRY_TO_EXTRACTION",
    "carrying": true,
    "danger": "safe",
    "name": "Tasia",
    "stale": false
  }
}

═══════════════════════════════════════════════════════════════
CONFIG
═══════════════════════════════════════════════════════════════

Environment variables:
  PORT=24222
  MAX_CLIENTS_PER_ROOM=8
  ROOM_TOKEN_REQUIRED=false
  LOG_LEVEL=info
