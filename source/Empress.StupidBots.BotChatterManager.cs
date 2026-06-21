using System;
using System.Collections.Generic;
using UnityEngine;

namespace Empress.StupidBots;

internal class BotChatterManager : MonoBehaviour
{
	private static readonly List<BotChatterAgent> Agents = new List<BotChatterAgent>();

	private static readonly Dictionary<BotChatterAgent, float> SelfCD = new Dictionary<BotChatterAgent, float>();

	private static readonly Dictionary<ulong, float> PairCD = new Dictionary<ulong, float>();

	private static int _voicesPlaying;

	private static readonly string[] greetings = (string[])(object)new String[41]
	{
		"Eyes up.", "Stay sharp.", "You good.", "Keep moving.", "We move.", "Copy.", "On route.", "Heads on a swivel.", "Focus up. No sightseeing.", "Don’t get cute—move.",
		"Keep it tight.", "We are burning daylight.", "Quit gawking and hustle.", "Stay frosty.", "Watch your damn corners.", "Steady hands, steady feet.", "Less chatter, more ladder.", "Walk like you mean it.", "No hero plays. Not today.", "We’re ghosts—be loud later.",
		"If it moves, clock it. Twice.", "This place gives me the creeps—keep pace.", "Control your breathing.", "Don’t make me babysit you.", "We’re not lost… we’re exploring aggressively.", "Move smart, not pretty.", "Keep the line clean.", "Comms clear unless it’s critical.", "Don’t trip over your own ego.", "We’re here to work, not decorate the floor.",
		"If it bites, bite back harder.", "We’re fine. Act like it.", "Heads down, wallets up.", "No panic. Panic is expensive.", "Shut it and strut it.", "Grip it and zip it.", "Keep your soul inside your body.", "Clock’s ticking. Move your ass.", "Don’t test me—I will turn this run around.", "Game faces, criminals.",
		"Eyes, ears, all of it. On."
	};

	private static readonly string[] movePair = (string[])(object)new String[38]
	{
		"Form up.", "Stay with me.", "Hold pace.", "On your six.", "Push up.", "Stack up.", "Anchor left.", "Peel right.", "Crossing!", "Bounding—go!",
		"Don’t lag. I swear.", "Hold the line.", "Break contact if it gets stupid.", "Tight corners—no heroics.", "Don’t step on the loud things.", "Feet light, brain heavier.", "Keep the spacing, geniuses.", "Eyes high, eyes low—cover both.", "Switching lanes—don’t collide.", "Check your flanks, then check mine.",
		"On your shoulder—don’t stop.", "You lead, I bully.", "Move like rent’s due.", "Silent running—save the drama.", "We glide. We don’t stumble.", "If you clatter, I yell.", "Shuffle faster. Yes, that’s a thing.", "Doors ahead—don’t faceplant.", "I’m pacing you—don’t make me sprint.", "Breach and breathe.",
		"Keep momentum—friction kills.", "I step, you step. Rhythm, people.", "No zig-zagging like a drunk drone.", "Heel-toe, not heel-whoops.", "Stay latched.", "Trail tight. No wandering.", "Clear lanes. Clear heads.", "Eyes where your feet will be."
	};

	private static readonly string[] carryLead = (string[])(object)new String[29]
	{
		"Got one.", "Package in hand.", "Taking loot.", "I have it.", "Hauling this thing—don’t make me drop it.", "I’ve got the goods. Nobody sneeze.", "Bag secured—act like professionals.", "Got the prize. Try not to die.", "Courier mode. Cover me, damn it.", "I’m slow and expensive—guard me.",
		"This is worth more than your life. Move.", "Heavy as sin. Still mine.", "If I trip, I’m blaming you.", "I am the mule now. Respect it.", "Loot in hand, patience in short supply.", "Treasures secured—tempers unsecured.", "Dragging gold and my feet.", "My back hates you and this.", "If I drop this, I drop you.", "I’m the target now. Smile.",
		"One package. Zero excuses.", "I lift, you shoot.", "I’m the paycheck with legs.", "This better fence for a fortune.", "Got the shiny—eyes open.", "If I hear clattering, I’ll scream.", "Big bag, bigger attitude.", "Toting loot like a legend.", "Carrier pigeon, but angrier."
	};

