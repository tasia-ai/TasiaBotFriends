using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Empress.StupidBots;

internal class StupidBotBrain : MonoBehaviour
{
	internal struct Config : ValueType
	{
		public bool FollowPlayer;

		public float WanderRadius;

		public float Speed;
	}

	private const float FollowOrbitRadius = 12f;

	private Config _cfg;

	private NavMeshAgent? _agent;

	private Transform? _player;

	private Vector3 _homePos;

	private float _retargetTimer;

	private float _emoteTimer;

	private Vector3 _lastPos;

	private float _stuckTimer;

	private bool _hopping;

	private readonly List<Vector3> _globalTargets = new List<Vector3>();

	private bool _haveGlobal;

	[field: CompilerGenerated]
	internal bool ExternalControl
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	internal void SetExternalControl(bool on)
	{
		ExternalControl = on;
	}

	internal void Init(Config cfg)
	{
		_cfg = cfg;
	}

	private void Start()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		_agent = ((Component)this).GetComponent<NavMeshAgent>();
		if (Object.op_Implicit((Object)(object)_agent))
		{
			if (!_agent.isOnNavMesh)
			{
				TryPlaceOnNavMesh(_agent, ((Component)this).transform.position);
			}
			_agent.isStopped = false;
			_agent.updateRotation = true;
			_agent.autoBraking = true;
		}
		_homePos = ((Component)this).transform.position;
		TryGetPlayer(out _player);
		_retargetTimer = Random.Range(0.25f, 1.25f);
		_emoteTimer = Random.Range(2.5f, 7.5f);
		_lastPos = ((Component)this).transform.position;
		BuildGlobalTargets();
	}

	private void Update()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Invalid comparison between Unknown and I4
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_agent) && ((Behaviour)_agent).enabled)
		{
			_agent.speed = _cfg.Speed;
		}
		if ((Object)(object)_player == (Object)null)
		{
			TryGetPlayer(out _player);
		}
		if (ExternalControl)
		{
			return;
		}
		_retargetTimer -= Time.deltaTime;
		bool flag = false;
		if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			bool num = !_agent.hasPath || (int)_agent.pathStatus > 0;
			bool flag2 = _agent.hasPath && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
			if (num || flag2 || _retargetTimer <= 0f)
			{
				flag = true;
			}
		}
		else if (_retargetTimer <= 0f)
		{
			flag = true;
		}
		if (flag)
		{
			_retargetTimer = Random.Range(1.6f, 3.2f);
			PickNextDestination();
		}
		_emoteTimer -= Time.deltaTime;
		if (_emoteTimer <= 0f)
		{
			_emoteTimer = Random.Range(4f, 10f);
			TryHop();
		}
		Vector3 val;
		if (Object.op_Implicit((Object)(object)_agent) && _agent.hasPath)
		{
			val = _agent.velocity;
			if (((Vector3)(ref val)).sqrMagnitude > 0.01f)
			{
				val = _agent.velocity;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				if (((Vector3)(ref normalized)).sqrMagnitude > 0.001f)
				{
					Quaternion val2 = Quaternion.LookRotation(normalized, Vector3.up);
					((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, val2, Time.deltaTime * 5f);
				}
			}
		}
		val = ((Component)this).transform.position - _lastPos;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		_stuckTimer = ((sqrMagnitude < 0.0009f) ? (_stuckTimer + Time.deltaTime) : 0f);
		_lastPos = ((Component)this).transform.position;
		if (_stuckTimer > 1.5f && Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			_stuckTimer = 0f;
			_agent.ResetPath();
			_agent.Warp(((Component)this).transform.position);
		}
	}

	private void PickNextDestination()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 homePos = _homePos;
		if (_cfg.FollowPlayer && Object.op_Implicit((Object)(object)_player))
		{
			Vector3 val = Random.insideUnitSphere;
			val.y = 0f;
			if (((Vector3)(ref val)).sqrMagnitude < 0.0001f)
			{
				val = Vector3.forward;
			}
			homePos = _player.position + ((Vector3)(ref val)).normalized * Random.Range(2.5f, 12f);
		}
		else if (_haveGlobal)
		{
			int num = Random.Range(0, _globalTargets.Count);
			homePos = _globalTargets[num];
		}
		else
		{
			homePos = _homePos + Random.insideUnitSphere.WithY(0f) * 25f;
		}
		if (Object.op_Implicit((Object)(object)_agent) && _agent.isOnNavMesh)
		{
			NavMeshHit val2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(homePos, ref val2, 3f, -1))
			{
				_agent.SetDestination(((NavMeshHit)(ref val2)).position);
			}
			else
			{
				_agent.SetDestination(homePos);
			}
			return;
		}
		Vector3 val3 = homePos - ((Component)this).transform.position;
		val3.y = 0f;
		float num2 = Mathf.Min(_cfg.Speed * 0.5f * Time.deltaTime, ((Vector3)(ref val3)).magnitude);
		if (num2 > 0f)
		{
			Transform transform = ((Component)this).transform;
			transform.position += ((Vector3)(ref val3)).normalized * num2;
		}
	}

	private void TryHop()
	{
		if (!((Object)(object)_agent == (Object)null) && !_hopping)
		{
			((MonoBehaviour)this).StartCoroutine(HopCR());
		}
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator HopCR()
	{
		_hopping = true;
		float start = _agent.baseOffset;
		float peak = start + 0.6f;
		float t2 = 0f;
		float up = 0.15f;
		while (t2 < up)
		{
			t2 += Time.deltaTime;
			_agent.baseOffset = Mathf.Lerp(start, peak, t2 / up);
			yield return null;
		}
		t2 = 0f;
		float down = 0.2f;
		while (t2 < down)
		{
			t2 += Time.deltaTime;
			_agent.baseOffset = Mathf.Lerp(peak, start, t2 / down);
			yield return null;
		}
		_agent.baseOffset = start;
		_hopping = false;
	}

	private static bool TryGetPlayer(out Transform? player)
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

	private static bool TryPlaceOnNavMesh(NavMeshAgent agent, Vector3 desired)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		NavMeshHit val = default(NavMeshHit);
		if (NavMesh.SamplePosition(desired, ref val, 6f, -1))
		{
			return agent.Warp(((NavMeshHit)(ref val)).position);
		}
		return agent.Warp(desired);
	}

	private void BuildGlobalTargets()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_globalTargets.Clear();
		try
		{
			if (Object.op_Implicit((Object)(object)LevelGenerator.Instance) && LevelGenerator.Instance.LevelPathPoints != null)
			{
				List<LevelPoint> levelPathPoints = LevelGenerator.Instance.LevelPathPoints;
				NavMeshHit val2 = default(NavMeshHit);
				for (int i = 0; i < levelPathPoints.Count; i++)
				{
					LevelPoint val = levelPathPoints[i];
					if (Object.op_Implicit((Object)(object)val) && NavMesh.SamplePosition(((Component)val).transform.position, ref val2, 3f, -1))
					{
						_globalTargets.Add(((NavMeshHit)(ref val2)).position);
					}
				}
			}
		}
		catch (Object)
		{
		}
		if (_globalTargets.Count == 0)
		{
			try
			{
				Vector3[] vertices = NavMesh.CalculateTriangulation().vertices;
				int num = Mathf.Max(1, vertices.Length / 64);
				NavMeshHit val4 = default(NavMeshHit);
				for (int j = 0; j < vertices.Length; j += num)
				{
					if (NavMesh.SamplePosition(vertices[j], ref val4, 3f, -1))
					{
						_globalTargets.Add(((NavMeshHit)(ref val4)).position);
					}
				}
			}
			catch (Object)
			{
			}
		}
		_haveGlobal = _globalTargets.Count > 0;
	}
}
