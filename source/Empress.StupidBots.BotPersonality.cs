using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Empress.StupidBots;

internal class BotPersonality : MonoBehaviour
{
	[field: CompilerGenerated]
	public BotPersonalityType Type
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public float SpeedMultiplier
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = 1f;


	[field: CompilerGenerated]
	public bool AllowValuables
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = true;


	[field: CompilerGenerated]
	public bool AllowWeapons
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = true;


	[field: CompilerGenerated]
	public float AimConeDegrees
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = 6f;


	[field: CompilerGenerated]
	public Vector2 FireInterval
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = new Vector2(0.9f, 1.6f);


	[field: CompilerGenerated]
	public float ToyingChance
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public float DropChanceOnShoot
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	public void InitRandom(int seed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Random val = new Random(seed);
		BotPersonalityType[] array = (BotPersonalityType[])(object)Enum.GetValues(typeof(BotPersonalityType));
		Type = array[val.Next(array.Length)];
		ApplyDefaults(Type);
	}

	public void ApplyDefaults(BotPersonalityType t)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		Type = t;
		SpeedMultiplier = 1f;
		AllowValuables = true;
		AllowWeapons = true;
		AimConeDegrees = 6f;
		FireInterval = new Vector2(0.9f, 1.6f);
		ToyingChance = 0f;
		DropChanceOnShoot = 0f;
		switch (t)
		{
		case BotPersonalityType.Clumsy:
			SpeedMultiplier = 0.95f;
			AimConeDegrees = 10f;
			DropChanceOnShoot = 0.12f;
			break;
		case BotPersonalityType.Fast:
			SpeedMultiplier = 1.25f;
			AimConeDegrees = 7f;
			FireInterval = new Vector2(0.6f, 1f);
			break;
		case BotPersonalityType.Slow:
			SpeedMultiplier = 0.7f;
			AimConeDegrees = 8f;
			FireInterval = new Vector2(1.2f, 2f);
			break;
		case BotPersonalityType.Aggressive:
			SpeedMultiplier = 1.1f;
			AimConeDegrees = 5f;
			FireInterval = new Vector2(0.35f, 0.75f);
			break;
		case BotPersonalityType.Coward:
			SpeedMultiplier = 1f;
			AimConeDegrees = 12f;
			FireInterval = new Vector2(1.5f, 2.2f);
			AllowValuables = false;
			break;
		case BotPersonalityType.Curious:
			SpeedMultiplier = 1f;
			AimConeDegrees = 9f;
			FireInterval = new Vector2(0.8f, 1.6f);
			ToyingChance = 0.25f;
			break;
		case BotPersonalityType.Guardian:
			SpeedMultiplier = 0.95f;
			AimConeDegrees = 4f;
			FireInterval = new Vector2(0.55f, 1.1f);
			break;
		case BotPersonalityType.Jittery:
			SpeedMultiplier = 1.1f;
			AimConeDegrees = 14f;
			FireInterval = new Vector2(0.25f, 0.6f);
			break;
		case BotPersonalityType.Pacifist:
			SpeedMultiplier = 1f;
			AllowWeapons = false;
			break;
		case BotPersonalityType.Trickster:
			SpeedMultiplier = 1f;
			AimConeDegrees = 18f;
			FireInterval = new Vector2(0.4f, 1.2f);
			ToyingChance = 0.5f;
			break;
		}
	}
}
