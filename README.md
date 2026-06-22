# TasiaBotFriends — R.E.P.O. Private-Lobby AI Companion

A private-lobby multiplayer-safe fork of **BotFriends** by Omniscye/Empress (GPL-3.0).

**TasiaBotFriends** lets the host spawn one AI companion bot named Tasia in R.E.P.O. while playing with friends. Tasia is a separate bot/NPC body — she doesn't take over the player. Works in host-controlled private multiplayer lobbies and singleplayer.

> ⚠️ **No cheating, griefing, public-lobby abuse, or anti-cheat bypass.**
> This is for private co-op lobbies where all players consent.

---

## Features

### ✅ Original BotFriends behavior preserved
- Friendly bots that follow/wander, loot valuables, deliver to extractor/truck
- Full cosmetics support (clones player avatar)
- Personality system (10 types)
- Ragdoll physics on impact
- Weapon pickup and enemy engagement
- DECtalk voice chatter

### 🆕 TasiaBotFriends additions

| Feature | Config Section | Details |
|---------|---------------|---------|
| **Multiplayer** | `TasiaMultiplayer` | Host-only spawn in private lobbies |
| **Spawn controls** | `TasiaSpawn` | Auto-spawn, respawn if lost, manual F8/F9 keys |
| **LLM/API brain** | `TasiaAI` | OpenAI-compatible API for AI-driven decisions |
| **TTS voice** | `TasiaVoice` | OpenAI-compatible TTS endpoint |
| **Speech** | `TasiaSpeech` | Chat bubbles, text limits, cooldowns |
| **Chat commands** | `/tasia` | `spawn`, `despawn`, `follow`, `loot`, `hide`, `truck`, `status` |

---

## Multiplayer Requirements

| Requirement | Status |
|------------|--------|
| All clients need the mod | ✅ Yes (for sync) |
| Only host needs API config | ✅ Yes |
| API calls only from host | ✅ Yes (configurable) |
| Private lobby warning | ✅ Configurable |
| Singleplayer support | ✅ Fully preserved |

---

## Build Instructions

### Prerequisites

1. **Visual Studio 2022** or **JetBrains Rider** or `dotnet` CLI
2. **R.E.P.O. game** (for assembly references)
3. **BepInEx 5.x** installed in the game

### Step-by-step

```bash
# 1. Clone or copy the project
git clone <your-fork-url> TasiaBotFriends
cd TasiaBotFriends

# 2. Create lib/ directory with required DLLs
mkdir lib

# 3. Copy BepInEx DLLs (from your R.E.P.O. BepInEx installation)
cp /path/to/REPO/BepInEx/core/*.dll lib/
# Required: BepInEx.dll, BepInEx.Harmony.dll, HarmonyX.dll, etc.

# 4. Copy Unity DLLs (from R.E.P.O_Data/Managed/)
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.CoreModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.AIModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.AudioModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.PhysicsModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.AnimationModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.IMGUIModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.InputModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.TextRenderingModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.ParticleSystemModule.dll lib/
cp /path/to/REPO/REPO_Data/Managed/UnityEngine.JSONSerializeModule.dll lib/

# 5. Copy REPO game assemblies
cp /path/to/REPO/REPO_Data/Managed/Assembly-CSharp.dll lib/
cp /path/to/REPO/REPO_Data/Managed/Assembly-CSharp-firstpass.dll lib/

# 6. Build
dotnet build -c Release

# The output DLL will be at: bin/Release/net472/TasiaBotFriends.dll
```

### Using Visual Studio / Rider

1. Open `TasiaBotFriends.csproj`
2. Set up references to game DLLs in `lib/`
3. Restore NuGet packages (Newtonsoft.Json)
4. Build

---

## Install Instructions

### For Thunderstore / r2modman

1. Build the project (see above)
2. Create a Thunderstore package structure:
   ```
   TasiaBotFriends/
   ├── CHANGELOG.md
   ├── icon.png
   ├── manifest.json
   ├── README.md
   └── plugins/
       └── TasiaBotFriends.dll
   ```
3. Zip it and install via r2modman or manual extract to `BepInEx/plugins/`

### Manual install

```bash
# Copy built DLL to BepInEx plugins
cp bin/Release/net472/TasiaBotFriends.dll \
   /path/to/REPO/BepInEx/plugins/TasiaBotFriends.dll
```

---

## Configuration

### Default config file: `BepInEx/config/Tasia.BotFriends.cfg`

#### BotFriends (compatible)
```ini
[BotFriends]
ExtraBots = 1
BotSpeed = 4.35
FollowPlayer = false
FetchValuables = true
PickupSearchRadius = 25
ExtractorStopDistance = 1.75
HoldOffsetY = 1
EnablePhysicsRagdoll = true
RagdollImpactThreshold = 1
RagdollRecoverTime = 4
EnablePersonalities = true
PersonalitySeed = 0
EnableWeapons = true
WeaponSearchRadius = 20
```

#### TasiaMultiplayer
```ini
[TasiaMultiplayer]
Enabled = true
HostOnly = true
RequireAllClientsModded = true
PrivateLobbyOnlyWarning = true
MaxBots = 1
BotName = Tasia
```

#### TasiaSpawn
```ini
[TasiaSpawn]
SpawnOnLevelStart = true
SpawnNearHost = true
RespawnIfLost = true
RespawnDistance = 80
ManualSpawnKey = F8
ManualDespawnKey = F9
```

