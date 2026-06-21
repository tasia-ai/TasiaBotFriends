using System;
using System.Collections.Generic;
using UnityEngine;

namespace Empress.StupidBots;

internal static class BotUtils : Object
{
	private static readonly Dictionary<int, StupidBotCarrier> Claims = new Dictionary<int, StupidBotCarrier>();

	private static readonly Dictionary<int, float> IgnoreUntil = new Dictionary<int, float>();

	internal static List<PhysGrabObject> FindNearbyValuables(Vector3 position, float range)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<PhysGrabObject> val = new List<PhysGrabObject>();
		try
		{
			List<PhysGrabObject> val2 = SemiFunc.PhysGrabObjectGetAllWithinRange(range, position, true, default(LayerMask), (PhysGrabObject)null);
			for (int i = 0; i < val2.Count; i++)
			{
				PhysGrabObject val3 = val2[i];
				if (Object.op_Implicit((Object)(object)val3) && Object.op_Implicit((Object)(object)((Component)val3).GetComponent<ValuableObject>()))
				{
					val.Add(val3);
				}
			}
		}
		catch (Object)
		{
		}
		return val;
	}

	internal static bool TryClaim(PhysGrabObject pgo, StupidBotCarrier owner)
	{
		if (!Object.op_Implicit((Object)(object)pgo))
		{
			return false;
		}
		int instanceID = ((Object)pgo).GetInstanceID();
		if (IsIgnored(pgo))
		{
			return false;
		}
		StupidBotCarrier stupidBotCarrier = default(StupidBotCarrier);
		if (Claims.TryGetValue(instanceID, ref stupidBotCarrier) && Object.op_Implicit((Object)(object)stupidBotCarrier) && (Object)(object)stupidBotCarrier != (Object)(object)owner)
		{
			return false;
		}
		Claims[instanceID] = owner;
		return true;
	}

	internal static void ReleaseClaim(PhysGrabObject pgo, StupidBotCarrier owner)
	{
		if (Object.op_Implicit((Object)(object)pgo))
		{
			int instanceID = ((Object)pgo).GetInstanceID();
			StupidBotCarrier stupidBotCarrier = default(StupidBotCarrier);
			if (Claims.TryGetValue(instanceID, ref stupidBotCarrier) && (Object)(object)stupidBotCarrier == (Object)(object)owner)
			{
				Claims.Remove(instanceID);
			}
		}
	}

	internal static bool IsClaimedByOther(PhysGrabObject pgo, StupidBotCarrier owner)
	{
		if (!Object.op_Implicit((Object)(object)pgo))
		{
			return false;
		}
		int instanceID = ((Object)pgo).GetInstanceID();
		StupidBotCarrier stupidBotCarrier = default(StupidBotCarrier);
		if (!Claims.TryGetValue(instanceID, ref stupidBotCarrier))
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)stupidBotCarrier))
		{
			return (Object)(object)stupidBotCarrier != (Object)(object)owner;
		}
		return false;
	}

	internal static void IgnoreFor(PhysGrabObject pgo, float seconds)
	{
		if (Object.op_Implicit((Object)(object)pgo))
		{
			int instanceID = ((Object)pgo).GetInstanceID();
			IgnoreUntil[instanceID] = Time.time + Mathf.Max(0f, seconds);
			Claims.Remove(instanceID);
		}
	}

	internal static bool IsIgnored(PhysGrabObject pgo)
	{
		if (!Object.op_Implicit((Object)(object)pgo))
		{
			return false;
		}
		int instanceID = ((Object)pgo).GetInstanceID();
		float num = default(float);
		if (!IgnoreUntil.TryGetValue(instanceID, ref num))
		{
			return false;
		}
		return num > Time.time;
	}

	internal static Vector3 WithY(this Vector3 v, float y)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		v.y = y;
		return v;
	}
}
