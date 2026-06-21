using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Empress.StupidBots;

internal sealed class EmpressBotAvatarVisual : MonoBehaviour
{
	private const float DefaultSpeed = 4.2f;

	private const int MaxRandomCosmetics = 4;

	private static readonly string[] HiddenVisualNameParts = (string[])(object)new String[7] { "grab", "grabb", "holder", "suction", "vacuum", "tool", "cart" };

	private static readonly string[] PreferredVisibleLayers = (string[])(object)new String[2] { "PlayerVisuals", "Default" };

	private static readonly CosmeticType[] RandomCosmeticTypes;

	private static readonly FieldRef<PlayerAvatar, PlayerCosmetics> PlayerAvatarCosmeticsRef;

	private static readonly FieldRef<PlayerAvatarVisuals, PlayerCosmetics> VisualsPlayerCosmeticsRef;

	private static readonly FieldRef<PlayerCosmetics, PlayerAvatarVisuals> PlayerCosmeticsVisualsRef;

	private static readonly FieldRef<PlayerCosmetics, List<CosmeticParent>> CosmeticParentsRef;

	private static readonly FieldRef<CosmeticParent, CosmeticType> CosmeticParentTypeRef;

	private static readonly FieldRef<CosmeticParent, PlayerSpringImpulse> CosmeticParentSpringImpulseRef;

	private static readonly FieldRef<CosmeticParent, Transform> CosmeticParentParentRef;

	private static readonly FieldRef<CosmeticParent, bool> CosmeticParentResetTransformRef;

	private static readonly FieldRef<CosmeticParent, List<Transform>> BaseMeshParentsRef;

	private static readonly FieldRef<CosmeticParent, List<Transform>> BaseMeshesRef;

	private static readonly FieldRef<PlayerCosmetics, bool> FirstSetupRef;

	private static readonly FieldRef<PlayerCosmetics, bool> FirstSetupCoroutineRef;

	private static readonly FieldRef<MetaManager, List<CosmeticAsset>> CosmeticAssetsRef;

	private static readonly FieldRef<CosmeticAsset, CosmeticType> CosmeticAssetTypeRef;

	private static readonly FieldRef<CosmeticAsset, PrefabRef> CosmeticAssetPrefabRef;

	private static readonly MethodInfo InstantiateCosmeticMethod;

	private NavMeshAgent? _agent;

	private Animator[] _animators = Array.Empty<Animator>();

	private Vector3 _lastPosition;

	private float _fallbackSpeed = 4.2f;

