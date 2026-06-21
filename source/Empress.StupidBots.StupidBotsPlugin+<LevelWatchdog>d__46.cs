using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Empress.StupidBots;

[BepInPlugin("Empress.BotFriends", "Empress BotFriends", "1.1.3")]
public class StupidBotsPlugin : BaseUnityPlugin
{
	[CompilerGenerated]
	private sealed class <>c__DisplayClass51_0 : Object
	{
		public Transform player;

		internal Pose <SpawnBots>b__0(int i)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = player.position;
			Vector3 val = Random.onUnitSphere.WithY(0f);
			return new Pose(position + ((Vector3)(ref val)).normalized * (2f + (float)i * 1.25f), Quaternion.identity);
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass52_0 : Object
	{
		public Vector3 reference;

		internal float <FindOrderedSpawnPoints>b__1(SpawnPoint p)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.SqrMagnitude(((Component)p).transform.position - reference);
		}
	}

	private static readonly string[] NamePool = (string[])(object)new String[50]
	{
		"1A3", "753", "Ada", "Adalade Kasner", "Atom", "Bishop", "Blok", "Bocon", "Bolt", "Cinder",
		"Circuit", "Dex", "Doppelclick", "Drift", "Echo", "Ember", "Empress", "Endershade", "Frost", "Friendly",
		"Goat", "Gremlin", "Hex", "Impboy", "Ivy", "Jettcodey", "Jinx", "Kilo", "Lumen", "Mako",
		"Nova", "Omniscye", "Onyx", "Origami", "Pixel", "Quark", "Rex", "Rune", "Sable", "Skript",
		"Sizzlium", "Swaggies", "Talon", "Tangle", "Vanta", "Warden", "Zephyr", "Zehs", "gaymer", "s1ckboy"
	};

	private const int CountMin = 0;

	private const int CountMax = 5;

	private const float SpeedMin = 0.5f;

	private const float SpeedMax = 6f;

	private const float SearchRadiusMin = 2f;

	private const float SearchRadiusMax = 40f;

	private const float ExtractorStopMin = 0.5f;

	private const float ExtractorStopMax = 4f;

	private const float HoldOffsetMin = 0.2f;

	private const float HoldOffsetMax = 2f;

	private const float FollowOrbitRadius = 12f;

	public const string PluginGuid = "Empress.BotFriends";

	public const string PluginName = "Empress BotFriends";

	public const string PluginVersion = "1.1.3";

	private ConfigEntry<int> _botCount;

	private ConfigEntry<float> _botSpeed;

	private ConfigEntry<bool> _botsFollowPlayer;

	private ConfigEntry<bool> _botsFetchValuables;

	private ConfigEntry<float> _pickupSearchRadius;

	private ConfigEntry<float> _extractorStopDistance;

	private ConfigEntry<float> _holdOffsetY;

	private ConfigEntry<bool> _enableRagdoll;

	private ConfigEntry<float> _ragdollImpactThreshold;

	private ConfigEntry<float> _ragdollRecoverTime;

	private readonly List<GameObject> _bots = new List<GameObject>();

	private ConfigEntry<bool> _enablePersonalities;

	private ConfigEntry<int> _personalitySeed;

	private ConfigEntry<bool> _enableWeapons;

	private ConfigEntry<float> _weaponSearchRadius;

	private bool _spawnedThisLevel;

	private static Queue<string> _nameBag;

