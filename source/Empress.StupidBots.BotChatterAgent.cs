using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Empress.StupidBots;

internal class BotChatterAgent : MonoBehaviour
{
	public enum BotState : Enum
	{
		Idle,
		Moving,
		Carrying,
		Ragdoll
	}

	private NavMeshAgent _agent;

	private Rigidbody _rb;

	private AudioSource _src;

	private readonly List<PhysGrabObject> _buf = new List<PhysGrabObject>();

	private float _lastLocalSpeak;

	private bool _lastCarrying;

	private string _lastCarriedName;

	[field: CompilerGenerated]
	public BotState State
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public bool IsCarrying
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	[field: CompilerGenerated]
	public string CarriedLabel
	{
		[CompilerGenerated]
		get;
		[CompilerGenerated]
		private set;
	}

	private void Awake()
	{
		_agent = ((Component)this).GetComponent<NavMeshAgent>();
		_rb = ((Component)this).GetComponent<Rigidbody>();
		_src = ((Component)this).GetComponent<AudioSource>();
		if (!Object.op_Implicit((Object)(object)_src))
		{
			_src = ((Component)this).gameObject.AddComponent<AudioSource>();
		}
		_src.spatialBlend = 1f;
		_src.rolloffMode = (AudioRolloffMode)0;
		_src.minDistance = 2f;
		_src.maxDistance = 18f;
	}

	private void OnEnable()
	{
		BotChatterManager.Register(this);
	}

	private void OnDisable()
	{
		BotChatterManager.Unregister(this);
	}

	private void Update()
	{
		UpdateState();
	}

	private void UpdateState()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (Object.op_Implicit((Object)(object)_agent) && ((Behaviour)_agent).enabled)
		{
			Vector3 velocity = _agent.velocity;
			flag = ((Vector3)(ref velocity)).sqrMagnitude > 0.04f;
		}
		bool flag2 = false;
		if (Object.op_Implicit((Object)(object)_rb))
		{
			flag2 = !_rb.isKinematic && !((Behaviour)_agent).enabled;
		}
		DetectCarry(out bool carrying, out string label);
		IsCarrying = carrying;
		CarriedLabel = label;
		if (flag2)
		{
			State = BotState.Ragdoll;
		}
		else if (carrying)
		{
			State = BotState.Carrying;
		}
		else if (flag)
		{
			State = BotState.Moving;
		}
		else
		{
			State = BotState.Idle;
		}
	}

	private void DetectCarry(out bool carrying, out string label)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		carrying = false;
		label = "";
		_buf.Clear();
		((Component)this).GetComponentsInChildren<PhysGrabObject>(true, _buf);
		PhysGrabObject val = null;
		float num = 1f / 0f;
		for (int i = 0; i < _buf.Count; i++)
		{
			PhysGrabObject val2 = _buf[i];
			if (!Object.op_Implicit((Object)(object)val2))
			{
				continue;
			}
			Transform transform = ((Component)val2).transform;
			if (!((Object)(object)transform == (Object)(object)((Component)this).transform))
			{
				Vector3 val3 = transform.position - ((Component)this).transform.position;
				float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					val = val2;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			carrying = true;
			label = ((Object)((Component)val).gameObject).name;
		}
	}

	internal void PlayClip(DecTalkSynth.Clip clip, float volume)
	{
		if (Object.op_Implicit((Object)(object)_src))
		{
			AudioClip val = AudioClip.Create("BotChat", clip.Samples.Length / clip.Channels, clip.Channels, clip.SampleRate, false);
			val.SetData(clip.Samples, 0);
			_src.Stop();
			_src.clip = val;
			_src.loop = false;
			_src.volume = Mathf.Clamp01(volume);
			_src.Play();
		}
	}

	internal bool TickLocal(float now, bool canSpeak)
	{
		if (!canSpeak)
		{
			return false;
		}
		if (now < _lastLocalSpeak)
		{
			return false;
		}
		bool flag = false;
		if (IsCarrying && !_lastCarrying)
		{
			flag = true;
		}
		_lastCarrying = IsCarrying;
		if (!flag)
		{
			return false;
		}
		string text = (IsCarrying ? "Moving with package." : "Moving.");
		if (EmpressBotFriendsChatterPlugin.UseDECtalk.Value)
		{
			DecTalkSynth.Enqueue(this, text);
			_lastLocalSpeak = now + EmpressBotFriendsChatterPlugin.SelfCooldown.Value;
			return true;
		}
		return false;
	}
}
