#!/usr/bin/env python3
"""
Tasia Sync Server — WebSocket relay for TasiaBotFriends multiplayer state.
Host publishes Tasia state, friends subscribe and render.
No AI, no game logic, no secrets — just relay.
"""

import asyncio
import json
import logging
import time
import uuid
from dataclasses import dataclass, field
from typing import Optional

import websockets
from websockets.server import WebSocketServerProtocol

# ─── Config ───────────────────────────────────────────────────────────────
HOST = "0.0.0.0"
PORT = 24222
MAX_CLIENTS_PER_ROOM = 8
ROOM_TOKEN_REQUIRED = False  # Set True for private sessions
MAX_MESSAGE_BYTES = 16384
STATE_RATE_LIMIT_HZ = 15
PROTOCOL_VERSION = 1
MOD_VERSION = "1.0.0"
LOG_LEVEL = "INFO"

logging.basicConfig(
    level=getattr(logging, LOG_LEVEL.upper()),
    format="%(asctime)s [%(levelname)s] %(message)s",
    datefmt="%H:%M:%S",
)
log = logging.getLogger("tasia-sync")

# ─── Room model ──────────────────────────────────────────────────────────
@dataclass
class Room:
    room_id: str
    token: str = ""
    host: Optional[WebSocketServerProtocol] = None
    clients: set = field(default_factory=set)
    host_addr: str = ""
    created_at: float = field(default_factory=time.time)
    last_state: dict = field(default_factory=dict)

    @property
    def client_count(self):
        return len(self.clients)

    @property
    def is_active(self):
        return self.host is not None and self.host.open


rooms: dict[str, Room] = {}
server_start = time.time()


# ─── Helpers ──────────────────────────────────────────────────────────────
def make_msg(msg_type: str, room_id: str, **kwargs) -> str:
    payload = {
        "type": msg_type,
        "protocolVersion": PROTOCOL_VERSION,
        "modVersion": MOD_VERSION,
        "roomId": room_id,
        "timestamp": time.time(),
        **kwargs,
    }
    return json.dumps(payload, default=str)


def get_query_param(path: str, key: str) -> str:
    """Extract query parameter from WebSocket path."""
    if "?" not in path:
        return ""
    query = path.split("?", 1)[1]
    for part in query.split("&"):
        if "=" in part:
            k, v = part.split("=", 1)
            if k == key:
                return v
    return ""


def get_remote_addr(ws: WebSocketServerProtocol) -> str:
    try:
        host, port = ws.remote_address
        return f"{host}:{port}"
    except:
        return "unknown"


async def send_json(ws: WebSocketServerProtocol, data: str):
    try:
        await ws.send(data)
    except websockets.exceptions.ConnectionClosed:
        pass


# ─── Health endpoint (plain HTTP via websockets) ──────────────────────────
async def handle_health(path, request_headers):
    if path == "/health":
        body = json.dumps({
            "status": "ok",
            "uptime": round(time.time() - server_start, 1),
            "rooms": len(rooms),
            "clients": sum(r.client_count for r in rooms.values()),
            "protocolVersion": PROTOCOL_VERSION,
        })
        return websockets.http.Headers({"Content-Type": "application/json"}), 200, body
    return None


