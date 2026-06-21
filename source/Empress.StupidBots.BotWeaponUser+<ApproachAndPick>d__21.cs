using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Empress.StupidBots;

internal class BotWeaponUser : MonoBehaviour
{
	internal struct Cfg : ValueType
	{
		public float SearchRadius;
	}

	private static readonly Dictionary<int, BotWeaponUser> Claims = new Dictionary<int, BotWeaponUser>();

	private static readonly Dictionary<int, float> EmptyUntil = new Dictionary<int, float>();

	private Cfg _cfg;

	private NavMeshAgent _agent;

	private Transform _hold;

	private StupidBotBrain _brain;

	private BotPersonality _persona;

	private ItemGun _gun;

	private PhysGrabObject _pgo;

	private Rigidbody _rb;

	private Collider[] _myCols;

	private readonly List<Collider> _gunCols = new List<Collider>();

	private float _retargetTimer;

	private float _fireTimer;

	private float _toyTimer;

	private bool _busy;

	private bool _subscribed;

	internal void Init(Cfg cfg, NavMeshAgent agent, Transform hold, StupidBotBrain brain, BotPersonality persona)
	{
		_cfg = cfg;
		_agent = agent;
		_hold = hold;
		_brain = brain;
		_persona = persona;
		_myCols = ((Component)this).GetComponentsInChildren<Collider>(true);
		_retargetTimer = Random.Range(0.2f, 0.8f);
		_fireTimer = Random.Range(0.4f, 1f);
		_toyTimer = Random.Range(1.5f, 3.5f);
	}

	private void OnDisable()
	{
		Drop(immediate: true);
	}