	private static readonly string[] carryLeadName = (string[])(object)new String[26]
	{
		"Carrying {0}.", "I have {0}.", "Package is {0}.", "{0} secured.", "Got {0}. Try and keep up.", "If {0} hits the floor, so do you.", "{0} is mine. Back off.", "Walking paycheck: {0}.", "Escorting {0} to freedom.", "This {0} better be worth the back pain.",
		"Cradling {0} like it’s sacred.", "Babysitting {0}. Cheer for me.", "Hauling {0}. Don’t test me.", "If {0} explodes, that’s on you.", "All roads lead to cash—starring {0}.", "Mother of loot: {0}.", "Dangerous and delicate: {0}.", "My spine signed for {0}.", "Do not poke {0}. Or me.", "If {0} falls, I fall on you.",
		"Escort detail for {0}, sadly.", "Moving {0}. Pretend we’re smart.", "I got {0}. Pray for my knees.", "This is {0}. Handle me gently.", "I’m marrying {0} if this pays out.", "Slapping a bow on {0} after this."
	};

	private static readonly string[] carrySupport = (string[])(object)new String[29]
	{
		"I cover.", "Clear the path.", "Move, I got you.", "Go extract.", "Covering your ass—don’t make me regret it.", "You lug it, I plug it.", "Overwatch up. Walk like a boss.", "I’m your shadow. Don’t outrun me.", "Keep that bag high, head low.", "I swat flies; you carry fortune.",
		"If it looks at you, I’ll make it stop.", "Doors first, problems later—move.", "Eyes on stairwells. Keep rolling.", "You stumble, I scream.", "No sightseeing. I clear, you scurry.", "I’m the umbrella. Don’t step out.", "Take corners wide. I’ve got lanes.", "If you drop it, I drop you. Kidding. Maybe.", "Focus on feet—I’ll handle teeth.", "Breathe and push. I’ll be loud.",
		"You’re precious cargo now. Try to act it.", "Nothing touches you unless I say so.", "Keep walking. I’m writing threats.", "Move like it’s hot—because it is.", "I’ll stitch the noise. You hug the loot.", "Shield up—go, go, go.", "If it snarls, I bark louder.", "Run lines, not circles.", "I’m the broom; you’re the mess. Sweep time."
	};

	private static readonly string[] carryBothA = (string[])(object)new String[27]
	{
		"Race you to the gate.", "Two packages.", "Double score.", "Heavy run.", "Two mules, one finish line.", "We’re the economy now.", "Twin jackpots. Try not to choke.", "Don’t brag until we cash in.", "We wobble, we win.", "If we drop both, I’m faking my death.",
		"Heavy squad, heavier attitude.", "We look ridiculous—keep running.", "Hoarders anonymous, speedrun edition.", "Two bags, zero shame.", "We are walking vaults. Guard us.", "This is greed on legs.", "Call us profit with shoes.", "Double trouble, double payday.", "We’re slow, dumb, and loaded.", "This path better be short.",
		"If gravity wins, we riot.", "We flex, the world pays.", "Clutch up; both bags live.", "Don’t trip; I will laugh first, help second.", "Two payloads, one exit, no mercy.", "The gate owes us applause.", "We’re bringing the store home."
	};

	private static readonly string[] carryBothB = (string[])(object)new String[25]
	{
		"Do not trip.", "Do not drop it.", "Keep pace.", "Stay clean.", "One slip and we’re memes.", "Eyes down; ankles up.", "We fumble and I’m screaming.", "No hero turns, just straight lines.", "Left foot, right foot, pay day.", "Balance or embarrassment—pick one.",
		"Bags high, egos low.", "Close ranks—wobble later.", "No sudden stops, tourists.", "Heels quiet, hearts quieter.", "Grip like your rent depends on it.", "Don’t argue—oxygen is precious.", "We are not sprinting—yet.", "Stop clanking. Sound travels.", "If you sneeze, you carry both.", "Hands steady, brain steady.",
		"We finish or we suffer.", "Minimal drama. Maximal loot.", "If we make it, I’m napping forever.", "Don’t tail-slap me with that bag.", "Tiptoe like thieves—because we are."
	};