# ─── WebSocket handler ────────────────────────────────────────────────────
async def handler(ws: WebSocketServerProtocol, path: str):
    # Parse query params
    room_id = get_query_param(path, "room") or "default"
    role = get_query_param(path, "role") or "client"
    token = get_query_param(path, "token") or ""
    addr = get_remote_addr(ws)

    log.info(f"Connect: role={role} room={room_id} addr={addr}")

    # Token check
    if ROOM_TOKEN_REQUIRED and not token:
        await send_json(ws, make_msg("ERROR", room_id, error="token_required", message="Room token is required"))
        await ws.close(4001, "token_required")
        return

    # Get or create room
    room = rooms.get(room_id)
    if room is None:
        room = Room(room_id=room_id, token=token)
        rooms[room_id] = room
        log.info(f"Room created: {room_id}")

    # Check room token
    if ROOM_TOKEN_REQUIRED and room.token and token != room.token:
        await send_json(ws, make_msg("ERROR", room_id, error="invalid_token", message="Invalid room token"))
        await ws.close(4001, "invalid_token")
        return

    # Role handling
    if role == "host":
        if room.host is not None and room.host.open:
            await send_json(ws, make_msg("ERROR", room_id, error="host_exists", message="A host is already connected to this room"))
            await ws.close(4002, "host_exists")
            return
        room.host = ws
        room.host_addr = addr
        await send_json(ws, make_msg("HOST_HELLO", room_id, role="host", clients=0))
        log.info(f"Host registered: room={room_id}")

    elif role == "client":
        if room.client_count >= MAX_CLIENTS_PER_ROOM:
            await send_json(ws, make_msg("ERROR", room_id, error="room_full", message=f"Room is full ({MAX_CLIENTS_PER_ROOM} max)"))
            await ws.close(4003, "room_full")
            return
        room.clients.add(ws)
        await send_json(ws, make_msg("CLIENT_HELLO", room_id, role="client", hostConnected=room.is_active))

        # Send latest state if available
        if room.last_state:
            await send_json(ws, json.dumps(room.last_state))

        # Notify host of new client
        if room.host and room.host.open:
            await send_json(room.host, make_msg("CLIENT_JOINED", room_id, clients=room.client_count))

        log.info(f"Client joined: room={room_id} total={room.client_count}")

    else:
        await send_json(ws, make_msg("ERROR", room_id, error="invalid_role", message=f"Unknown role: {role}"))
        await ws.close(4004, "invalid_role")
        return

    # ─── Message loop ────────────────────────────────────────────────────
    try:
        async for raw in ws:
            try:
                if len(raw) > MAX_MESSAGE_BYTES:
                    await send_json(ws, make_msg("ERROR", room_id, error="message_too_large",
                                                 message=f"Max {MAX_MESSAGE_BYTES} bytes"))
                    continue

                msg = json.loads(raw)
                msg_type = msg.get("type", "")

                # Host publishes state
                if role == "host" and msg_type == "STATE_UPDATE":
                    room.last_state = msg
                    # Broadcast to all clients
                    dead_clients = set()
                    for client in room.clients:
                        if client.open:
                            await send_json(client, raw)
                        else:
                            dead_clients.add(client)
                    room.clients -= dead_clients

                elif role == "host" and msg_type == "VOICE_LINE":
                    # Relay voice to clients
                    dead_clients = set()
                    for client in room.clients:
                        if client.open:
                            await send_json(client, raw)
                        else:
                            dead_clients.add(client)
                    room.clients -= dead_clients

                elif msg_type == "PING":
                    await send_json(ws, make_msg("PONG", room_id))

                else:
                    log.warning(f"Unexpected message type={msg_type} from role={role}")

            except json.JSONDecodeError:
                await send_json(ws, make_msg("ERROR", room_id, error="invalid_json", message="Invalid JSON"))

    except websockets.exceptions.ConnectionClosed:
        pass

    finally:
        # Cleanup on disconnect
        if role == "host":
            room.host = None
            # Notify clients
            dead_clients = set()
            for client in room.clients:
                if client.open:
                    await send_json(client, make_msg("HOST_LEFT", room_id, reason="host_disconnected"))
                else:
                    dead_clients.add(client)
            room.clients -= dead_clients
            log.info(f"Host left: room={room_id} clients={room.client_count}")

        elif role == "client":
            room.clients.discard(ws)
            if room.host and room.host.open:
                await send_json(room.host, make_msg("CLIENT_LEFT", room_id, clients=room.client_count))
            log.info(f"Client left: room={room_id} remaining={room.client_count}")

        # Clean empty rooms
        if room.host is None and room.client_count == 0:
            rooms.pop(room_id, None)
            log.info(f"Room removed (empty): {room_id}")


# ─── Main ─────────────────────────────────────────────────────────────────
async def main():
    log.info(f"Tasia Sync Server v{MOD_VERSION}")
    log.info(f"Port: {PORT}  Protocol: v{PROTOCOL_VERSION}")
    log.info(f"Max clients per room: {MAX_CLIENTS_PER_ROOM}")
    log.info(f"Token required: {ROOM_TOKEN_REQUIRED}")

    async with websockets.serve(handler, HOST, PORT, ping_interval=10, ping_timeout=5, max_size=MAX_MESSAGE_BYTES):
        log.info(f"Server listening on ws://{HOST}:{PORT}/")
        await asyncio.Future()  # run forever


if __name__ == "__main__":
    asyncio.run(main())
