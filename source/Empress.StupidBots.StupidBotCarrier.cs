using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Empress.StupidBots;

internal class StupidBotCarrier : MonoBehaviour
{
	internal struct Cfg : ValueType
	{
		public float SearchRadius;

		public float ExtractorStopDistance;

		public float GentleDropTime;
	}

	private Cfg _cfg;

	private NavMeshAgent _agent;

	private Transform _hold;

	private StupidBotBrain _brain;

	private int _lastActivateId;

	private float _lastActivateTime;

	private Vector3 _truckDestination;

	private bool _truckCached;

	private PhysGrabObject _carried;

	private Rigidbody _carriedRB;

	private bool _busy;

	private bool _dropping;

	private float _retargetTimer;

	private float _savedStopDist;

	private Collider[] _myCols;

	private readonly List<Collider> _carriedCols = new List<Collider>();

	private readonly HashSet<int> _ignoreIds = new HashSet<int>();

	private float _activationGraceUntil;

	private float _carryTimer;

	private bool _isJumping;

	private float _jumpCD;

	private readonly Collider[] _overlap = (Collider[])(object)new Collider[32];

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)_carried))
		{
			ForceDrop(immediate: true);
		}
	}

	internal void Init(Cfg cfg, NavMeshAgent agent, Transform holdPoint, StupidBotBrain brain)
	{
		_myCols = ((Component)this).GetComponentsInChildren<Collider>(true);
		_cfg = cfg;
		_agent = agent;
		_hold = holdPoint;
		_brain = brain;
		_retargetTimer = Random.Range(0.25f, 0.75f);
		_activationGraceUntil = 0f;
		_carryTimer = 0f;
		_isJumping = false;
		_jumpCD = 0f;
	}

	private void Update()
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_carried))
		{
			_brain.SetExternalControl(on: true);
			_carryTimer += Time.deltaTime;
			ExtractionPoint targetEP = null;
			if (TryResolveDeliveryTarget(out Vector3 goal, out targetEP))
			{
				if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
				{
					if (!_agent.hasPath || Vector3.SqrMagnitude(_agent.destination - goal) > 0.25f)
					{
						_agent.SetDestination(goal);
					}
					_agent.stoppingDistance = Mathf.Clamp(_cfg.ExtractorStopDistance * 0.5f, 0.2f, 1f);
				}
				float num = Vector3.Distance(((Component)this).transform.position, goal);
				if (!_dropping && num <= Mathf.Max(0.5f, _cfg.ExtractorStopDistance + 0.25f))
				{
					if (IsDeliveryDropAllowed(targetEP) || Time.time < _activationGraceUntil || _carryTimer > 12f)
					{
						_dropping = true;
						((MonoBehaviour)this).StartCoroutine(DropGently());
					}
					else if (Object.op_Implicit((Object)(object)targetEP))
					{
						TryActivateExtraction(targetEP);
					}
				}
			}
			if (Object.op_Implicit((Object)(object)_carried) && !_dropping)
			{
				Transform transform = ((Component)_carried).transform;
				if ((Object)(object)transform.parent != (Object)(object)_hold)
				{
					transform.SetParent(_hold, false);
				}
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
			return;
		}
		_carryTimer = 0f;
		if (_busy)
		{
			return;
		}
		_retargetTimer -= Time.deltaTime;
		if (_retargetTimer > 0f)
		{
			return;
		}
		_retargetTimer = Random.Range(0.9f, 1.4f);
		List<PhysGrabObject> obj = BotUtils.FindNearbyValuables(((Component)this).transform.position, _cfg.SearchRadius);
		PhysGrabObject val = null;
		float num2 = 1f / 0f;
		Enumerator<PhysGrabObject> enumerator = obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PhysGrabObject current = enumerator.Current;
				if (Object.op_Implicit((Object)(object)current) && !SemiFunc.PhysGrabObjectIsGrabbed(current) && Object.op_Implicit((Object)(object)((Component)current).GetComponent<ValuableObject>()) && !_ignoreIds.Contains(((Object)current).GetInstanceID()) && !BotUtils.IsIgnored(current) && !BotUtils.IsClaimedByOther(current, this) && !IsLikelyInsideExtractor(((Component)current).transform))
				{
					float num3 = Vector3.SqrMagnitude(((Component)current).transform.position - ((Component)this).transform.position);
					if (num3 < num2)
					{
						num2 = num3;
						val = current;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		if (!Object.op_Implicit((Object)(object)val) || !BotUtils.TryClaim(val, this))
		{
			return;
		}
		_brain.SetExternalControl(on: true);
		_busy = true;
		if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			Vector3 position = ((Component)val).transform.position;
			NavMeshHit val2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(position, ref val2, 2.5f, -1))
			{
				_agent.SetDestination(((NavMeshHit)(ref val2)).position);
			}
			else
			{
				_agent.SetDestination(position);
			}
		}
		((MonoBehaviour)this).StartCoroutine(ApproachAndPick(val));
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator ApproachAndPick(PhysGrabObject target)
	{
		float stoppingDistance = 0.15f;
		float prevStop = (Object.op_Implicit((Object)(object)_agent) ? _agent.stoppingDistance : 0f);
		if (Object.op_Implicit((Object)(object)_agent))
		{
			_agent.stoppingDistance = stoppingDistance;
		}
		float giveUp = 10f;
		while (giveUp > 0f && Object.op_Implicit((Object)(object)target) && Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			giveUp -= Time.deltaTime;
			if (BotUtils.IsClaimedByOther(target, this))
			{
				BotUtils.ReleaseClaim(target, this);
				break;
			}
			if (CanReachByHold(target) || CanTouchTarget(target))
			{
				break;
			}
			TryJumpAssist(target);
			yield return null;
		}
		if (Object.op_Implicit((Object)(object)_agent))
		{
			_agent.stoppingDistance = prevStop;
		}
		if (!Object.op_Implicit((Object)(object)target))
		{
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		if (!Object.op_Implicit((Object)(object)_agent) || !_agent.isOnNavMesh)
		{
			BotUtils.ReleaseClaim(target, this);
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		if (SemiFunc.PhysGrabObjectIsGrabbed(target))
		{
			BotUtils.ReleaseClaim(target, this);
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		if (!CanReachByHold(target) && !CanTouchTarget(target))
		{
			BotUtils.ReleaseClaim(target, this);
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		bool flag = false;
		Vector3 val = (Object.op_Implicit((Object)(object)_hold) ? _hold.position : (((Component)this).transform.position + Vector3.up * 1.5f));
		Vector3 val2 = ((Component)target).transform.position + Vector3.up * 0.1f;
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Linecast(val, val2, ref val3, -1, (QueryTriggerInteraction)1))
		{
			Transform transform = ((RaycastHit)(ref val3)).transform;
			if (Object.op_Implicit((Object)(object)transform) && (Object)(object)transform != (Object)(object)((Component)target).transform && !transform.IsChildOf(((Component)target).transform))
			{
				flag = true;
			}
		}
		if (flag && (CanReachByHold(target) || CanTouchTarget(target)))
		{
			flag = false;
		}
		if (flag)
		{
			BotUtils.ReleaseClaim(target, this);
			_busy = false;
			_brain.SetExternalControl(on: false);
			yield break;
		}
		if (Object.op_Implicit((Object)(object)target.rb) && !target.rb.isKinematic && (CanReachByHold(target) || CanTouchTarget(target)))
		{
			Vector3 val4 = (((Component)target).transform.position - ((Component)this).transform.position).WithY(0f);
			Vector3 val5 = ((Vector3)(ref val4)).normalized;
			if (((Vector3)(ref val5)).sqrMagnitude < 0.0001f)
			{
				val5 = ((Component)this).transform.forward;
			}
			try
			{
				target.rb.AddForce(val5 * 1.25f + Vector3.down * 2f, (ForceMode)1);
			}
			catch (Object)
			{
			}
			yield return new WaitForSeconds(0.05f);
		}
		DetachExternalLinks(target);
		_carried = target;
		_carriedRB = target.rb;
		try
		{
			((Component)_carried).transform.SetParent((Transform)null, true);
			if (Object.op_Implicit((Object)(object)_carriedRB))
			{
				_carriedRB.velocity = Vector3.zero;
				_carriedRB.angularVelocity = Vector3.zero;
				_carriedRB.isKinematic = true;
			}
			target.OverrideKnockOutOfGrabDisable(2f);
		}
		catch (Object)
		{
		}
		SetCarryCollisionIgnore(ignore: true);
		if (Object.op_Implicit((Object)(object)_agent))
		{
			_savedStopDist = _agent.stoppingDistance;
		}
		_carryTimer = 0f;
	}

	private bool CanReachByHold(PhysGrabObject target)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Object.op_Implicit((Object)(object)_hold) ? _hold.position : (((Component)this).transform.position + Vector3.up * 1.5f));
		float num = 1f / 0f;
		float num2 = 1f / 0f;
		_carriedCols.Clear();
		((Component)target).GetComponentsInChildren<Collider>(true, _carriedCols);
		for (int i = 0; i < _carriedCols.Count; i++)
		{
			Collider val2 = _carriedCols[i];
			if (Object.op_Implicit((Object)(object)val2))
			{
				Vector3 val3 = val2.ClosestPoint(val);
				float num3 = Vector3.Distance(val, val3);
				if (num3 < num)
				{
					num = num3;
				}
				float num4 = Mathf.Abs(val3.y - val.y);
				if (num4 < num2)
				{
					num2 = num4;
				}
			}
		}
		if (Single.IsInfinity(num))
		{
			num = Vector3.Distance(val, ((Component)target).transform.position);
			num2 = Mathf.Abs(((Component)target).transform.position.y - val.y);
		}
		Vector3 position = ((Component)this).transform.position;
		position.y = 0f;
		Vector3 position2 = ((Component)target).transform.position;
		position2.y = 0f;
		float num5 = Vector3.Distance(position, position2);
		if (num <= 1.5f && num5 <= 2.2f)
		{
			return true;
		}
		if (num5 <= 2.5f && num2 <= 2.2f)
		{
			return true;
		}
		return false;
	}

	private bool CanTouchTarget(PhysGrabObject target)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		int num = Physics.OverlapSphereNonAlloc(Object.op_Implicit((Object)(object)_hold) ? _hold.position : (((Component)this).transform.position + Vector3.up * 1.5f), 0.5f, _overlap, -1, (QueryTriggerInteraction)2);
		for (int i = 0; i < num; i++)
		{
			Collider obj = _overlap[i];
			Transform val = ((obj != null) ? ((Component)obj).transform : null);
			if (Object.op_Implicit((Object)(object)val) && ((Object)(object)val == (Object)(object)((Component)target).transform || val.IsChildOf(((Component)target).transform)))
			{
				return true;
			}
		}
		return false;
	}

	private void DetachExternalLinks(PhysGrabObject target)
	{
		try
		{
			Joint[] componentsInChildren = ((Component)target).GetComponentsInChildren<Joint>(true);
			foreach (Joint val in componentsInChildren)
			{
				if (!Object.op_Implicit((Object)(object)val))
				{
					continue;
				}
				Rigidbody connectedBody = val.connectedBody;
				if (Object.op_Implicit((Object)(object)connectedBody))
				{
					Transform transform = ((Component)connectedBody).transform;
					if (Object.op_Implicit((Object)(object)transform) && !transform.IsChildOf(((Component)target).transform) && (Object)(object)transform != (Object)(object)((Component)target).transform)
					{
						DisableJoint(val);
					}
				}
			}
		}
		catch (Object)
		{
		}
		try
		{
			((Component)target).transform.SetParent((Transform)null, true);
		}
		catch (Object)
		{
		}
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator DropGently()
	{
		if (!Object.op_Implicit((Object)(object)_carried))
		{
			_dropping = false;
			yield break;
		}
		Transform tform = ((Component)_carried).transform;
		try
		{
			tform.SetParent((Transform)null, true);
		}
		catch (Object)
		{
		}
		Vector3 start = tform.position;
		ExtractionPoint val2 = SemiFunc.ExtractionPointGetNearest(((Component)this).transform.position);
		Vector3 val3;
		Vector3 val4;
		if (!Object.op_Implicit((Object)(object)val2))
		{
			val3 = ((Component)this).transform.forward;
		}
		else
		{
			val4 = (((Component)val2).transform.position - ((Component)this).transform.position).WithY(0f);
			val3 = ((Vector3)(ref val4)).normalized;
		}
		Vector3 val5 = val3;
		if (((Vector3)(ref val5)).sqrMagnitude < 0.0001f)
		{
			val5 = ((Component)this).transform.forward;
		}
		Vector3 end = start + val5 * 0.6f + Vector3.down * 0.3f;
		float t = 0f;
		float dur = Mathf.Max(0.05f, _cfg.GentleDropTime);
		while (t < dur && Object.op_Implicit((Object)(object)_carried))
		{
			t += Time.deltaTime;
			float num = Mathf.Clamp01(t / dur);
			tform.position = Vector3.Lerp(start, end, num);
			tform.rotation = Quaternion.Slerp(tform.rotation, Quaternion.identity, num * 0.5f);
			yield return null;
		}
		ForceDropInternal(reenableCollision: false);
		if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			Vector3 position = ((Component)this).transform.position;
			val4 = Random.insideUnitSphere.WithY(0f);
			NavMeshHit val6 = default(NavMeshHit);
			if (NavMesh.SamplePosition(position + ((Vector3)(ref val4)).normalized * 1.2f, ref val6, 2f, -1))
			{
				_agent.SetDestination(((NavMeshHit)(ref val6)).position);
			}
		}
		yield return new WaitForFixedUpdate();
		HardReleaseAndSeparate(((Component)this).transform.position);
		FinalizeDropCommon();
		_dropping = false;
	}

	private void HardReleaseAndSeparate(Vector3 awayFrom, float minSeparation = 0.9f)
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_carried == (Object)null)
		{
			return;
		}
		try
		{
			((Component)_carried).transform.SetParent((Transform)null, true);
		}
		catch (Object)
		{
		}
		try
		{
			SetCarryCollisionIgnore(ignore: false);
		}
		catch (Object)
		{
		}
		if (Object.op_Implicit((Object)(object)_carriedRB))
		{
			try
			{
				_carriedRB.isKinematic = false;
				_carriedRB.detectCollisions = true;
				if ((int)_carriedRB.collisionDetectionMode == 0)
				{
					_carriedRB.collisionDetectionMode = (CollisionDetectionMode)2;
				}
				_carriedRB.velocity = Vector3.zero;
				_carriedRB.angularVelocity = Vector3.zero;
			}
			catch (Object)
			{
			}
		}
		try
		{
			Joint[] components = ((Component)((Component)_carried).transform).GetComponents<Joint>();
			foreach (Joint val4 in components)
			{
				if (Object.op_Implicit((Object)(object)val4))
				{
					Rigidbody connectedBody = val4.connectedBody;
					if (Object.op_Implicit((Object)(object)connectedBody) && !((Object)(object)((Component)connectedBody).transform == (Object)null) && ((Object)(object)((Component)connectedBody).transform == (Object)(object)((Component)this).transform || ((Component)connectedBody).transform.IsChildOf(((Component)this).transform)))
					{
						DisableJoint(val4);
					}
				}
			}
		}
		catch (Object)
		{
		}
		Vector3 val6 = (((Component)_carried).transform.position - awayFrom).WithY(0f);
		if (((Vector3)(ref val6)).sqrMagnitude < 0.0001f)
		{
			val6 = ((Component)this).transform.forward;
		}
		val6 = ((Vector3)(ref val6)).normalized;
		Vector3 position = ((Component)_carried).transform.position + val6 * minSeparation;
		RaycastHit val7 = default(RaycastHit);
		if (Physics.Raycast(((Component)_carried).transform.position + Vector3.up * 0.1f, val6, ref val7, minSeparation, -1, (QueryTriggerInteraction)1))
		{
			position = ((RaycastHit)(ref val7)).point - val6 * 0.05f;
		}
		((Component)_carried).transform.position = position;
		Physics.SyncTransforms();
		try
		{
			if (Object.op_Implicit((Object)(object)_carriedRB))
			{
				_carriedRB.AddForce(val6 * 1.25f, (ForceMode)1);
			}
		}
		catch (Object)
		{
		}
	}

	private static void DisableJoint(Joint? joint)
	{
		if (!Object.op_Implicit((Object)(object)joint))
		{
			return;
		}
		try
		{
			joint.connectedBody = null;
		}
		catch (Object)
		{
		}
		try
		{
			joint.enableCollision = false;
		}
		catch (Object)
		{
		}
	}

	private void ForceDrop(bool immediate)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		ForceDropInternal(reenableCollision: true);
		if (!immediate && Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 val = Random.insideUnitSphere.WithY(0f);
			NavMeshHit val2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(position + ((Vector3)(ref val)).normalized * 1.2f, ref val2, 2f, -1))
			{
				_agent.SetDestination(((NavMeshHit)(ref val2)).position);
			}
		}
	}

	private void ForceDropInternal(bool reenableCollision)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_carried))
		{
			return;
		}
		PhysGrabObject carried = _carried;
		try
		{
			((Component)carried).transform.SetParent((Transform)null, true);
			if (Object.op_Implicit((Object)(object)_carriedRB) && reenableCollision)
			{
				_carriedRB.isKinematic = false;
				_carriedRB.velocity = Vector3.zero;
				_carriedRB.angularVelocity = Vector3.zero;
			}
		}
		catch (Object)
		{
		}
		ExtractionPoint val2 = null;
		try
		{
			val2 = SemiFunc.ExtractionPointGetNearest(((Component)this).transform.position);
		}
		catch (Object)
		{
			val2 = null;
		}
		if (Object.op_Implicit((Object)(object)val2) && Vector3.Distance(((Component)carried).transform.position, ((Component)val2).transform.position) <= Mathf.Max(0.75f, _cfg.ExtractorStopDistance + 0.25f))
		{
			_ignoreIds.Add(((Object)carried).GetInstanceID());
			BotUtils.IgnoreFor(carried, 3.5f);
		}
		if (!reenableCollision)
		{
			return;
		}
		try
		{
			SetCarryCollisionIgnore(ignore: false);
		}
		catch (Object)
		{
		}
		try
		{
			if (Object.op_Implicit((Object)(object)_carriedRB))
			{
				_carriedRB.isKinematic = false;
			}
		}
		catch (Object)
		{
		}
		FinalizeDropCommon();
	}

	private void FinalizeDropCommon()
	{
		BotUtils.ReleaseClaim(_carried, this);
		_carried = null;
		_carriedRB = null;
		_busy = false;
		_brain.SetExternalControl(on: false);
		_activationGraceUntil = 0f;
		_carryTimer = 0f;
		if (Object.op_Implicit((Object)(object)_agent))
		{
			_agent.stoppingDistance = _savedStopDist;
		}
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
		return Vector3.Distance(t.position, ((Component)val).transform.position) <= Mathf.Max(1f, _cfg.ExtractorStopDistance + 0.25f);
	}

	private void SetCarryCollisionIgnore(bool ignore)
	{
		if ((Object)(object)_carried == (Object)null)
		{
			return;
		}
		_carriedCols.Clear();
		((Component)_carried).GetComponentsInChildren<Collider>(true, _carriedCols);
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
			for (int j = 0; j < _carriedCols.Count; j++)
			{
				Collider val2 = _carriedCols[j];
				if (Object.op_Implicit((Object)(object)val2))
				{
					try
					{
						Physics.IgnoreCollision(val, val2, ignore);
					}
					catch (Object)
					{
					}
				}
			}
		}
	}

	private bool TryResolveDeliveryTarget(out Vector3 goal, out ExtractionPoint targetEP)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		goal = ((Component)this).transform.position;
		targetEP = null;
		try
		{
			if (Object.op_Implicit((Object)(object)RoundDirector.instance))
			{
				if (RoundDirector.instance.allExtractionPointsCompleted)
				{
					goal = GetTruckDestination();
					return true;
				}
				if (RoundDirector.instance.extractionPointActive && Object.op_Implicit((Object)(object)RoundDirector.instance.extractionPointCurrent))
				{
					targetEP = RoundDirector.instance.extractionPointCurrent;
					goal = ((Component)targetEP).transform.position;
					return true;
				}
				ExtractionPoint val = SemiFunc.ExtractionPointGetNearestNotActivated(((Component)this).transform.position);
				if (Object.op_Implicit((Object)(object)val))
				{
					targetEP = val;
					goal = ((Component)val).transform.position;
					return true;
				}
			}
		}
		catch (Object)
		{
		}
		ExtractionPoint val3 = SemiFunc.ExtractionPointGetNearest(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)val3))
		{
			targetEP = val3;
			goal = ((Component)val3).transform.position;
			return true;
		}
		return false;
	}

	private bool IsDeliveryDropAllowed(ExtractionPoint targetEP)
	{
		try
		{
			if (Object.op_Implicit((Object)(object)RoundDirector.instance))
			{
				if (RoundDirector.instance.allExtractionPointsCompleted)
				{
					return true;
				}
				if ((Object)(object)targetEP == (Object)null)
				{
					return false;
				}
				return RoundDirector.instance.extractionPointActive && (Object)(object)RoundDirector.instance.extractionPointCurrent == (Object)(object)targetEP;
			}
		}
		catch (Object)
		{
		}
		return true;
	}

	private void TryActivateExtraction(ExtractionPoint ep)
	{
		if (!Object.op_Implicit((Object)(object)ep))
		{
			return;
		}
		int instanceID = ((Object)ep).GetInstanceID();
		if (_lastActivateId == instanceID && Time.time - _lastActivateTime < 0.75f)
		{
			return;
		}
		_lastActivateId = instanceID;
		_lastActivateTime = Time.time;
		_activationGraceUntil = Time.time + 2f;
		try
		{
			ep.ButtonPress();
		}
		catch (Object)
		{
		}
	}

	private Vector3 GetTruckDestination()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (_truckCached)
		{
			return _truckDestination;
		}
		_truckDestination = ((Component)this).transform.position;
		try
		{
			if (Object.op_Implicit((Object)(object)LevelGenerator.Instance) && LevelGenerator.Instance.LevelPathPoints != null)
			{
				List<LevelPoint> levelPathPoints = LevelGenerator.Instance.LevelPathPoints;
				for (int i = 0; i < levelPathPoints.Count; i++)
				{
					LevelPoint val = levelPathPoints[i];
					if (Object.op_Implicit((Object)(object)val))
					{
						RoomVolume room = val.Room;
						if (Object.op_Implicit((Object)(object)room) && room.Truck)
						{
							_truckDestination = ((Component)val).transform.position;
							_truckCached = true;
							break;
						}
					}
				}
			}
		}
		catch (Object)
		{
		}
		return _truckDestination;
	}

	private void TryJumpAssist(PhysGrabObject target)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!_isJumping && !(_jumpCD > Time.time))
		{
			Vector3 val = (Object.op_Implicit((Object)(object)_hold) ? _hold.position : (((Component)this).transform.position + Vector3.up * 1.5f));
			Vector3 val2 = ((Component)target).transform.position - val;
			val2.y = 0f;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			RaycastHit val3 = default(RaycastHit);
			if (!(magnitude <= 2.5f) && !(magnitude > 5.5f) && Physics.Raycast(val + Vector3.up * 0.2f, ((Vector3)(ref val2)).normalized, ref val3, magnitude, -1, (QueryTriggerInteraction)1))
			{
				_isJumping = true;
				((MonoBehaviour)this).StartCoroutine(JumpCR());
			}
		}
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator JumpCR()
	{
		_jumpCD = Time.time + 1.25f;
		if (Object.op_Implicit((Object)(object)_agent))
		{
			NavMeshAgent agent = _agent;
			agent.baseOffset += 0.4f;
		}
		float t = 0f;
		float dur = 0.25f;
		while (t < dur)
		{
			t += Time.deltaTime;
			yield return null;
		}
		if (Object.op_Implicit((Object)(object)_agent))
		{
			NavMeshAgent agent2 = _agent;
			agent2.baseOffset -= 0.4f;
		}
		_isJumping = false;
	}
}