	private static readonly string[] ragdollSelf = (string[])(object)new String[29]
	{
		"I am fine.", "Minor setback.", "Systems nominal.", "Back up.", "Ow. That sucked.", "I tripped. The floor punched back.", "Body’s fine. Pride’s bleeding.", "I meant to do that. Obviously.", "I slipped on pure stupidity.", "I’m okay—gravity cheated.",
		"Who moved the ground? Rude.", "That was tactical… falling.", "Don’t clip that. I swear.", "My knees are filing complaints.", "I’m good. I’m great. I’m lying.", "Did the earth jump up or…", "Studying floor patterns. For science.", "I’m vertical-adjacent, not down.", "Ragdoll mode: deactivated. Mostly.", "I’ll bounce. Watch me.",
		"Floor tasted like disappointment.", "I’ve had worse hugs than that wall.", "No bones broken, just my ego.", "Up. Angry. Motivated.", "Gravity and I are not friends.", "Walk it off. Swear it off.", "Pain is temporary, swearing eternal.", "Okay, that was spicy.", "If you laugh, you carry."
	};

	private static readonly string[] ragdollOther = (string[])(object)new String[27]
	{
		"Get up.", "Move soldier.", "No downtime.", "Recover now.", "Up you get, floor-lover.", "Stop kissing the concrete.", "On your feet, drama queen.", "Walk it off, champion of gravity.", "The ground isn’t your friend—move.", "No naps mid-run. Up.",
		"I’ll drag you, but I’ll complain loudly.", "Quit breakdancing. We’re late.", "Try legs. They’re great.", "Pretend you’re tough. Stand.", "You good? Too bad, move.", "We can cry after payday.", "You fall, we stall—rise.", "Come on, my back hurts watching.", "Shake it off. Angrier.", "If you’re dead, say so louder.",
		"You bend, don’t break. Go.", "If you do that again, I’m charging rent.", "I’ll count to one. Up.", "Dust off and scare the floor back.", "We leave no teammate—get moving.", "Gravity won the round, not the match.", "I didn’t see anything. Move."
	};

	internal static void Register(BotChatterAgent a)
	{
		if (!Agents.Contains(a))
		{
			Agents.Add(a);
		}
	}

	internal static void Unregister(BotChatterAgent a)
	{
		Agents.Remove(a);
		SelfCD.Remove(a);
	}

