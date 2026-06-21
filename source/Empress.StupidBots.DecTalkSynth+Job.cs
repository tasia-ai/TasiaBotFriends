using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Empress.StupidBots;

internal static class DecTalkSynth : Object
{
	internal struct Clip : ValueType
	{
		public float[] Samples;

		public int Channels;

		public int SampleRate;
	}

	private struct Job : ValueType
	{
		public BotChatterAgent Agent;

		public string Text;
	}

	private struct Ready : ValueType
	{
		public BotChatterAgent Agent;

		public Clip Clip;
	}

	private static class Wav : Object
	{
		internal static bool TryLoad(string path, out float[] data, out int channels, out int sampleRate)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			data = Array.Empty<float>();
			channels = 0;
			sampleRate = 0;
			FileStream val = File.OpenRead(path);
			try
			{
				BinaryReader val2 = new BinaryReader((Stream)(object)val);
				try
				{
					if ((string)new String(val2.ReadChars(4)) != "RIFF")
					{
						return false;
					}
					val2.ReadInt32();
					if ((string)new String(val2.ReadChars(4)) != "WAVE")
					{
						return false;
					}
					short num = 1;
					short num2 = 16;
					bool flag = false;
					while (val2.BaseStream.Position < val2.BaseStream.Length)
					{
						string text = (string)new String(val2.ReadChars(4));
						int num3 = val2.ReadInt32();
						if (text == "fmt ")
						{
							flag = true;
							num = val2.ReadInt16();
							channels = val2.ReadInt16();
							sampleRate = val2.ReadInt32();
							val2.ReadInt32();
							val2.ReadInt16();
							num2 = val2.ReadInt16();
							int num4 = num3 - 16;
							if (num4 > 0)
							{
								val2.ReadBytes(num4);
							}
							continue;
						}
						if (text == "data")
						{
							if (!flag)
							{
								return false;
							}
							byte[] array = val2.ReadBytes(num3);
							if (num != 1)
							{
								return false;
							}
							switch (num2)
							{
							case 16:
							{
								int num6 = array.Length / 2;
								float[] array3 = (float[])(object)new Single[num6];
								int num7 = 0;
								int num8 = 0;
								while (num7 < num6)
								{
									short num9 = (short)(array[num8] | (array[num8 + 1] << 8));
									array3[num7] = (float)num9 / 32768f;
									num7++;
									num8 += 2;
								}
								data = array3;
								return true;
							}
							case 8:
							{
								int num5 = array.Length;
								float[] array2 = (float[])(object)new Single[num5];
								for (int i = 0; i < num5; i++)
								{
									array2[i] = (float)(array[i] - 128) / 128f;
								}
								data = array2;
								return true;
							}
							default:
								return false;
							}
						}
						val2.ReadBytes(num3);
					}
					return false;
				}
				finally
				{
					if (val2 != null)
					{
						((IDisposable)val2).Dispose();
					}
				}
			}
			finally
			{
				if (val != null)
				{
					((IDisposable)val).Dispose();
				}
			}
		}

