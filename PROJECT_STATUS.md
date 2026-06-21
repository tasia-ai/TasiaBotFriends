# TasiaBotFriends — Project Status

## Overview
TasiaBotFriends is a **single-DLL autonomous teammate mod** for R.E.P.O. (BepInEx 5.x).

**Version:** 1.2.0
**File:** `/home/marty/tasiabotfriends/bin/Release/net472/TasiaBotFriends.dll`

## Architecture (v1.2.0)

### Components
| Component | Purpose |
|-----------|---------|
| `TasiaBotFriendsPlugin` | Main plugin: config, spawn, keybinds, AI loop, chat, Harmony patches (god-mode) |
| `TasiaCommandParser` | Static Polish keyword parser for natural player commands |
| `TasiaRuntimeRunner` | Dedicated Unity GameObject that drives coroutines/Update (lifecycle fix) |
| `TasiaBotBrain` | **State machine + decision + movement.** Core autonomous teammate brain |
| `TasiaBotCarrier` | Loot pickup, carry, extractor delivery |
| `TasiaBotWeaponUser` | Gun pickup, aiming, shooting (incl. starter gun cloning) |
| `VectorExtensions` | `FlatY()` utility |

### State Machine (TasiaBotBrain)
**Priority order (highest to lowest):**
1. `ImmediateDanger` — survival, flee from monsters
2. `DirectCommand` — Marty's override via chat/keywords
3. `HelpTeammates` — assist nearby players
4. `CarryExtract` — delivering valuable loot
5. `SearchLoot` — looking for valuables
6. `Idle` — wait, observe

**States:** `Idle`, `Searching`, `GoingToLoot`, `CarryingLoot`, `Delivering`, `Following`, `Protecting`, `Fleeing`, `Waiting`, `ComingToPlayer`, `Helping`, `Hiding`

### Command Parser (Polish)
Understands natural commands including profanity:
| Command | Polish keywords |
|---------|----------------|
| come | chodź, podejdź, do mnie, tutaj, wracaj |
| follow | za mną, idź za, follow |
| flee/hide | uciekaj, chowaj, kryj, zmykaj |
| help | pomóż, ratuj, pomoc |
| wait | czekaj, stój, zostań |
| deliver/drop | zostaw, upuść, rzuć |
| fight | walcz, strzelaj, zabij |
| search | szukaj, zbieraj, loot |

### AI Integration
- OpenAI-compatible API via HTTP
- System prompt in Polish
- Autonomous decision-making with priority awareness
- Command override: Marty's commands interrupt non-critical tasks
- Chat hook: replies to messages containing "Tasia" or commands via `/`

### Voice & Speech
- Text bubbles above Tasia (TextMesh)
- TTS HTTP request sent (no local audio playback yet — needs UnityWebRequestAudioModule)
- AudioSource component attached to Tasia for future playback

## Deploy History
| Date | Version | Size | What |
|------|---------|------|------|
| 2026-06-21 | 1.0.0 | 38912 | Initial own-core |
| 2026-06-21 | 1.1.0 | 43008 | Runtime runner + diagnostic |
| 2026-06-21 | 1.1.1 | 44544 | Lifecycle fix |
| 2026-06-21 | 1.1.2 | 53248 | Standalone mode, pink robot, personal space, map-aware |
| **2026-06-21** | **1.2.0** | **58880** | **Brain v2: state machine, command parser, priority system, Polish AI** |

## Next Steps
1. **Test in game:** autonomous behavior, command parsing, priority system
2. **REPOLib integration:** network visibility for multiplayer (all players see Tasia)
3. **TTS audio playback:** add UnityWebRequestAudioModule DLL reference for `AudioClip` loading
4. **Cosmetics:** REPO avatar clone + customizable elements (shirt, hair, skirt)
5. **Ragdoll physics:** prevent falling over/tripping
6. **Improved monster avoidance:** smarter pathfinding around threats
7. **Idle chatter:** personality-driven random lines when no task active

## Known Limitations
- AudioSource is attached but TTS MP3 playback not yet implemented (needs missing Unity DLL)
- Network visibility: local/host only. REPOLib's network prefab spawning needed for all clients
- Visual: runtime primitives only. AssetBundle/cosmetics system coming after brain stability
- No Photon/replication hacks — legitimate modded setup only
