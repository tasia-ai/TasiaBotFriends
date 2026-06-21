using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace Empress.StupidBots;

[BepInPlugin("Empress.BotFriends.Chatter", "EmpressBotFriendsChatter", "1.0.0")]
public class EmpressBotFriendsChatterPlugin : BaseUnityPlugin
{
	internal static EmpressBotFriendsChatterPlugin Instance;

	internal static ConfigEntry<bool> Enabled;

	internal static ConfigEntry<float> TalkRadius;

	internal static ConfigEntry<float> PairCooldown;

	internal static ConfigEntry<float> SelfCooldown;

	internal static ConfigEntry<int> MaxConcurrent;

	internal static ConfigEntry<float> Volume;

	internal static ConfigEntry<bool> UseDECtalk;

	internal static ConfigEntry<string> ExePath;

	internal static ConfigEntry<string> Voice;

	internal static ConfigEntry<int> SampleRate;

	internal static ConfigEntry<string> DictionaryPath;

	internal static ManualLogSource Logger => Instance._logger;

	private ManualLogSource _logger => ((BaseUnityPlugin)this).Logger;

	private void Awake()
	{
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		Instance = this;
		Enabled = ((BaseUnityPlugin)this).Config.Bind<bool>("Chatter", "Enabled", false, "on");
		TalkRadius = ((BaseUnityPlugin)this).Config.Bind<float>("Chatter", "TalkRadius", 1f, "m");
		PairCooldown = ((BaseUnityPlugin)this).Config.Bind<float>("Chatter", "PairCooldown", 10f, "s");
		SelfCooldown = ((BaseUnityPlugin)this).Config.Bind<float>("Chatter", "SelfCooldown", 6f, "s");
		MaxConcurrent = ((BaseUnityPlugin)this).Config.Bind<int>("Chatter", "MaxConcurrent", 1, "voices");
		Volume = ((BaseUnityPlugin)this).Config.Bind<float>("Chatter", "Volume", 0.6f, "0..1");
		UseDECtalk = ((BaseUnityPlugin)this).Config.Bind<bool>("DECtalk", "UseDECtalk", true, "on");
		ExePath = ((BaseUnityPlugin)this).Config.Bind<string>("DECtalk", "ExePath", "say.exe", "path");
		Voice = ((BaseUnityPlugin)this).Config.Bind<string>("DECtalk", "Voice", "Paul", "name");
		SampleRate = ((BaseUnityPlugin)this).Config.Bind<int>("DECtalk", "SampleRate", 22050, "Hz");
		DictionaryPath = ((BaseUnityPlugin)this).Config.Bind<string>("DECtalk", "DictionaryPath", "dtalk_us.dic", "path or blank");
		DecTalkSynth.Configure(ExePath.Value, Voice.Value, SampleRate.Value, DictionaryPath.Value);
		new GameObject("BotChatterManager")
		{
			hideFlags = (HideFlags)61
		}.AddComponent<BotChatterManager>();
		Logger.LogInfo((object)"Empress chatter ready");
	}
}
