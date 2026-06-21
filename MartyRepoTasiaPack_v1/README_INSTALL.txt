╔══════════════════════════════════════════════════════════════╗
║  MartyRepoTasiaPack v1 — Stable Friends Build               ║
║  Install Instructions                                       ║
╚══════════════════════════════════════════════════════════════╝

TWO PACKAGES:
  Host_Marty/   → for the host (Marty). Runs full Tasia AI brain.
  Friend_Client/ → for friends. Lightweight sync receiver only.

═══════════════════════════════════════════════════════════════
FOR MARTY (HOST)
═══════════════════════════════════════════════════════════════

1. Install R.E.P.O. on your machine.

2. Install required dependencies via r2modman or manually:
   - BepInExPack
   - REPOLib
   - MenuLib
   - REPOConfig (recommended)

3. Copy the contents of Host_Marty/ into your R.E.P.O. game folder:
   - BepInEx/plugins/TasiaBotFriends.dll
   - BepInEx/config/Tasia.BotFriends.cfg.template
     → Rename to Tasia.BotFriends.cfg after copying
     → Open Tasia.BotFriends.cfg in a text editor
     → Add your AI API key in the [AI] section (ApiKey field)
     → Optionally set ApiUrl and Model for your provider

4. Optional: install any map/QoL mods from the mod list.

5. Start the game. Tasia should spawn automatically after the level loads.

6. Host a lobby. Friends with the friend client can join.

IMPORTANT:
- Never share your Tasia.BotFriends.cfg file with friends.
- It contains your personal API keys.
- Only the Host_Marty folder goes on your machine.

═══════════════════════════════════════════════════════════════
FOR FRIENDS
═══════════════════════════════════════════════════════════════

1. Install R.E.P.O. on your machine.

2. Install the same required dependencies:
   - BepInExPack
   - REPOLib
   - MenuLib
   - REPOConfig (optional but recommended)

3. Copy ONLY the contents of Friend_Client/ into your game folder:
   - BepInEx/plugins/TasiaFriendClient.dll
   - BepInEx/config/Tasia.FriendClient.cfg.template
     → Rename to Tasia.FriendClient.cfg after copying
     → Open and optionally adjust settings (defaults work fine)

4. DO NOT install TasiaBotFriends.dll. DO NOT add API keys.

5. Optional: install matching map/QoL mods if the host uses them.

6. Start the game and join Marty's lobby.

7. You should see Tasia as a pink robot avatar with nameplate.
   You will see her speech bubbles when she talks.
   You do NOT need to configure any AI settings.

═══════════════════════════════════════════════════════════════
MANUAL INSTALL (without mod manager)
═══════════════════════════════════════════════════════════════

All files should be placed in:
C:\Program Files (x86)\Steam\steamapps\common\REPO\

Or wherever your R.E.P.O. is installed.

Layout for HOST:
  REPO/
  ├── BepInEx/
  │   ├── plugins/
  │   │   ├── TasiaBotFriends.dll
  │   │   └── ... (other mods)
  │   └── config/
  │       ├── Tasia.BotFriends.cfg
  │       └── ... (other configs)
  └── ... (game files)

Layout for FRIEND:
  REPO/
  ├── BepInEx/
  │   ├── plugins/
  │   │   ├── TasiaFriendClient.dll
  │   │   └── ... (other mods)
  │   └── config/
  │       ├── Tasia.FriendClient.cfg
  │       └── ... (other configs)
  └── ... (game files)

═══════════════════════════════════════════════════════════════
WARNINGS
═══════════════════════════════════════════════════════════════

- Friends must NOT install TasiaBotFriends.dll.
  If they do, they may get duplicate Tasia or broken sync.

- The Friend Client does NOT run the AI brain.
  It only receives and displays what the host sends.

- Host must have API keys configured. Friends do not need them.

- Map, QoL, and content mods must match between host and friends
  for stable multiplayer (same versions).
