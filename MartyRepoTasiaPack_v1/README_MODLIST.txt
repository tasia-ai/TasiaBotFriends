# MartyRepoTasiaPack v1 — Mod List

## Core Mods (Required for both Host and Friend)

### BepInExPack
- Package: BepInEx-BepInExPack
- Version: 5.4.22 or later
- Purpose: Mod loader for R.E.P.O.
- Required: yes
- Install: both host and friend
- Source: Thunderstore

### REPOLib
- Package: Zehs-REPOLib
- Version: latest stable
- Purpose: Library for network prefabs, content registration
- Required: yes
- Install: both host and friend
- Notes: Required by many mods including Tasia

### MenuLib
- Package: Kesomannen-MenuLib
- Version: latest stable
- Purpose: UI library for mod menus
- Required: yes
- Install: both host and friend
- Source: Thunderstore

### REPOConfig
- Package: Kesomannen-REPOConfig
- Version: latest stable
- Purpose: In-game config editor
- Required: yes (recommended)
- Install: both host and friend
- Source: Thunderstore

## Tasia Mods (This Pack)

### TasiaBotFriends (Host only)
- Package: N/A (custom)
- Version: 1.2.x (commit 3eea96d)
- Purpose: Full AI teammate with brain, movement, loot, modes, HUD
- Required: yes for host, NO for friend
- Install: Host_Marty/BepInEx/plugins/
- Notes: Requires API keys configured locally

### TasiaFriendClient (Friend only)
- Package: N/A (custom)
- Version: 1.0.0
- Purpose: Lightweight sync receiver, renders Tasia avatar/voice
- Required: yes for friend, NO for host
- Install: Friend_Client/BepInEx/plugins/
- Notes: No API keys needed. Does not run brain.

## Maps (Optional, all players need them)

### MinecraftStrongholdLevel
- Package: TBA (Thunderstore)
- Purpose: Minecraft stronghold themed level
- Required: no
- Install: both host and friend if used

### FNAFLevel
- Package: TBA
- Purpose: Five Nights at Freddy's themed level
- Required: no

### Tolian Levels
- Package: Tolian-TolianLevels
- Purpose: Additional level pack
- Required: no

### Wesleys Levels
- Package: Wesleys-WesleysLevels
- Purpose: Additional level pack
- Required: no

### Deeproot Garden
- Package: TBA
- Purpose: Garden themed level
- Required: no

### Minecraft Village
- Package: TBA
- Purpose: Minecraft village themed level
- Required: no

### MapVote
- Package: TBA
- Purpose: Vote for next map in multiplayer
- Required: no (but recommended for friends)

## QoL (Quality of Life, Optional)

### MoreUpgrades
- Package: TBA
- Purpose: More upgrade options in truck
- Required: no

### ExtractionPointConfirmButton
- Package: TBA
- Purpose: Confirm before extracting
- Required: no

### DeadTTS
- Package: TBATTS-DeadTTS
- Purpose: Text-to-speech for dead players
- Required: no

### MoreReviveHP
- Package: TBA
- Purpose: More health after revive
- Required: no

### BetterTruckHeals
- Package: TBA
- Purpose: Truck heals more effectively
- Required: no

### MapValueTracker (chosen over FindRemainingValuables)
- Package: TBA
- Purpose: Track remaining loot value on map
- Required: no
- Notes: Chosen for v1 as it's more stable and less intrusive than FindRemainingValuables

### PostLevelSummary
- Package: TBA
- Purpose: Show level summary after extraction
- Required: no

## Valuables (Optional)

### LethalCompanyValuables
- Package: TBA
- Purpose: Lethal Company themed valuables
- Required: no

### Wesleys Valuables
- Package: Wesleys-WesleysValuables
- Purpose: Additional valuable items
- Required: no

### REPOing Valuables
- Package: TBA
- Purpose: Additional valuable items
- Required: no

## Enemies (Optional)

### Wesleys Enemies
- Package: Wesleys-WesleysEnemies
- Purpose: Additional enemy types
- Required: no

---
Note: Exact Thunderstore package IDs need to be verified before publishing.
Check https://thunderstore.io/c/repo/ for current package names and versions.