	private void Update()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if ((Object.op_Implicit((Object)(object)_persona) && !_persona.AllowWeapons) || _busy)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)_gun))
		{
			EnsureHeldTransform();
			_brain.SetExternalControl(on: true);
			AimAndShoot();
			return;
		}
		_retargetTimer -= Time.deltaTime;
		if (_retargetTimer > 0f)
		{
			return;
		}
		_retargetTimer = Random.Range(0.9f, 1.6f);
		ItemGun val = FindNearestGun();
		if (!Object.op_Implicit((Object)(object)val) || !TryClaim(val))
		{
			return;
		}
		_busy = true;
		Vector3 position = ((Component)val).transform.position;
		if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			NavMeshHit val2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(position, ref val2, 2.5f, -1))
			{
				_agent.SetDestination(((NavMeshHit)(ref val2)).position);
			}
			else
			{
				_agent.SetDestination(position);
			}
			_agent.stoppingDistance = 0.35f;
		}
		((MonoBehaviour)this).StartCoroutine(ApproachAndPick(val));
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator ApproachAndPick(ItemGun target)
	{
		float giveUp = 8f;
		_pgo = (Object.op_Implicit((Object)(object)target) ? ((Component)target).GetComponent<PhysGrabObject>() : null);
		while (giveUp > 0f && Object.op_Implicit((Object)(object)target) && Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			giveUp -= Time.deltaTime;
			if (!IsValidTarget(target) || Vector3.Distance(((Component)this).transform.position, ((Component)target).transform.position) <= 1.8f)
			{
				break;
			}
			yield return null;
		}
		if (!Object.op_Implicit((Object)(object)target) || !Object.op_Implicit((Object)(object)_agent) || !_agent.isOnNavMesh || !IsValidTarget(target))
		{
			if (Object.op_Implicit((Object)(object)target))
			{
				Release(target);
			}
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		_gun = target;
		_pgo = ((Component)target).GetComponent<PhysGrabObject>();
		_rb = (Object.op_Implicit((Object)(object)_pgo) ? _pgo.rb : null) ?? ((Component)target).GetComponent<Rigidbody>();
		try
		{
			Transform transform = ((Component)target).transform;
			transform.SetParent(_hold, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			if (Object.op_Implicit((Object)(object)_rb))
			{
				_rb.velocity = Vector3.zero;
				_rb.angularVelocity = Vector3.zero;
				_rb.isKinematic = true;
			}
		}
		catch (Object)
		{
		}
		SetCarryCollisionIgnore(ignore: true);
		HookGunEvents(_gun, on: true);
		_brain.SetExternalControl(on: true);
		_busy = false;
	}

	private void AimAndShoot()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		_toyTimer -= Time.deltaTime;
		_fireTimer -= Time.deltaTime;
		Vector3 aimPos;
		Transform val = FindEnemyConstrained(out aimPos);
		Vector3 val2 = aimPos - _hold.position;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		if (((Vector3)(ref normalized)).sqrMagnitude > 0.0001f)
		{
			_hold.rotation = Quaternion.Slerp(_hold.rotation, Quaternion.LookRotation(normalized, Vector3.up), Time.deltaTime * 10f);
		}
		if (Object.op_Implicit((Object)(object)_persona) && _persona.ToyingChance > 0f && _toyTimer <= 0f)
		{
			_toyTimer = Random.Range(1.5f, 3.5f);
			if (Random.value < _persona.ToyingChance && Object.op_Implicit((Object)(object)_pgo))
			{
				ItemToggle val3 = (Object.op_Implicit((Object)(object)_gun) ? ((Component)_gun).GetComponent<ItemToggle>() : null);
				if (Object.op_Implicit((Object)(object)val3))
				{
					val3.ToggleItem(!val3.toggleState, -1);
				}
			}
		}
		if (_fireTimer <= 0f)
		{
			_fireTimer = (Object.op_Implicit((Object)(object)_persona) ? Random.Range(_persona.FireInterval.x, _persona.FireInterval.y) : Random.Range(0.8f, 1.6f));
			if (Object.op_Implicit((Object)(object)val))
			{
				_gun.Shoot();
			}
			if (Object.op_Implicit((Object)(object)_persona) && _persona.DropChanceOnShoot > 0f && Random.value < _persona.DropChanceOnShoot)
			{
				Drop(immediate: false);
			}
		}
	}

	private void EnsureHeldTransform()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_gun))
		{
			Transform transform = ((Component)_gun).transform;
			if ((Object)(object)transform.parent != (Object)(object)_hold)
			{
				transform.SetParent(_hold, false);
			}
			if (transform.localPosition != Vector3.zero)
			{
				transform.localPosition = Vector3.zero;
			}
			if (transform.localRotation != Quaternion.identity)
			{
				transform.localRotation = Quaternion.identity;
			}
			if (Object.op_Implicit((Object)(object)_rb) && !_rb.isKinematic)
			{
				_rb.velocity = Vector3.zero;
				_rb.angularVelocity = Vector3.zero;
				_rb.isKinematic = true;
			}
		}
	}

	private void Drop(bool immediate)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_gun))
		{
			return;
		}
		try
		{
			HookGunEvents(_gun, on: false);
			SetCarryCollisionIgnore(ignore: false);
			((Component)_gun).transform.SetParent((Transform)null, true);
			if (Object.op_Implicit((Object)(object)_rb))
			{
				_rb.isKinematic = false;
				_rb.velocity = Vector3.zero;
				_rb.angularVelocity = Vector3.zero;
			}
		}
		catch (Object)
		{
		}
		Release(_gun);
		_gun = null;
		_pgo = null;
		_rb = null;
		_brain.SetExternalControl(on: false);
	}

	private bool IsValidTarget(ItemGun g)
	{
		if (!Object.op_Implicit((Object)(object)g))
		{
			return false;
		}
		int instanceID = ((Object)g).GetInstanceID();
		float num = default(float);
		if (EmptyUntil.TryGetValue(instanceID, ref num) && num > Time.time)
		{
			return false;
		}
		PhysGrabObject component = ((Component)g).GetComponent<PhysGrabObject>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return false;
		}
		if (SemiFunc.PhysGrabObjectIsGrabbed(component))
		{
			return false;
		}
		return true;
	}

	private ItemGun FindNearestGun()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		ItemGun result = null;
		float num = 1f / 0f;
		ItemGun[] array = Object.FindObjectsOfType<ItemGun>();
		Vector3 position = ((Component)this).transform.position;
		float num2 = _cfg.SearchRadius * _cfg.SearchRadius;
		BotWeaponUser botWeaponUser = default(BotWeaponUser);
		foreach (ItemGun val in array)
		{
			if (Object.op_Implicit((Object)(object)val) && IsValidTarget(val) && !IsLikelyInsideExtractor(((Component)val).transform) && (!Claims.TryGetValue(((Object)val).GetInstanceID(), ref botWeaponUser) || !Object.op_Implicit((Object)(object)botWeaponUser) || !((Object)(object)botWeaponUser != (Object)(object)this)))
			{
				Vector3 val2 = ((Component)val).transform.position - position;
				float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
				if (!(sqrMagnitude > num2) && sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = val;
				}
			}
		}
		return result;
	}

	private Transform FindEnemyConstrained(out Vector3 aimPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		aimPos = ((Component)this).transform.position + ((Component)this).transform.forward * 6f;
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		Transform result = null;
		float num = 1f / 0f;
		Vector3 val = (Object.op_Implicit((Object)(object)_hold) ? _hold.position : (((Component)this).transform.position + Vector3.up * 1.5f));
		float num2 = Mathf.Cos((Object.op_Implicit((Object)(object)_persona) ? Mathf.Max(1f, _persona.AimConeDegrees) : 8f) * ((float)Math.PI / 180f));
		RaycastHit val5 = default(RaycastHit);
		foreach (Enemy val2 in array)
		{
			if (!Object.op_Implicit((Object)(object)val2) || !((Component)val2).gameObject.activeInHierarchy)
			{
				continue;
			}
			Transform transform = ((Component)val2).transform;
			Vector3 val3 = transform.position + Vector3.up * 0.6f;
			Vector3 val4 = val3 - val;
			float sqrMagnitude = ((Vector3)(ref val4)).sqrMagnitude;
			if (!(sqrMagnitude > 900f))
			{
				Vector3 normalized = ((Vector3)(ref val4)).normalized;
				if (!(Vector3.Dot(((Component)this).transform.forward, normalized) < num2) && (!Physics.Raycast(val, normalized, ref val5, Mathf.Sqrt(sqrMagnitude), -1, (QueryTriggerInteraction)1) || (Object.op_Implicit((Object)(object)((RaycastHit)(ref val5)).collider) && Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref val5)).collider).GetComponentInParent<Enemy>()))) && sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = transform;
					aimPos = val3;
				}
			}
		}
		return result;
	}

	private bool IsLikelyInsideExtractor(Transform t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ExtractionPoint val = SemiFunc.ExtractionPointGetNearest(t.position);
		if (!Object.op_Implicit((Object)(object)val))
		{
			return false;
		}
		return Vector3.Distance(t.position, ((Component)val).transform.position) <= 1.25f;
	}

	private void SetCarryCollisionIgnore(bool ignore)
	{
		if (!Object.op_Implicit((Object)(object)_gun))
		{
			return;
		}
		_gunCols.Clear();
		((Component)_gun).GetComponentsInChildren<Collider>(true, _gunCols);
		if (_myCols == null || _myCols.Length == 0)
		{
			_myCols = ((Component)this).GetComponentsInChildren<Collider>(true);
		}
		for (int i = 0; i < _myCols.Length; i++)
		{
			Collider val = _myCols[i];
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			for (int j = 0; j < _gunCols.Count; j++)
			{
				Collider val2 = _gunCols[j];
				if (Object.op_Implicit((Object)(object)val2))
				{
					Physics.IgnoreCollision(val, val2, ignore);
				}
			}
		}
	}

	private void HookGunEvents(ItemGun g, bool on)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)g) && _subscribed != on)
		{
			_subscribed = on;
			if (on)
			{
				g.onStateOutOfAmmoStart.AddListener(new UnityAction(OnGunOutOfAmmoStart));
			}
			else
			{
				g.onStateOutOfAmmoStart.RemoveListener(new UnityAction(OnGunOutOfAmmoStart));
			}
		}
	}

	private void OnGunOutOfAmmoStart()
	{
		if (Object.op_Implicit((Object)(object)_gun))
		{
			int instanceID = ((Object)_gun).GetInstanceID();
			EmptyUntil[instanceID] = Time.time + 25f;
		}
		Drop(immediate: false);
	}

	private bool TryClaim(ItemGun g)
	{
		if (!Object.op_Implicit((Object)(object)g))
		{
			return false;
		}
		int instanceID = ((Object)g).GetInstanceID();
		BotWeaponUser botWeaponUser = default(BotWeaponUser);
		if (Claims.TryGetValue(instanceID, ref botWeaponUser) && Object.op_Implicit((Object)(object)botWeaponUser) && (Object)(object)botWeaponUser != (Object)(object)this)
		{
			return false;
		}
		Claims[instanceID] = this;
		return true;
	}

	private void Release(ItemGun g)
	{
		if (Object.op_Implicit((Object)(object)g))
		{
			int instanceID = ((Object)g).GetInstanceID();
			BotWeaponUser botWeaponUser = default(BotWeaponUser);
			if (Claims.TryGetValue(instanceID, ref botWeaponUser) && (Object)(object)botWeaponUser == (Object)(object)this)
			{
				Claims.Remove(instanceID);
			}
		}
	}
}