		internal static float[] ResampleLinear(float[] interleaved, int channels, int srcRate, int dstRate)
		{
			if (srcRate == dstRate)
			{
				return interleaved;
			}
			int num = interleaved.Length / channels;
			double num2 = (double)dstRate / (double)srcRate;
			int num3 = Mathf.Max(1, (int)Math.Round((double)num * num2));
			float[] array = (float[])(object)new Single[num3 * channels];
			for (int i = 0; i < channels; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					double num4 = (double)j / num2;
					int num5 = (int)Math.Floor(num4);
					int num6 = Math.Min(num - 1, num5 + 1);
					double num7 = num4 - (double)num5;
					float num8 = interleaved[num5 * channels + i];
					float num9 = interleaved[num6 * channels + i];
					array[j * channels + i] = (float)((double)num8 + (double)(num9 - num8) * num7);
				}
			}
			return array;
		}
	}

	[CompilerGenerated]
	private static class <>O : Object
	{
		public static ThreadStart <0>__WorkerLoop;
	}

	private static string _exe;

	private static string _voice;

	private static int _rate;

	private static string _dict;

	private static readonly ConcurrentQueue<Job> _jobs = new ConcurrentQueue<Job>();

	private static readonly ConcurrentQueue<Ready> _ready = new ConcurrentQueue<Ready>();

	private static readonly Dictionary<string, Clip> _cache = new Dictionary<string, Clip>(64);

	private static readonly LinkedList<string> _lru = new LinkedList<string>();

	private static Thread _worker;

	private static volatile bool _run;

	internal static void Configure(string exe, string voice, int sampleRate, string dictionaryPath)
	{
		_exe = ResolvePath(exe);
		_voice = (String.IsNullOrWhiteSpace(voice) ? "Paul" : voice.Trim());
		_rate = Mathf.Clamp(sampleRate, 8000, 48000);
		_dict = (String.IsNullOrWhiteSpace(dictionaryPath) ? "" : ResolvePath(dictionaryPath.Trim()));
		StartWorker();
	}

	internal static void Enqueue(BotChatterAgent who, string text)
	{
		if (!((Object)(object)who == (Object)null) && !String.IsNullOrWhiteSpace(text))
		{
			_jobs.Enqueue(new Job
			{
				Agent = who,
				Text = text
			});
			StartWorker();
		}
	}

	internal static bool TryDequeueReady(out BotChatterAgent who, out Clip clip)
	{
		who = null;
		clip = default(Clip);
		Ready ready = default(Ready);
		if (_ready.TryDequeue(ref ready))
		{
			who = ready.Agent;
			clip = ready.Clip;
			return true;
		}
		return false;
	}

	internal static bool TrySynthesize(string text, out Clip clip)
	{
		clip = default(Clip);
		if (!TrySynthesizeInternal(text, out clip))
		{
			return false;
		}
		return true;
	}

	private static void StartWorker()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (_worker == null || !_worker.IsAlive)
		{
			_run = true;
			object obj = <>O.<0>__WorkerLoop;
			if (obj == null)
			{
				ThreadStart val = WorkerLoop;
				<>O.<0>__WorkerLoop = val;
				obj = (object)val;
			}
			_worker = new Thread((ThreadStart)obj)
			{
				IsBackground = true,
				Name = "DecTalkSynthWorker"
			};
			_worker.Start();
		}
	}

	private static void WorkerLoop()
	{
		Job job = default(Job);
		while (_run)
		{
			Clip clip;
			if (!_jobs.TryDequeue(ref job))
			{
				Thread.Sleep(1);
			}
			else if (!((Object)(object)job.Agent == (Object)null) && TrySynthesizeInternal(job.Text, out clip))
			{
				_ready.Enqueue(new Ready
				{
					Agent = job.Agent,
					Clip = clip
				});
			}
		}
	}

	private static bool TrySynthesizeInternal(string text, out Clip clip)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		clip = default(Clip);
		if (String.IsNullOrEmpty(_exe) || !File.Exists(_exe))
		{
			return false;
		}
		if (TryGetCache(text, out clip))
		{
			return true;
		}
		try
		{
			string text2 = String.Concat("[:name ", _voice, "] ", text);
			string text3 = Path.GetDirectoryName(_exe) ?? Directory.GetCurrentDirectory();
			string text4 = Path.Combine(text3, "out");
			Directory.CreateDirectory(text4);
			DateTime utcNow = DateTime.UtcNow;
			long ticks = ((DateTime)(ref utcNow)).Ticks;
			string text5 = ((Int64)(ref ticks)).ToString("x");
			string text6 = Path.Combine(text4, String.Concat("dt_", text5, ".wav"));
			string text7 = "";
			if (!String.IsNullOrEmpty(_dict) && File.Exists(_dict))
			{
				text7 = String.Concat("-d \"", _dict, "\" ");
			}
			ProcessStartInfo val = new ProcessStartInfo();
			val.FileName = _exe;
			val.Arguments = String.Concat((string[])(object)new String[7] { "-w \"", text6, "\" ", text7, "\"", text2, "\"" });
			val.WorkingDirectory = text3;
			val.CreateNoWindow = true;
			val.UseShellExecute = false;
			val.RedirectStandardOutput = true;
			val.RedirectStandardError = true;
			Process val2 = Process.Start(val);
			try
			{
				if (val2 != null)
				{
					val2.WaitForExit(15000);
				}
			}
			finally
			{
				if (val2 != null)
				{
					((IDisposable)val2).Dispose();
				}
			}
			if (!File.Exists(text6))
			{
				return false;
			}
			if (!Wav.TryLoad(text6, out float[] data, out int channels, out int sampleRate))
			{
				return false;
			}
			if (sampleRate != _rate)
			{
				data = Wav.ResampleLinear(data, channels, sampleRate, _rate);
				sampleRate = _rate;
			}
			clip = new Clip
			{
				Samples = data,
				Channels = channels,
				SampleRate = sampleRate
			};
			try
			{
				File.Delete(text6);
			}
			catch (Object)
			{
			}
			PutCache(text, clip);
			return true;
		}
		catch (Object)
		{
			return false;
		}
	}

	private static bool TryGetCache(string text, out Clip clip)
	{
		if (_cache.TryGetValue(text, ref clip))
		{
			LinkedListNode<string> val = _lru.Find(text);
			if (val != null)
			{
				_lru.Remove(val);
				_lru.AddFirst(val);
			}
			return true;
		}
		return false;
	}

	private static void PutCache(string text, Clip clip)
	{
		if (_cache.ContainsKey(text))
		{
			return;
		}
		_cache[text] = clip;
		_lru.AddFirst(text);
		if (_lru.Count > 64)
		{
			LinkedListNode<string> last = _lru.Last;
			if (last != null)
			{
				_cache.Remove(last.Value);
				_lru.RemoveLast();
			}
		}
	}

	private static string ResolvePath(string p)
	{
		if (Path.IsPathRooted(p))
		{
			return p;
		}
		string location = Assembly.GetExecutingAssembly().Location;
		return Path.GetFullPath(Path.Combine(String.IsNullOrEmpty(location) ? Directory.GetCurrentDirectory() : (Path.GetDirectoryName(location) ?? Directory.GetCurrentDirectory()), p));
	}
}