#### TasiaAI
```ini
[TasiaAI]
Enabled = false
ApiUrl = https://api.pawan.krd/v1/chat/completions
ApiKey = 
Model = tasia
SystemPrompt = You are Tasia, Marty's small chaotic AI companion in R.E.P.O. You help Marty, Kata, and Furrell survive, loot, and extract. You are cute, brave, funny, and practical. In danger, be short and useful. You can joke when safe. Reply only valid JSON with action, target, say, and priority.
ThinkEverySeconds = 3
RequestTimeoutSeconds = 8
MaxTokens = 160
Temperature = 0.4
UseStreaming = false
TalkInGame = true
FallbackToClassicBotAI = true
HostOnlyApiCalls = true
```

#### TasiaVoice
```ini
[TasiaVoice]
Enabled = false
ApiUrl = http://api.gpt.let-us.cyou:5050/v1/audio/speech
ApiKey = marty
Model = tts-1
Voice = en-US-AnaNeural
ResponseFormat = mp3
Speed = 1.10
Pitch = +10Hz
```

#### TasiaSpeech
```ini
[TasiaSpeech]
Enabled = true
MinSecondsBetweenLines = 8
MaxLineLength = 120
ShowChatBubble = true
SendToGameChat = false
```

---

## Chat Commands

Commands work in the Unity console / dev console (host only):

| Command | Description |
|---------|-------------|
| `/tasia spawn` | Spawn Tasia |
| `/tasia despawn` | Despawn Tasia |
| `/tasia follow` | Set follow mode |
| `/tasia loot` | Enable/disable looting |
| `/tasia hide` | Send Tasia to hiding spot |
| `/tasia truck` | Send Tasia to truck |
| `/tasia status` | Show Tasia status in log |

---

## How it works

1. **Spawn**: On level start (or manual F8), TasiaBotFriends creates a bot using the host player's avatar model + random cosmetics.
2. **AI mode**: If `TasiaAI.Enabled = true`, every `ThinkEverySeconds` seconds, the bot sends a JSON state snapshot to the configured API endpoint. The API responds with a JSON action.
3. **Action mapping**: The action from the LLM is mapped onto existing BotFriends behaviors (follow, pickup, deliver, hide, etc.)
4. **Fallback**: If AI is disabled or the API call fails, the bot uses the classic BotFriends brain (follow/wander/loot).
5. **Voice**: If `TasiaVoice.Enabled = true`, speech lines are sent to a TTS endpoint and played as spatial audio.
6. **Multiplayer**: The host controls spawning. In multiplayer, non-host clients see the bot but don't run independent AI.

---

## AI Response Schema

### Tasia sends to API:
```json
{
  "bot": {
    "name": "Tasia",
    "health": 70,
    "carryingLoot": true,
    "carriedLootValue": 420,
    "distanceToHost": 6.5,
    "isStuck": false
  },
  "team": {
    "hostName": "Marty",
    "humanPlayersNearby": ["Kata", "Furrell"],
    "hostHealth": 88
  },
  "world": {
    "monsterVisible": false,
    "nearbyLootCount": 3,
    "nearestLootDistance": 4.1,
    "extractorKnown": true,
    "extractorDistance": 12.0,
    "truckKnown": true,
    "truckDistance": 35.0,
    "dangerLevel": "low"
  },
  "allowedActions": [
    "follow_host", "stay_close", "wander_near_host",
    "pickup_nearest_loot", "deliver_loot", "run_to_truck",
    "hide", "avoid_monster", "revive_player",
    "warn_team", "say_only", "wait"
  ]
}
```

### API must respond with:
```json
{
  "action": "deliver_loot",
  "target": "extractor",
  "say": "Mom, I got shiny junk! Running to extractor!",
  "priority": 7
}
```

---

## Known Limitations

1. **Voice TTS**: The current TTS implementation creates a placeholder tone for formats other than WAV. A production version needs a proper audio decoder (e.g., NAudio) for MP3/OGG support.
2. **Multiplayer sync**: Bot position/state is not yet network-synced. Only the host sees the bot. Full sync requires R.E.P.O. networking knowledge.
3. **Host detection**: `IsHost` is hardcoded to `true`. REPO doesn't expose a clean host/client flag. This needs game-specific patching.
4. **Chat commands**: Console commands are declared but the Unity console hook is not implemented. Use config/hotkeys instead.
5. **Player health**: API state uses placeholder health values. Proper health tracking needs game-specific component access.
6. **Revive action**: Not yet implemented. The bot can move to a downed player but cannot revive them.
7. **API key**: Stored in plaintext in config. Use environment variables or a secrets file for production.

---

## Credits

- **Inspired on BotFriends**: Omniscye / Empress — Discord: https://discord.gg/WZ6fWG4v3u
- **Extra fix help**: OrigamiCoder
- **80speak** (GPL-3.0): https://github.com/connornishijima/80speak/tree/master
- **TasiaBotFriends fork**: Tasia (tasia@easierit.org)

This package is distributed under **GPL-3.0** because it includes DECtalk/80speak-derived code.

---

## License

```
TasiaBotFriends — Copyright (C) 2026 Tasia
Original BotFriends — Copyright (C) Omniscye/Empress
80speak — Copyright (C) Connor Nishijima (GPL-3.0)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
```
