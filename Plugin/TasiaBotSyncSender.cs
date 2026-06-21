using UnityEngine;

namespace TasiaBotFriends;

/// <summary>
/// Attached to Tasia on the host. Sends state to external WebSocket sync server.
/// </summary>
internal sealed class TasiaBotSyncSender : MonoBehaviour
{
    private Transform _tform;
    private TasiaBotBrain _brain;
    private TasiaBotCarrier _carrier;

    private void Start()
    {
        _tform = transform;
        _brain = GetComponent<TasiaBotBrain>();
        _carrier = GetComponent<TasiaBotCarrier>();
    }

    private void Update()
    {
        if (!TasiaBotFriendsPlugin.IsGameplayReady()) return;
        if (!TasiaExternalSync.Enabled || !TasiaExternalSync.Connected) return;

        var pos = _tform.position;
        var rotY = _tform.eulerAngles.y;
        var mode = _brain?.CurrentMode.ToString() ?? "IDLE";
        var intent = _brain?.CurrentIntent.ToString() ?? "NONE";
        var carry = _carrier != null && _carrier.IsCarrying;
        var danger = "low";
        if (_brain?.Perception != null)
            danger = _brain.Perception.DangerLevel;

        TasiaExternalSync.SendState(pos, rotY, mode, intent, carry, danger, active: true);
    }
}
