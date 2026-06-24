using UnityEngine;

namespace TasiaBotFriends;

/// <summary>Visible state of Tasia that Network can read.</summary>
public struct TasiaVisibleState
{
    public bool   Active;
    public Vector3 Position;
    public float  RotationY;
    public string Intent;
    public string Mode;
    public bool   IsCarrying;
    public string SpeechText;
    public bool   IsSpeaking;
}

/// <summary>Public API for TasiaNetwork to read Tasia's state.</summary>
public interface ITasiaNetworkStateProvider
{
    bool HasActiveTasia { get; }
    TasiaVisibleState GetVisibleState();
}