	[field: CompilerGenerated]
	internal static StupidBotsPlugin Instance
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	} = null;


	internal static ManualLogSource Logger => Instance._logger;

	private ManualLogSource _logger => ((BaseUnityPlugin)this).Logger;

	[field: CompilerGenerated]
	internal Harmony? EmpressHarmony
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		set;
	}

	private void Awake()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		Instance = this;
		((Component)this).gameObject.transform.parent = null;
		((Object)((Component)this).gameObject).hideFlags = (HideFlags)61;
		_botCount = ((BaseUnityPlugin)this).Config.Bind<int>("BotFriends", "ExtraBots", 1, new ConfigDescription("How many stupid bot friends to spawn (0-5).", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 5), Array.Empty<object>()));
		_botSpeed = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "BotSpeed", 4.35f, new ConfigDescription("Move speed for bots.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.5f, 6f), Array.Empty<object>()));
		_botsFollowPlayer = ((BaseUnityPlugin)this).Config.Bind<bool>("BotFriends", "FollowPlayer", false, "If true, bots loosely follow you. If false, they roam globally.");
		_botsFetchValuables = ((BaseUnityPlugin)this).Config.Bind<bool>("BotFriends", "FetchValuables", true, "If true, bots will fetch nearby valuables to the active extractor.");
		_pickupSearchRadius = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "PickupSearchRadius", 25f, new ConfigDescription("Scan radius for valuables.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(2f, 40f), Array.Empty<object>()));
		_extractorStopDistance = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "ExtractorStopDistance", 1.75f, new ConfigDescription("Drop distance from extractor.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.5f, 4f), Array.Empty<object>()));
		_holdOffsetY = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "HoldOffsetY", 1f, new ConfigDescription("Local Y offset for held items.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.2f, 2f), Array.Empty<object>()));
		_enableRagdoll = ((BaseUnityPlugin)this).Config.Bind<bool>("BotFriends", "EnablePhysicsRagdoll", true, "Enable temporary physics ragdoll on heavy impact.");
		_ragdollImpactThreshold = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "RagdollImpactThreshold", 1f, "Minimum impact speed to trigger ragdoll.");
		_ragdollRecoverTime = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "RagdollRecoverTime", 4f, "Seconds before recovery check.");
		_enablePersonalities = ((BaseUnityPlugin)this).Config.Bind<bool>("BotFriends", "EnablePersonalities", true, "Randomize bot personalities.");
		_personalitySeed = ((BaseUnityPlugin)this).Config.Bind<int>("BotFriends", "PersonalitySeed", 0, "0=auto random, otherwise deterministic personalities.");
		_enableWeapons = ((BaseUnityPlugin)this).Config.Bind<bool>("BotFriends", "EnableWeapons", true, "Allow bots to pick up and use weapons.");
		_weaponSearchRadius = ((BaseUnityPlugin)this).Config.Bind<float>("BotFriends", "WeaponSearchRadius", 20f, new ConfigDescription("Scan radius for weapons.", (AcceptableValueBase)(object)new AcceptableValueRange<float>(2f, 40f), Array.Empty<object>()));
		Patch();
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		((MonoBehaviour)this).StartCoroutine(LevelWatchdog());
		Logger.LogInfo((object)String.Format("{0} v{1} loaded.", (object)((BaseUnityPlugin)this).Info.Metadata.GUID, (object)((BaseUnityPlugin)this).Info.Metadata.Version));
	}

	internal void Patch()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0025: Expected O, but got Unknown
		if (EmpressHarmony == null)
		{
			Harmony val = new Harmony(((BaseUnityPlugin)this).Info.Metadata.GUID);
			Harmony val2 = val;
			EmpressHarmony = val;
		}
		EmpressHarmony.PatchAll();
	}

	private void OnActiveSceneChanged(Scene a, Scene b)
	{
		_spawnedThisLevel = false;
		CleanupBots();
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator LevelWatchdog()
	{
		WaitForSeconds wait = new WaitForSeconds(0.25f);
		while (true)
		{
			try
			{
				Transform player;
				if (IsMultiplayerSafe())
				{
					if (_bots.Count > 0)
					{
						CleanupBots();
					}
					_spawnedThisLevel = false;
				}
				else if (TryGetAnyPlayer(out player) && IsLevelGenerated() && !_spawnedThisLevel)
				{
					SpawnBots(player);
					_spawnedThisLevel = true;
				}
			}
			catch (Exception val)
			{
				Exception val2 = val;
				Logger.LogError((object)String.Format("Watchdog tick failed: {0}", (object)val2));
			}
			yield return wait;
		}
	}

	private bool TryGetAnyPlayer(out Transform player)
	{
		player = null;
		try
		{
			if (Object.op_Implicit((Object)(object)PlayerController.instance))
			{
				player = ((Component)PlayerController.instance).transform;
				return true;
			}
		}
		catch (Object)
		{
		}
		try
		{
			PlayerAvatar[] array = Object.FindObjectsOfType<PlayerAvatar>();
			if (array != null && array.Length != 0)
			{
				player = ((Component)array[0]).transform;
				return true;
			}
		}
		catch (Object)
		{
		}
		return false;
	}

	private static bool TryGetAnyPlayerAvatar(out PlayerAvatar? playerAvatar)
	{
		playerAvatar = null;
		try
		{
			if (Object.op_Implicit((Object)(object)PlayerAvatar.instance))
			{
				playerAvatar = PlayerAvatar.instance;
				return true;
			}
		}
		catch (Object)
		{
		}
		try
		{
			PlayerAvatar[] array = Object.FindObjectsOfType<PlayerAvatar>();
			if (array != null && array.Length != 0)
			{
				playerAvatar = array[0];
				return true;
			}
		}
		catch (Object)
		{
		}
		return false;
	}

	private static bool IsMultiplayerSafe()
	{
		try
		{
			return GameManager.Multiplayer();
		}
		catch (Object)
		{
		}
		try
		{
			return SemiFunc.IsMultiplayer();
		}
		catch (Object)
		{
		}
		return false;
	}

	private static bool IsLevelGenerated()
	{
		try
		{
			return (Object)(object)LevelGenerator.Instance != (Object)null && LevelGenerator.Instance.Generated;
		}
		catch (Object)
		{
			return false;
		}
	}

	private void SpawnBots(Transform player)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		<>c__DisplayClass51_0 CS$<>8__locals0 = new <>c__DisplayClass51_0();
		CS$<>8__locals0.player = player;
		CleanupBots();
		int num = Mathf.Clamp(_botCount.Value, 0, 5);
		if (num <= 0)
		{
			return;
		}
		List<Pose> val = FindOrderedSpawnPoints(CS$<>8__locals0.player.position);
		if (val.Count == 0)
		{
			val = Enumerable.ToList<Pose>(Enumerable.Select<int, Pose>(Enumerable.Range(0, num), (Func<int, Pose>)delegate(int i)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = CS$<>8__locals0.player.position;
				Vector3 val4 = Random.onUnitSphere.WithY(0f);
				return new Pose(position + ((Vector3)(ref val4)).normalized * (2f + (float)i * 1.25f), Quaternion.identity);
			}));
		}
		int num2 = ((val.Count > 0) ? 1 : 0);
		int num3 = 0;
		for (int j = 0; j < val.Count; j++)
		{
			if (num3 >= num)
			{
				break;
			}
			int num4 = (num2 + j) % val.Count;
			Pose val2 = val[num4];
			GameObject val3 = CreateBot(val2.position, val2.rotation, num3);
			if ((Object)(object)val3 != (Object)null)
			{
				_bots.Add(val3);
				num3++;
			}
		}
		Logger.LogInfo((object)String.Format("Spawned {0} bot(s).", (object)_bots.Count));
	}

	private List<Pose> FindOrderedSpawnPoints(Vector3 reference)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		<>c__DisplayClass52_0 CS$<>8__locals0 = new <>c__DisplayClass52_0();
		CS$<>8__locals0.reference = reference;
		List<Pose> result = new List<Pose>();
		try
		{
			result = Enumerable.ToList<Pose>(Enumerable.Select<SpawnPoint, Pose>((IEnumerable<SpawnPoint>)(object)Enumerable.OrderBy<SpawnPoint, float>(Enumerable.Where<SpawnPoint>((IEnumerable<SpawnPoint>)(object)Object.FindObjectsOfType<SpawnPoint>(), (Func<SpawnPoint, bool>)((SpawnPoint p) => ((Component)p).gameObject.activeInHierarchy)), (Func<SpawnPoint, float>)((SpawnPoint p) => Vector3.SqrMagnitude(((Component)p).transform.position - CS$<>8__locals0.reference))), (Func<SpawnPoint, Pose>)((SpawnPoint p) => new Pose(((Component)p).transform.position, ((Component)p).transform.rotation))));
		}
		catch (Object)
		{
		}
		return result;
	}

	private static bool TryPlaceOnNavMesh(NavMeshAgent agent, Vector3 desired)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)agent))
		{
			return false;
		}
		Vector3 val = desired;
		NavMeshHit val2 = default(NavMeshHit);
		if (NavMesh.SamplePosition(desired, ref val2, 6f, -1))
		{
			val = ((NavMeshHit)(ref val2)).position;
		}
		if (agent.Warp(val))
		{
			return agent.isOnNavMesh;
		}
		return false;
	}

	private GameObject? CreateBot(Vector3 pos, Quaternion rot, int index)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Expected O, but got Unknown
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Expected O, but got Unknown
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Expected O, but got Unknown
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = NextName();
			Color val = Color.HSVToRGB(Mathf.Repeat((float)index * 0.381966f + Random.Range(0.02f, 0.14f), 1f), Random.Range(0.8f, 0.98f), 1f);
			GameObject val2 = new GameObject(text);
			val2.layer = LayerMask.NameToLayer("Default");
			val2.transform.SetPositionAndRotation(pos, rot);
			CapsuleCollider obj = val2.AddComponent<CapsuleCollider>();
			obj.height = 1.8f;
			obj.radius = 0.35f;
			obj.center = new Vector3(0f, 0.9f, 0f);
			GameObject val3 = new GameObject("NameTag");
			val3.transform.SetParent(val2.transform, false);
			val3.transform.localPosition = new Vector3(0f, 2.15f, 0f);
			TextMesh obj2 = val3.AddComponent<TextMesh>();
			obj2.text = text;
			obj2.fontSize = 48;
			obj2.characterSize = 0.06f;
			obj2.alignment = (TextAlignment)1;
			obj2.anchor = (TextAnchor)7;
			NavMeshAgent val4 = val2.AddComponent<NavMeshAgent>();
			val4.speed = Mathf.Clamp(_botSpeed.Value, 0.5f, 6f);
			obj2.color = val;
			val4.angularSpeed = 720f;
			val4.acceleration = 20f;
			val4.radius = 0.3f;
			val4.height = 1.8f;
			val4.obstacleAvoidanceType = (ObstacleAvoidanceType)4;
			val4.autoTraverseOffMeshLink = true;
			TryPlaceOnNavMesh(val4, pos);
			val4.stoppingDistance = 1.5f;
			bool flag = false;
			if (TryGetAnyPlayerAvatar(out PlayerAvatar playerAvatar))
			{
				flag = EmpressBotAvatarVisual.TryAttach(val2, playerAvatar, val, val4, val4.speed);
			}
			if (!flag)
			{
				GameObject val5 = GameObject.CreatePrimitive((PrimitiveType)1);
				((Object)val5).name = "Body";
				val5.transform.SetParent(val2.transform, false);
				val5.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
				val5.transform.localPosition = new Vector3(0f, 0.9f, 0f);
				Collider component = val5.GetComponent<Collider>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.enabled = false;
				}
				GameObject obj3 = GameObject.CreatePrimitive((PrimitiveType)0);
				((Object)obj3).name = "Head";
				obj3.transform.SetParent(val2.transform, false);
				obj3.transform.localScale = Vector3.one * 0.35f;
				obj3.transform.localPosition = new Vector3(0f, 1.6f, 0f);
				MeshRenderer component2 = val5.GetComponent<MeshRenderer>();
				MeshRenderer component3 = obj3.GetComponent<MeshRenderer>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					Material val6 = new Material(((Renderer)component2).sharedMaterial);
					val6.color = val;
					((Renderer)component2).material = val6;
				}
				if (Object.op_Implicit((Object)(object)component3))
				{
					Material val7 = new Material(((Renderer)component3).sharedMaterial);
					val7.color = Color.white;
					((Renderer)component3).material = val7;
				}
			}
			StupidBotBrain stupidBotBrain = val2.AddComponent<StupidBotBrain>();
			BotPersonality botPersonality = null;
			if (_enablePersonalities.Value)
			{
				botPersonality = val2.AddComponent<BotPersonality>();
				int seed = ((_personalitySeed.Value != 0) ? (_personalitySeed.Value + index) : Random.Range(1, 2147483647));
				botPersonality.InitRandom(seed);
				float speed = (val4.speed = Mathf.Clamp(_botSpeed.Value * botPersonality.SpeedMultiplier, 0.5f, 6f));
				stupidBotBrain.Init(new StupidBotBrain.Config
				{
					FollowPlayer = _botsFollowPlayer.Value,
					WanderRadius = 12f,
					Speed = speed
				});
			}
			stupidBotBrain.Init(new StupidBotBrain.Config
			{
				FollowPlayer = _botsFollowPlayer.Value,
				WanderRadius = 12f,
				Speed = Mathf.Clamp(_botSpeed.Value, 0.5f, 6f)
			});
			val2.AddComponent<BotChatterAgent>();
			Transform transform = new GameObject("HoldPoint").transform;
			transform.SetParent(val2.transform, false);
			transform.localPosition = new Vector3(0f, Mathf.Clamp(_holdOffsetY.Value, 0.2f, 2f), 0.6f);
			transform.localRotation = Quaternion.identity;
			StupidBotCarrier stupidBotCarrier = null;
			if (_botsFetchValuables.Value)
			{
				stupidBotCarrier = val2.AddComponent<StupidBotCarrier>();
				stupidBotCarrier.Init(new StupidBotCarrier.Cfg
				{
					SearchRadius = Mathf.Clamp(_pickupSearchRadius.Value, 2f, 40f),
					ExtractorStopDistance = Mathf.Clamp(_extractorStopDistance.Value, 0.5f, 4f),
					GentleDropTime = 0.25f
				}, val4, transform, stupidBotBrain);
				if (Object.op_Implicit((Object)(object)botPersonality) && !botPersonality.AllowValuables)
				{
					((Behaviour)stupidBotCarrier).enabled = false;
				}
			}
			if (_enableWeapons.Value)
			{
				val2.AddComponent<BotWeaponUser>().Init(new BotWeaponUser.Cfg
				{
					SearchRadius = Mathf.Clamp(_weaponSearchRadius.Value, 2f, 40f)
				}, val4, transform, stupidBotBrain, botPersonality);
			}
			Rigidbody val8 = val2.GetComponent<Rigidbody>() ?? val2.AddComponent<Rigidbody>();
			val8.mass = 60f;
			val8.useGravity = true;
			val8.isKinematic = true;
			val8.collisionDetectionMode = (CollisionDetectionMode)2;
			val8.interpolation = (RigidbodyInterpolation)1;
			if (_enableRagdoll.Value)
			{
				val2.AddComponent<BotRagdoll>().Init(val4, stupidBotBrain, stupidBotCarrier, val8, _enableRagdoll.Value, _ragdollImpactThreshold.Value, _ragdollRecoverTime.Value);
			}
			return val2;
		}
		catch (Exception val9)
		{
			Exception val10 = val9;
			Logger.LogError((object)String.Format("Failed to create bot: {0}", (object)val10));
			return null;
		}
	}

	private static void EnsureNameBag()
	{
		if (_nameBag == null || _nameBag.Count <= 0)
		{
			List<string> val = new List<string>((IEnumerable<string>)(object)NamePool);
			for (int i = 0; i < val.Count; i++)
			{
				int num = Random.Range(i, val.Count);
				string text = val[i];
				val[i] = val[num];
				val[num] = text;
			}
			_nameBag = new Queue<string>((IEnumerable<string>)(object)val);
		}
	}

	private static string NextName()
	{
		EnsureNameBag();
		if (_nameBag.Count <= 0)
		{
			return String.Format("Unit-{0}", (object)Random.Range(100, 999));
		}
		return _nameBag.Dequeue();
	}

	private void CleanupBots()
	{
		for (int num = _bots.Count - 1; num >= 0; num--)
		{
			GameObject val = _bots[num];
			if (Object.op_Implicit((Object)(object)val))
			{
				val.SetActive(false);
			}
		}
		_bots.Clear();
	}
}
