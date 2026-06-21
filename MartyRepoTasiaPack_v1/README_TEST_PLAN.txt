╔══════════════════════════════════════════════════════════════╗
║  MartyRepoTasiaPack v1 — Test Plan                         ║
╚══════════════════════════════════════════════════════════════╝

Prerequisites:
- Host has full TasiaBotFriends installed with API keys
- Friend has TasiaFriendClient installed
- Both have same required dependencies (BepInEx, REPOLib, etc.)
- Configure VerboseLogs=true on host for debugging

═══════════════════════════════════════════════════════════════
TEST 1: Host-Only Regression
═══════════════════════════════════════════════════════════════
Steps:
1. Start REPO singleplayer.
2. F8 to spawn Tasia.
3. Test commands: "Tasia chodź tu", "Tasia czekaj", "Tasia zbieraj"
4. Test modes: "Tasia tryb follow", "Tasia tryb walki", "Tasia tryb czekania"
5. Test map query: "Tasia gdzie jesteś?", "Tasia gdzie loot?"
6. Test loot carry: pick up an item, carry to extraction.
7. Test extraction disappearance: observe behavior.
8. F11 god mode (if configured).
Expected:
- Tasia spawns, follows commands, changes modes.
- Map queries return real data from memory.
- Loot is carried carefully, extraction logic works.
- No teleport/snap movement.
- Mode changes logged as [TasiaMode].

═══════════════════════════════════════════════════════════════
TEST 2: Friend Joins with Micro Client
═══════════════════════════════════════════════════════════════
Steps:
1. Host starts lobby (private).
2. Friend joins.
3. Friend checks: does Tasia avatar appear?
4. Friend checks: is nameplate "Tasia" visible?
5. Host moves, friend checks: does Tasia move smoothly?
6. Friend checks: is there only ONE Tasia? (no duplicate)
Expected:
- Friend sees one Tasia avatar.
- Nameplate visible.
- Smooth interpolation, no jitter/teleport.
- No duplicate Tasia.
- Log on friend: [TasiaFriend] Avatar visual created.

═══════════════════════════════════════════════════════════════
TEST 3: Mode Sync
═══════════════════════════════════════════════════════════════
Steps:
1. Host says "Tasia tryb follow".
2. Friend checks: does avatar state change? (if debug visible)
3. Host says "Tasia tryb zbierania".
4. Host says "Tasia tryb walki".
5. Host says "Tasia tryb czekania".
Expected:
- Host sees mode changes logged as [TasiaMode].
- Friend sees consistent visual state.

═══════════════════════════════════════════════════════════════
TEST 4: Voice Sync
═══════════════════════════════════════════════════════════════
Steps:
1. Tasia says something (through AI or command).
2. Host should see speech bubble.
3. Friend should see speech bubble on avatar.
Expected:
- Host sees bubble.
- Friend sees bubble.
- If shared audio implemented, friend hears audio.
- If not, text bubble fallback is documented acceptable.

═══════════════════════════════════════════════════════════════
TEST 5: Map Query
═══════════════════════════════════════════════════════════════
Steps:
1. Say "Tasia gdzie jesteś?" → expect position relative to Marty.
2. Say "Tasia gdzie loot?" → expect known loot info or "nie wiem".
3. Say "Tasia gdzie extraction?" → expect extraction location or "nie wiem".
4. Say "Tasia co robisz?" → expect mode/intent report.
Expected:
- Answers from real WorldMemory, not hallucination.
- Unknown answers say "nie wiem" honestly.

═══════════════════════════════════════════════════════════════
TEST 6: Collect Mode
═══════════════════════════════════════════════════════════════
Steps:
1. Host mode = COLLECT.
2. No extraction exists → Tasia should NOT pick up loot.
3. Extraction exists → Tasia should pick up and carry carefully.
4. Extraction disappears while carrying → Tasia should stop, wait, retarget or place safe.
Expected:
- Delivery-first pickup works.
- No loot pickup without delivery plan.
- Careful carry: slow movement, safe drop.
- No item destruction during normal carry.

═══════════════════════════════════════════════════════════════
TEST 7: Stale Sync
═══════════════════════════════════════════════════════════════
Steps:
1. Friend is connected and seeing Tasia.
2. Host stops sending (e.g. leaves the game, pauses, or mod stops).
3. Friend observes Tasia behavior.
Expected:
- After ~2s: nameplate shows "Tasia (stale)" in gray.
- After ~5s: Tasia avatar hides/fades.
- No crash.
- No wild teleport.
- When host resumes, Tasia reappears smoothly.

═══════════════════════════════════════════════════════════════
TEST 8: Modpack Stability with Optional Mods
═══════════════════════════════════════════════════════════════
Steps:
1. Enable all stable maps/QoL/valuables/enemies from the mod list.
2. Start game.
3. Host lobby.
4. Friend joins.
5. Play one round.
Expected:
- Game starts without errors.
- Lobby works.
- No major BepInEx errors.
- Friend stays connected.
- Tasia visible throughout.

═══════════════════════════════════════════════════════════════
TEST 9: Optional Mods Not Enabled by Default
═══════════════════════════════════════════════════════════════
Steps:
1. Read README_OPTIONAL_MODS.txt.
2. Confirm optional mods are listed there, not in the main install.
Expected:
- Clear separation between stable and optional mods.
- Optional mods documented but not included in v1 default.