	private void Update()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		DrainReady();
		if (!EmpressBotFriendsChatterPlugin.Enabled.Value)
		{
			return;
		}
		for (int num = Agents.Count - 1; num >= 0; num--)
		{
			if (!Object.op_Implicit((Object)(object)Agents[num]))
			{
				Agents.RemoveAt(num);
			}
		}
		float time = Time.time;
		float num2 = default(float);
		for (int i = 0; i < Agents.Count; i++)
		{
			BotChatterAgent botChatterAgent = Agents[i];
			if (Object.op_Implicit((Object)(object)botChatterAgent) && ((Behaviour)botChatterAgent).isActiveAndEnabled)
			{
				if (!SelfCD.TryGetValue(botChatterAgent, ref num2))
				{
					num2 = 0f;
				}
				if (botChatterAgent.TickLocal(time, num2 <= time && RoomForVoice()))
				{
					SelfCD[botChatterAgent] = time + EmpressBotFriendsChatterPlugin.SelfCooldown.Value;
				}
			}
		}
		float num4 = default(float);
		for (int j = 0; j < Agents.Count; j++)
		{
			for (int k = j + 1; k < Agents.Count; k++)
			{
				BotChatterAgent botChatterAgent2 = Agents[j];
				BotChatterAgent botChatterAgent3 = Agents[k];
				if (Object.op_Implicit((Object)(object)botChatterAgent2) && Object.op_Implicit((Object)(object)botChatterAgent3) && !(Vector3.Distance(((Component)botChatterAgent2).transform.position, ((Component)botChatterAgent3).transform.position) > EmpressBotFriendsChatterPlugin.TalkRadius.Value))
				{
					ulong num3 = PairKey(botChatterAgent2, botChatterAgent3);
					if (!PairCD.TryGetValue(num3, ref num4))
					{
						num4 = 0f;
					}
					if (!(num4 > time) && RoomForVoice() && TryDialogue(botChatterAgent2, botChatterAgent3, time))
					{
						PairCD[num3] = time + EmpressBotFriendsChatterPlugin.PairCooldown.Value;
					}
				}
			}
		}
		_voicesPlaying = Mathf.Max(0, _voicesPlaying - 1);
		DrainReady();
	}

	private static void DrainReady()
	{
		BotChatterAgent who;
		DecTalkSynth.Clip clip;
		while (DecTalkSynth.TryDequeueReady(out who, out clip))
		{
			if (Object.op_Implicit((Object)(object)who))
			{
				who.PlayClip(clip, EmpressBotFriendsChatterPlugin.Volume.Value);
				_voicesPlaying++;
			}
		}
	}

	private static bool RoomForVoice()
	{
		return _voicesPlaying < EmpressBotFriendsChatterPlugin.MaxConcurrent.Value;
	}

	private static ulong PairKey(BotChatterAgent a, BotChatterAgent b)
	{
		int instanceID = ((Object)a).GetInstanceID();
		int instanceID2 = ((Object)b).GetInstanceID();
		uint num = (uint)Mathf.Min(instanceID, instanceID2);
		return ((ulong)(uint)Mathf.Max(instanceID, instanceID2) << 32) | num;
	}

	private static string Pick(string[] arr)
	{
		return arr[Random.Range(0, arr.Length)];
	}

	private static bool Speak(BotChatterAgent who, string line)
	{
		if (!EmpressBotFriendsChatterPlugin.UseDECtalk.Value)
		{
			return false;
		}
		DecTalkSynth.Enqueue(who, line);
		return true;
	}

	private static bool TryDialogue(BotChatterAgent a, BotChatterAgent b, float now)
	{
		BotChatterAgent.BotState state = a.State;
		BotChatterAgent.BotState state2 = b.State;
		if (state == BotChatterAgent.BotState.Ragdoll && state2 != BotChatterAgent.BotState.Ragdoll)
		{
			string line = Pick(ragdollSelf);
			string line2 = Pick(ragdollOther);
			bool num = Speak(a, line);
			if (num)
			{
				Speak(b, line2);
			}
			return num;
		}
		if (state2 == BotChatterAgent.BotState.Ragdoll && state != BotChatterAgent.BotState.Ragdoll)
		{
			string line3 = Pick(ragdollOther);
			string line4 = Pick(ragdollSelf);
			bool num2 = Speak(a, line3);
			if (num2)
			{
				Speak(b, line4);
			}
			return num2;
		}
		if (a.IsCarrying && !b.IsCarrying)
		{
			string carriedLabel = a.CarriedLabel;
			string line5 = (String.IsNullOrEmpty(carriedLabel) ? Pick(carryLead) : String.Format(Pick(carryLeadName), (object)carriedLabel));
			string line6 = Pick(carrySupport);
			bool num3 = Speak(a, line5);
			if (num3)
			{
				Speak(b, line6);
			}
			return num3;
		}
		if (b.IsCarrying && !a.IsCarrying)
		{
			string carriedLabel2 = b.CarriedLabel;
			string line7 = Pick(carrySupport);
			string line8 = (String.IsNullOrEmpty(carriedLabel2) ? Pick(carryLead) : String.Format(Pick(carryLeadName), (object)carriedLabel2));
			bool num4 = Speak(a, line7);
			if (num4)
			{
				Speak(b, line8);
			}
			return num4;
		}
		if (a.IsCarrying && b.IsCarrying)
		{
			string line9 = Pick(carryBothA);
			string line10 = Pick(carryBothB);
			bool num5 = Speak(a, line9);
			if (num5)
			{
				Speak(b, line10);
			}
			return num5;
		}
		if (state == BotChatterAgent.BotState.Moving && state2 == BotChatterAgent.BotState.Moving)
		{
			string line11 = Pick(movePair);
			return Speak((Random.value < 0.5f) ? a : b, line11);
		}
		string line12 = Pick(greetings);
		return Speak((Random.value < 0.5f) ? a : b, line12);
	}
}