	internal void Init(NavMeshAgent? agent, float fallbackSpeed)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_agent = agent;
		_fallbackSpeed = Mathf.Max(0.1f, fallbackSpeed);
		_animators = ((Component)this).GetComponentsInChildren<Animator>(true);
		_lastPosition = ((Component)this).transform.position;
		for (int i = 0; i < _animators.Length; i++)
		{
			Animator val = _animators[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				((Behaviour)val).enabled = true;
				val.applyRootMotion = false;
				val.speed = 1f;
				val.cullingMode = (AnimatorCullingMode)0;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Invalid comparison between Unknown and I4
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Invalid comparison between Unknown and I4
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		if (_animators == null || _animators.Length == 0)
		{
			return;
		}
		Vector3 val = ((Time.deltaTime > 0.0001f) ? ((((Component)this).transform.position - _lastPosition) / Time.deltaTime) : Vector3.zero);
		_lastPosition = ((Component)this).transform.position;
		if (Object.op_Implicit((Object)(object)_agent) && ((Behaviour)_agent).enabled && _agent.isOnNavMesh)
		{
			Vector3 velocity = _agent.velocity;
			if (((Vector3)(ref velocity)).sqrMagnitude > ((Vector3)(ref val)).sqrMagnitude)
			{
				val = _agent.velocity;
			}
		}
		Vector3 val2 = val;
		val2.y = 0f;
		float magnitude = ((Vector3)(ref val2)).magnitude;
		bool flag = magnitude > 0.08f;
		float num = (Object.op_Implicit((Object)(object)_agent) ? Mathf.Max(1.5f, _agent.speed) : Mathf.Max(1.5f, _fallbackSpeed));
		float num2 = Mathf.Clamp01(magnitude / num);
		Vector3 val3 = ((Component)this).transform.InverseTransformDirection(val2);
		for (int i = 0; i < _animators.Length; i++)
		{
			Animator val4 = _animators[i];
			if (!Object.op_Implicit((Object)(object)val4))
			{
				continue;
			}
			AnimatorControllerParameter[] parameters = val4.parameters;
			foreach (AnimatorControllerParameter val5 in parameters)
			{
				string text = val5.name.ToLowerInvariant();
				AnimatorControllerParameterType type = val5.type;
				if ((int)type != 1)
				{
					if ((int)type == 4)
					{
						if (text.Contains("moving") || text.Contains("move") || text.Contains("walk") || text.Contains("run"))
						{
							val4.SetBool(val5.nameHash, flag);
						}
						else if (text.Contains("ground"))
						{
							val4.SetBool(val5.nameHash, true);
						}
					}
				}
				else if (text.Contains("horizontal") || text.Contains("strafe"))
				{
					val4.SetFloat(val5.nameHash, val3.x);
				}
				else if (text.Contains("vertical") || text.Contains("forward"))
				{
					val4.SetFloat(val5.nameHash, val3.z);
				}
				else if (text.Contains("speed") || text.Contains("move") || text.Contains("velocity"))
				{
					val4.SetFloat(val5.nameHash, num2);
				}
			}
		}
	}

	internal static bool TryAttach(GameObject root, PlayerAvatar? player, Color accent, NavMeshAgent? agent, float fallbackSpeed)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)root) || !Object.op_Implicit((Object)(object)player))
		{
			return false;
		}
		GameObject val = ResolveAvatarVisualRoot(player);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return false;
		}
		PlayerCosmetics sourceCosmetics = ResolvePlayerCosmetics(player, val);
		GameObject val2;
		try
		{
			val2 = Object.Instantiate<GameObject>(val, root.transform, false);
		}
		catch (Object)
		{
			return false;
		}
		((Object)val2).name = "AvatarBody";
		val2.transform.localPosition = Vector3.zero;
		val2.transform.localRotation = Quaternion.identity;
		val2.transform.localScale = Vector3.one;
		PrepareClone(val2, val, sourceCosmetics);
		TintClone(val2, accent);
		val2.AddComponent<EmpressBotAvatarVisual>().Init(agent, fallbackSpeed);
		return CountVisibleRenderers(val2) > 0;
	}

	private static GameObject? ResolveAvatarVisualRoot(PlayerAvatar player)
	{
		if (!Object.op_Implicit((Object)(object)player))
		{
			return null;
		}
		try
		{
			if (Object.op_Implicit((Object)(object)player.playerAvatarVisuals))
			{
				return ((Component)player.playerAvatarVisuals).gameObject;
			}
		}
		catch (Object)
		{
		}
		try
		{
			FieldInfo val2 = AccessTools.Field(((Object)player).GetType(), "playerAvatarVisuals");
			if (val2 != (FieldInfo)null)
			{
				object value = val2.GetValue((object)player);
				Component val3 = (Component)((value is Component) ? value : null);
				if (Object.op_Implicit((Object)(object)val3))
				{
					return val3.gameObject;
				}
			}
		}
		catch (Object)
		{
		}
		try
		{
			PropertyInfo val5 = AccessTools.Property(((Object)player).GetType(), "playerAvatarVisuals");
			if (val5 != (PropertyInfo)null)
			{
				object value2 = val5.GetValue((object)player, (object[])null);
				Component val6 = (Component)((value2 is Component) ? value2 : null);
				if (Object.op_Implicit((Object)(object)val6))
				{
					return val6.gameObject;
				}
			}
		}
		catch (Object)
		{
		}
		try
		{
			PlayerAvatarVisuals componentInChildren = ((Component)player).GetComponentInChildren<PlayerAvatarVisuals>(true);
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				return ((Component)componentInChildren).gameObject;
			}
		}
		catch (Object)
		{
		}
		return null;
	}

	private static PlayerCosmetics? ResolvePlayerCosmetics(PlayerAvatar player, GameObject visualRoot)
	{
		if (!Object.op_Implicit((Object)(object)player))
		{
			return null;
		}
		try
		{
			PlayerCosmetics val = PlayerAvatarCosmeticsRef.Invoke(player);
			if (Object.op_Implicit((Object)(object)val))
			{
				return val;
			}
		}
		catch (Object)
		{
		}
		try
		{
			PlayerAvatarVisuals component = visualRoot.GetComponent<PlayerAvatarVisuals>();
			if (Object.op_Implicit((Object)(object)component))
			{
				PlayerCosmetics val3 = VisualsPlayerCosmeticsRef.Invoke(component);
				if (Object.op_Implicit((Object)(object)val3))
				{
					return val3;
				}
			}
		}
		catch (Object)
		{
		}
		try
		{
			PlayerCosmetics componentInChildren = ((Component)player).GetComponentInChildren<PlayerCosmetics>(true);
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				return componentInChildren;
			}
		}
		catch (Object)
		{
		}
		return null;
	}

	private static void PrepareClone(GameObject clone, GameObject sourceRoot, PlayerCosmetics? sourceCosmetics)
	{
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		MonoBehaviour[] componentsInChildren = clone.GetComponentsInChildren<MonoBehaviour>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren[i]))
			{
				componentsInChildren[i].StopAllCoroutines();
				((Behaviour)componentsInChildren[i]).enabled = false;
			}
		}
		clone.SetActive(true);
		int layer = ResolveVisibleLayer();
		EnsureCloneCosmetics(clone, sourceRoot, sourceCosmetics);
		PreventVanillaCosmeticSetup(clone);
		StripEquippedCosmetics(clone);
		RestoreBaseAvatarMeshes(clone);
		RandomizeCloneCosmetics(clone);
		Transform[] componentsInChildren2 = clone.GetComponentsInChildren<Transform>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren2[j]))
			{
				((Component)componentsInChildren2[j]).gameObject.layer = layer;
			}
		}
		HideNamedVisuals(clone);
		DisableCloneLights(clone);
		Renderer[] componentsInChildren3 = clone.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren3)
		{
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			if (ShouldHide(((Object)((Component)val).gameObject).name))
			{
				val.enabled = false;
				continue;
			}
			val.enabled = true;
			((Component)val).gameObject.layer = layer;
			val.shadowCastingMode = (ShadowCastingMode)1;
			val.receiveShadows = true;
			SkinnedMeshRenderer val2 = (SkinnedMeshRenderer)(object)((val is SkinnedMeshRenderer) ? val : null);
			if (val2 != null)
			{
				val2.updateWhenOffscreen = true;
			}
		}
		Animator[] componentsInChildren4 = clone.GetComponentsInChildren<Animator>(true);
		foreach (Animator val3 in componentsInChildren4)
		{
			if (Object.op_Implicit((Object)(object)val3))
			{
				((Behaviour)val3).enabled = true;
				val3.applyRootMotion = false;
				val3.speed = 1f;
				val3.cullingMode = (AnimatorCullingMode)0;
			}
		}
		Collider[] componentsInChildren5 = clone.GetComponentsInChildren<Collider>(true);
		for (int m = 0; m < componentsInChildren5.Length; m++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren5[m]))
			{
				componentsInChildren5[m].enabled = false;
			}
		}
		Rigidbody[] componentsInChildren6 = clone.GetComponentsInChildren<Rigidbody>(true);
		foreach (Rigidbody val4 in componentsInChildren6)
		{
			if (Object.op_Implicit((Object)(object)val4))
			{
				val4.isKinematic = true;
				val4.detectCollisions = false;
				val4.useGravity = false;
			}
		}
		AudioSource[] componentsInChildren7 = clone.GetComponentsInChildren<AudioSource>(true);
		for (int num = 0; num < componentsInChildren7.Length; num++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren7[num]))
			{
				((Behaviour)componentsInChildren7[num]).enabled = false;
			}
		}
		ParticleSystem[] componentsInChildren8 = clone.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem val5 in componentsInChildren8)
		{
			if (Object.op_Implicit((Object)(object)val5))
			{
				EmissionModule emission = val5.emission;
				((EmissionModule)(ref emission)).enabled = false;
				val5.Stop(true, (ParticleSystemStopBehavior)0);
			}
		}
		TextMesh[] componentsInChildren9 = clone.GetComponentsInChildren<TextMesh>(true);
		for (int num3 = 0; num3 < componentsInChildren9.Length; num3++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren9[num3]))
			{
				((Component)componentsInChildren9[num3]).gameObject.SetActive(false);
			}
		}
	}

	private static void RandomizeCloneCosmetics(GameObject clone)
	{
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)MetaManager.instance) || InstantiateCosmeticMethod == (MethodInfo)null)
		{
			return;
		}
		PlayerCosmetics val = Enumerable.FirstOrDefault<PlayerCosmetics>((IEnumerable<PlayerCosmetics>)(object)clone.GetComponentsInChildren<PlayerCosmetics>(true), (Func<PlayerCosmetics, bool>)((PlayerCosmetics component) => Object.op_Implicit((Object)(object)component)));
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		HashSet<CosmeticType> supportedCosmeticTypes = GetSupportedCosmeticTypes(val);
		if (supportedCosmeticTypes.Count == 0)
		{
			return;
		}
		List<CosmeticAsset> val2 = null;
		try
		{
			val2 = CosmeticAssetsRef.Invoke(MetaManager.instance);
		}
		catch (Object)
		{
		}
		if (val2 == null || val2.Count == 0)
		{
			return;
		}
		Dictionary<CosmeticType, List<CosmeticAsset>> val4 = new Dictionary<CosmeticType, List<CosmeticAsset>>();
		List<CosmeticAsset> val10 = default(List<CosmeticAsset>);
		for (int i = 0; i < val2.Count; i++)
		{
			CosmeticAsset val5 = val2[i];
			if (!Object.op_Implicit((Object)(object)val5))
			{
				continue;
			}
			CosmeticType val6;
			try
			{
				val6 = CosmeticAssetTypeRef.Invoke(val5);
			}
			catch (Object)
			{
				continue;
			}
			PrefabRef val8;
			try
			{
				val8 = CosmeticAssetPrefabRef.Invoke(val5);
			}
			catch (Object)
			{
				continue;
			}
			if (supportedCosmeticTypes.Contains(val6) && Enumerable.Contains<CosmeticType>((IEnumerable<CosmeticType>)(object)RandomCosmeticTypes, val6) && val8 != null && val8.IsValid())
			{
				if (!val4.TryGetValue(val6, ref val10))
				{
					val10 = (val4[val6] = new List<CosmeticAsset>());
				}
				val10.Add(val5);
			}
		}
		List<CosmeticType> val12 = Enumerable.ToList<CosmeticType>((IEnumerable<CosmeticType>)(object)Enumerable.OrderBy<CosmeticType, float>((IEnumerable<CosmeticType>)(object)val4.Keys, (Func<CosmeticType, float>)((CosmeticType _) => Random.value)));
		int num = Mathf.Min(4, val12.Count);
		for (int j = 0; j < num; j++)
		{
			List<CosmeticAsset> val13 = val4[val12[j]];
			if (val13.Count == 0)
			{
				continue;
			}
			CosmeticAsset val14 = val13[Random.Range(0, val13.Count)];
			try
			{
				object obj = ((MethodBase)InstantiateCosmeticMethod).Invoke((object)val, (object[])(object)new Object[1] { (Object)val14 });
				GameObject val15 = (GameObject)((obj is GameObject) ? obj : null);
				if (Object.op_Implicit((Object)(object)val15))
				{
					val15.SetActive(true);
				}
			}
			catch (Object)
			{
			}
		}
	}

	private static void EnsureCloneCosmetics(GameObject clone, GameObject sourceRoot, PlayerCosmetics? sourceCosmetics)
	{
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)sourceCosmetics))
		{
			return;
		}
		PlayerCosmetics val = clone.GetComponentInChildren<PlayerCosmetics>(true);
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = clone.AddComponent<PlayerCosmetics>();
		}
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		try
		{
			((MonoBehaviour)val).StopAllCoroutines();
		}
		catch (Object)
		{
		}
		try
		{
			FirstSetupRef.Invoke(val) = false;
		}
		catch (Object)
		{
		}
		try
		{
			FirstSetupCoroutineRef.Invoke(val) = true;
		}
		catch (Object)
		{
		}
		PlayerAvatarVisuals component = clone.GetComponent<PlayerAvatarVisuals>();
		if (Object.op_Implicit((Object)(object)component))
		{
			try
			{
				PlayerCosmeticsVisualsRef.Invoke(val) = component;
			}
			catch (Object)
			{
			}
			try
			{
				VisualsPlayerCosmeticsRef.Invoke(component) = val;
			}
			catch (Object)
			{
			}
		}
		List<CosmeticParent> val7 = null;
		try
		{
			val7 = CosmeticParentsRef.Invoke(sourceCosmetics);
		}
		catch (Object)
		{
		}
		if (val7 == null || val7.Count == 0)
		{
			return;
		}
		List<CosmeticParent> val9 = new List<CosmeticParent>();
		for (int i = 0; i < val7.Count; i++)
		{
			CosmeticParent val10 = val7[i];
			if (val10 == null)
			{
				continue;
			}
			Transform val11 = null;
			List<Transform> val12 = new List<Transform>();
			List<Transform> val13 = new List<Transform>();
			try
			{
				val11 = MapCloneTransform(CosmeticParentParentRef.Invoke(val10), sourceRoot.transform, clone.transform);
			}
			catch (Object)
			{
			}
			try
			{
				val12 = MapCloneTransforms(BaseMeshParentsRef.Invoke(val10), sourceRoot.transform, clone.transform);
			}
			catch (Object)
			{
			}
			try
			{
				val13 = MapCloneTransforms(BaseMeshesRef.Invoke(val10), sourceRoot.transform, clone.transform);
			}
			catch (Object)
			{
			}
			if (Object.op_Implicit((Object)(object)val11) && val12.Count != 0)
			{
				CosmeticParent val17 = new CosmeticParent();
				try
				{
					CosmeticParentTypeRef.Invoke(val17) = CosmeticParentTypeRef.Invoke(val10);
				}
				catch (Object)
				{
					continue;
				}
				try
				{
					CosmeticParentSpringImpulseRef.Invoke(val17) = null;
				}
				catch (Object)
				{
				}
				try
				{
					CosmeticParentParentRef.Invoke(val17) = val11;
				}
				catch (Object)
				{
				}
				try
				{
					CosmeticParentResetTransformRef.Invoke(val17) = CosmeticParentResetTransformRef.Invoke(val10);
				}
				catch (Object)
				{
				}
				try
				{
					BaseMeshParentsRef.Invoke(val17) = val12;
				}
				catch (Object)
				{
				}
				try
				{
					BaseMeshesRef.Invoke(val17) = val13;
				}
				catch (Object)
				{
				}
				val9.Add(val17);
			}
		}
		if (val9.Count <= 0)
		{
			return;
		}
		try
		{
			CosmeticParentsRef.Invoke(val) = val9;
		}
		catch (Object)
		{
		}
	}

	private static HashSet<CosmeticType> GetSupportedCosmeticTypes(PlayerCosmetics playerCosmetics)
	{
		HashSet<CosmeticType> val = new HashSet<CosmeticType>();
		List<CosmeticParent> val2 = null;
		try
		{
			val2 = CosmeticParentsRef.Invoke(playerCosmetics);
		}
		catch (Object)
		{
		}
		if (val2 == null)
		{
			return val;
		}
		for (int i = 0; i < val2.Count; i++)
		{
			CosmeticParent val4 = val2[i];
			if (val4 == null)
			{
				continue;
			}
			Transform val5 = null;
			try
			{
				val5 = CosmeticParentParentRef.Invoke(val4);
			}
			catch (Object)
			{
			}
			if (Object.op_Implicit((Object)(object)val5))
			{
				try
				{
					val.Add(CosmeticParentTypeRef.Invoke(val4));
				}
				catch (Object)
				{
				}
			}
		}
		return val;
	}

	private static List<Transform> MapCloneTransforms(List<Transform>? sourceTransforms, Transform sourceRoot, Transform cloneRoot)
	{
		List<Transform> val = new List<Transform>();
		if (sourceTransforms == null)
		{
			return val;
		}
		for (int i = 0; i < sourceTransforms.Count; i++)
		{
			Transform val2 = MapCloneTransform(sourceTransforms[i], sourceRoot, cloneRoot);
			if (Object.op_Implicit((Object)(object)val2))
			{
				val.Add(val2);
			}
		}
		return val;
	}

	private static Transform? MapCloneTransform(Transform? source, Transform sourceRoot, Transform cloneRoot)
	{
		if (!Object.op_Implicit((Object)(object)source))
		{
			return null;
		}
		if ((Object)(object)source == (Object)(object)sourceRoot)
		{
			return cloneRoot;
		}
		if (!source.IsChildOf(sourceRoot))
		{
			return null;
		}
		List<int> val = new List<int>();
		Transform val2 = source;
		while (Object.op_Implicit((Object)(object)val2) && (Object)(object)val2 != (Object)(object)sourceRoot)
		{
			val.Add(val2.GetSiblingIndex());
			val2 = val2.parent;
		}
		Transform val3 = cloneRoot;
		for (int num = val.Count - 1; num >= 0; num--)
		{
			int num2 = val[num];
			if (num2 < 0 || num2 >= val3.childCount)
			{
				return null;
			}
			val3 = val3.GetChild(num2);
		}
		return val3;
	}

	private static void PreventVanillaCosmeticSetup(GameObject clone)
	{
		PlayerCosmetics[] componentsInChildren = clone.GetComponentsInChildren<PlayerCosmetics>(true);
		foreach (PlayerCosmetics val in componentsInChildren)
		{
			if (Object.op_Implicit((Object)(object)val))
			{
				try
				{
					((MonoBehaviour)val).StopAllCoroutines();
				}
				catch (Object)
				{
				}
				try
				{
					FirstSetupRef.Invoke(val) = false;
				}
				catch (Object)
				{
				}
				try
				{
					FirstSetupCoroutineRef.Invoke(val) = true;
				}
				catch (Object)
				{
				}
			}
		}
	}

	private static void StripEquippedCosmetics(GameObject clone)
	{
		Cosmetic[] componentsInChildren = clone.GetComponentsInChildren<Cosmetic>(true);
		foreach (Cosmetic val in componentsInChildren)
		{
			if (Object.op_Implicit((Object)(object)val) && !((Object)(object)((Component)val).gameObject == (Object)(object)clone))
			{
				((Component)val).gameObject.SetActive(false);
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
	}

	private static void RestoreBaseAvatarMeshes(GameObject clone)
	{
		PlayerCosmetics[] componentsInChildren = clone.GetComponentsInChildren<PlayerCosmetics>(true);
		foreach (PlayerCosmetics val in componentsInChildren)
		{
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			List<CosmeticParent> val2 = null;
			try
			{
				val2 = CosmeticParentsRef.Invoke(val);
			}
			catch (Object)
			{
			}
			if (val2 == null)
			{
				continue;
			}
			for (int j = 0; j < val2.Count; j++)
			{
				CosmeticParent val4 = val2[j];
				if (val4 != null)
				{
					List<Transform> transforms = null;
					List<Transform> transforms2 = null;
					try
					{
						transforms = BaseMeshParentsRef.Invoke(val4);
					}
					catch (Object)
					{
					}
					try
					{
						transforms2 = BaseMeshesRef.Invoke(val4);
					}
					catch (Object)
					{
					}
					SetBaseMeshesActive(transforms, active: true);
					SetBaseMeshesActive(transforms2, active: true);
				}
			}
		}
	}

	private static void SetBaseMeshesActive(List<Transform>? transforms, bool active)
	{
		if (transforms == null)
		{
			return;
		}
		for (int i = 0; i < transforms.Count; i++)
		{
			Transform val = transforms[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(active);
			}
		}
	}

	private static void DisableCloneLights(GameObject clone)
	{
		Light[] componentsInChildren = clone.GetComponentsInChildren<Light>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren[i]))
			{
				((Behaviour)componentsInChildren[i]).enabled = false;
			}
		}
	}

	private static void HideNamedVisuals(GameObject clone)
	{
		Transform[] componentsInChildren = clone.GetComponentsInChildren<Transform>(true);
		foreach (Transform val in componentsInChildren)
		{
			if (Object.op_Implicit((Object)(object)val) && ShouldHide(((Object)((Component)val).gameObject).name))
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
	}

	private static bool ShouldHide(string name)
	{
		if (String.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		string text = name.ToLowerInvariant();
		for (int i = 0; i < HiddenVisualNameParts.Length; i++)
		{
			if (text.Contains(HiddenVisualNameParts[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static void TintClone(GameObject clone, Color accent)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = clone.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if (!Object.op_Implicit((Object)(object)val) || Object.op_Implicit((Object)(object)((Component)val).GetComponentInParent<Cosmetic>()))
			{
				continue;
			}
			Material[] materials;
			try
			{
				materials = val.materials;
			}
			catch (Object)
			{
				continue;
			}
			foreach (Material val3 in materials)
			{
				if (Object.op_Implicit((Object)(object)val3))
				{
					if (val3.HasProperty("_Color"))
					{
						val3.SetColor("_Color", Color.Lerp(val3.GetColor("_Color"), accent, 0.72f));
					}
					if (val3.HasProperty("_BaseColor"))
					{
						val3.SetColor("_BaseColor", Color.Lerp(val3.GetColor("_BaseColor"), accent, 0.72f));
					}
					if (val3.HasProperty("_EmissionColor"))
					{
						val3.EnableKeyword("_EMISSION");
						val3.SetColor("_EmissionColor", accent * 0.5f);
					}
				}
			}
		}
	}

	private static int CountVisibleRenderers(GameObject clone)
	{
		int num = 0;
		Renderer[] componentsInChildren = clone.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if (Object.op_Implicit((Object)(object)val) && val.enabled && ((Component)val).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		return num;
	}

	private static int ResolveVisibleLayer()
	{
		for (int i = 0; i < PreferredVisibleLayers.Length; i++)
		{
			int num = LayerMask.NameToLayer(PreferredVisibleLayers[i]);
			if (num >= 0)
			{
				return num;
			}
		}
		return 0;
	}

	static EmpressBotAvatarVisual()
	{
		CosmeticType[] array = new CosmeticType[11];
		RuntimeHelpers.InitializeArray((Array)(object)array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		RandomCosmeticTypes = (CosmeticType[])(object)array;
		PlayerAvatarCosmeticsRef = AccessTools.FieldRefAccess<PlayerAvatar, PlayerCosmetics>("playerCosmetics");
		VisualsPlayerCosmeticsRef = AccessTools.FieldRefAccess<PlayerAvatarVisuals, PlayerCosmetics>("playerCosmetics");
		PlayerCosmeticsVisualsRef = AccessTools.FieldRefAccess<PlayerCosmetics, PlayerAvatarVisuals>("playerAvatarVisuals");
		CosmeticParentsRef = AccessTools.FieldRefAccess<PlayerCosmetics, List<CosmeticParent>>("cosmeticParents");
		CosmeticParentTypeRef = AccessTools.FieldRefAccess<CosmeticParent, CosmeticType>("cosmeticType");
		CosmeticParentSpringImpulseRef = AccessTools.FieldRefAccess<CosmeticParent, PlayerSpringImpulse>("springImpulse");
		CosmeticParentParentRef = AccessTools.FieldRefAccess<CosmeticParent, Transform>("parent");
		CosmeticParentResetTransformRef = AccessTools.FieldRefAccess<CosmeticParent, bool>("resetTransform");
		BaseMeshParentsRef = AccessTools.FieldRefAccess<CosmeticParent, List<Transform>>("baseMeshParents");
		BaseMeshesRef = AccessTools.FieldRefAccess<CosmeticParent, List<Transform>>("baseMeshes");
		FirstSetupRef = AccessTools.FieldRefAccess<PlayerCosmetics, bool>("firstSetup");
		FirstSetupCoroutineRef = AccessTools.FieldRefAccess<PlayerCosmetics, bool>("firstSetupCoroutine");
		CosmeticAssetsRef = AccessTools.FieldRefAccess<MetaManager, List<CosmeticAsset>>("cosmeticAssets");
		CosmeticAssetTypeRef = AccessTools.FieldRefAccess<CosmeticAsset, CosmeticType>("type");
		CosmeticAssetPrefabRef = AccessTools.FieldRefAccess<CosmeticAsset, PrefabRef>("prefab");
		InstantiateCosmeticMethod = AccessTools.Method(typeof(PlayerCosmetics), "InstantiateCosmetic", (Type[])(object)new Type[1] { typeof(CosmeticAsset) }, (Type[])null);
	}
}
