#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# TasiaBotFriends — Build script
# Run this on the machine where R.E.P.O. is installed.
# It will locate the game, copy required DLLs, and build.
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
LIB_DIR="$PROJECT_DIR/lib"
PLUGIN_OUT="$PROJECT_DIR/bin/Release/net472/TasiaBotFriends.dll"

# ── Try to find R.E.P.O installation ──
REPO_DIR=""

# Common Steam paths
STEAM_PATHS=(
    "$HOME/.steam/steam/steamapps/common"
    "$HOME/.local/share/Steam/steamapps/common"
    "/mnt/c/Program Files (x86)/Steam/steamapps/common"
    "/mnt/games/Steam/steamapps/common"
    "/media/$USER/Steam/steamapps/common"
    "/opt/steam/steamapps/common"
)

for base in "${STEAM_PATHS[@]}"; do
    candidate="$base/REPO"
    if [ -d "$candidate" ]; then
        REPO_DIR="$candidate"
        break
    fi
    # Also try "R.E.P.O"
    candidate="$base/R.E.P.O"
    if [ -d "$candidate" ]; then
        REPO_DIR="$candidate"
        break
    fi
done

if [ -z "$REPO_DIR" ]; then
    echo "╔══════════════════════════════════════════════════════════╗"
    echo "║  Could not find R.E.P.O installation automatically.     ║"
    echo "║                                                        ║"
    echo "║  Set the REPO_DIR environment variable to your         ║"
    echo "║  R.E.P.O. game folder and re-run:                      ║"
    echo "║                                                        ║"
    echo "║    export REPO_DIR=/path/to/REPO                       ║"
    echo "║    ./build.sh                                          ║"
    echo "╚══════════════════════════════════════════════════════════╝"
    exit 1
fi

echo "✓ Found R.E.P.O. at: $REPO_DIR"

# Paths
MANAGED="$REPO_DIR/REPO_Data/Managed"
BEPINEX_CORE="$REPO_DIR/BepInEx/core"
UNITY_DIR="$MANAGED"

# ── Create lib directory ──
mkdir -p "$LIB_DIR"

# ── Copy BepInEx DLLs ──
echo "→ Copying BepInEx DLLs..."
BEPINEX_DLLS=(
    "BepInEx.dll"
    "BepInEx.Harmony.dll"
    "BepInEx.Preloader.dll"
    "HarmonyX.dll"
    "MonoMod.dll"
    "MonoMod.RuntimeDetour.dll"
    "MonoMod.Utils.dll"
)
for dll in "${BEPINEX_DLLS[@]}"; do
    if [ -f "$BEPINEX_CORE/$dll" ]; then
        cp "$BEPINEX_CORE/$dll" "$LIB_DIR/"
        echo "  ✓ $dll"
    else
        echo "  ⚠ $dll not found at $BEPINEX_CORE/$dll (non-fatal)"
    fi
done

# ── Copy Unity engine DLLs ──
echo "→ Copying Unity engine DLLs..."
UNITY_DLLS=(
    "UnityEngine.dll"
    "UnityEngine.CoreModule.dll"
    "UnityEngine.AIModule.dll"
    "UnityEngine.AudioModule.dll"
    "UnityEngine.PhysicsModule.dll"
    "UnityEngine.AnimationModule.dll"
    "UnityEngine.IMGUIModule.dll"
    "UnityEngine.InputModule.dll"
    "UnityEngine.TextRenderingModule.dll"
    "UnityEngine.ParticleSystemModule.dll"
    "UnityEngine.JSONSerializeModule.dll"
)
for dll in "${UNITY_DLLS[@]}"; do
    if [ -f "$UNITY_DIR/$dll" ]; then
        cp "$UNITY_DIR/$dll" "$LIB_DIR/"
        echo "  ✓ $dll"
    else
        echo "  ⚠ $dll not found (non-fatal)"
    fi
done

# ── Copy game assemblies ──
echo "→ Copying game assemblies..."
GAME_DLLS=(
    "Assembly-CSharp.dll"
    "Assembly-CSharp-firstpass.dll"
)
for dll in "${GAME_DLLS[@]}"; do
    if [ -f "$MANAGED/$dll" ]; then
        cp "$MANAGED/$dll" "$LIB_DIR/"
        echo "  ✓ $dll"
    else
        echo "  ✗ $dll not found! This is required."
        MISSING=1
    fi
done

if [ -n "${MISSING:-}" ]; then
    echo "ERROR: Required game assemblies are missing."
    exit 1
fi

# ── Copy Newtonsoft.Json from game if available ──
if [ -f "$MANAGED/Newtonsoft.Json.dll" ]; then
    cp "$MANAGED/Newtonsoft.Json.dll" "$LIB_DIR/"
    echo "  ✓ Newtonsoft.Json.dll (from game)"
fi

# ── Restore and build ──
echo ""
echo "→ Restoring NuGet packages..."
dotnet restore "$PROJECT_DIR"

echo ""
echo "→ Building TasiaBotFriends..."
dotnet build "$PROJECT_DIR" -c Release

# ── Result ──
if [ -f "$PLUGIN_OUT" ]; then
    SIZE=$(du -h "$PLUGIN_OUT" | cut -f1)
    echo ""
    echo "╔══════════════════════════════════════════════════╗"
    echo "║  ✅ BUILD SUCCESSFUL!                           ║"
    echo "║                                                 ║"
    echo "║  Output: $PLUGIN_OUT"
    echo "║  Size:   $SIZE"
    echo "╚══════════════════════════════════════════════════╝"
    echo ""
    echo "Install:"
    echo "  cp \"$PLUGIN_OUT\" \"$BEPINEX_CORE/../plugins/TasiaBotFriends.dll\""
    echo ""
    echo "Or copy manually to:"
    echo "  $REPO_DIR/BepInEx/plugins/TasiaBotFriends.dll"
else
    echo "ERROR: Build failed. Check the output above."
    exit 1
fi
