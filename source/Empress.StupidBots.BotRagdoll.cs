using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Empress.StupidBots;

internal class BotRagdoll : MonoBehaviour
{
	private bool _feature;

	private float _thresholdSqr;

	private float _recover;

	private Rigidbody _rb;

	private NavMeshAgent _agent;

	private StupidBotBrain _brain;

	private StupidBotCarrier _carrier;

	private bool _active;

	private bool _carrierPrevEnabled;

	private float _timer;

	internal void Init(NavMeshAgent agent, StupidBotBrain brain, StupidBotCarrier carrier, Rigidbody rb, bool enabled, float threshold, float recover)
	{
		_agent = agent;
		_brain = brain;
		_carrier = carrier;
		_rb = rb;
		_feature = enabled;
		_thresholdSqr = Mathf.Max(0.01f, threshold * threshold);
		_recover = Mathf.Max(0.5f, recover);
	}

	private void OnCollisionEnter(Collision c)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (_feature && !_active)
		{
			Vector3 relativeVelocity = c.relativeVelocity;
			if (((Vector3)(ref relativeVelocity)).sqrMagnitude >= _thresholdSqr)
			{
				Activate();
			}
		}
	}

	private void Activate()
	{
		_active = true;
		_timer = 0f;
		if (Object.op_Implicit((Object)(object)_carrier))
		{
			_carrierPrevEnabled = ((Behaviour)_carrier).enabled;
			((Behaviour)_carrier).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)_agent))
		{
			((Behaviour)_agent).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)_brain))
		{
			_brain.SetExternalControl(on: true);
		}
		if (Object.op_Implicit((Object)(object)_rb))
		{
			_rb.isKinematic = false;
		}
	}

	private void Deactivate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_active = false;
		if (Object.op_Implicit((Object)(object)_rb))
		{
			_rb.velocity = Vector3.zero;
			_rb.angularVelocity = Vector3.zero;
			_rb.isKinematic = true;
		}
		if (Object.op_Implicit((Object)(object)_agent))
		{
			((MonoBehaviour)this).StartCoroutine(EnableAgent());
			return;
		}
		if (Object.op_Implicit((Object)(object)_brain))
		{
			_brain.SetExternalControl(on: false);
		}
		if (Object.op_Implicit((Object)(object)_carrier) && _carrierPrevEnabled)
		{
			((Behaviour)_carrier).enabled = true;
		}
	}

	[IteratorStateMachine(/*Could not decode attribute arguments.*/)]
	private IEnumerator EnableAgent()
	{
		yield return null;
		if (Object.op_Implicit((Object)(object)_agent))
		{
			_agent.Warp(((Component)this).transform.position);
		}
		if (Object.op_Implicit((Object)(object)_agent))
		{
			((Behaviour)_agent).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)_brain))
		{
			_brain.SetExternalControl(on: false);
		}
		if (Object.op_Implicit((Object)(object)_carrier) && _carrierPrevEnabled)
		{
			((Behaviour)_carrier).enabled = true;
		}
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!_feature || !_active)
		{
			return;
		}
		_timer += Time.deltaTime;
		if (!(_timer < _recover) && Object.op_Implicit((Object)(object)_rb))
		{
			Vector3 velocity = _rb.velocity;
			if (((Vector3)(ref velocity)).sqrMagnitude < 0.16f)
			{
				Deactivate();
			}
		}
	}

	internal void SetEnabled(bool on)
	{
		_feature = on;
	}

	internal void Reconfigure(float threshold, float recover)
	{
		_thresholdSqr = Mathf.Max(0.01f, threshold * threshold);
		_recover = Mathf.Max(0.5f, recover);
	}
}
