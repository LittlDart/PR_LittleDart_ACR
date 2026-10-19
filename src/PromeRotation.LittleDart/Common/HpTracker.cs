using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;

namespace LittleDart.Common;

public sealed class HpTracker
{
	private readonly Func<IBattleChara?> _targetProvider;

	private readonly List<(long T, float Hp)> _self = new List<(long, float)>();

	private readonly List<(long T, float Hp)> _target = new List<(long, float)>();

	private long _lastSelfSampleMs;

	private long _lastTargetSampleMs;

	private uint _targetId;

	private long _lastSeenTargetMs;

	public bool HasTargetHistory => _target.Count >= 2;

	public HpTracker(Func<IBattleChara?>? targetProvider = null)
	{
		_targetProvider = targetProvider ?? ((Func<IBattleChara>)(() => (IBattleChara?)null));
	}

	public void Reset()
	{
		_self.Clear();
		_target.Clear();
		_lastSelfSampleMs = 0L;
		_lastTargetSampleMs = 0L;
		_targetId = 0u;
		_lastSeenTargetMs = 0L;
	}

	public void Tick(long now, int sampleIntervalMs, int keepWindowMs)
	{
		if (now - _lastSelfSampleMs >= sampleIntervalMs)
		{
			_lastSelfSampleMs = now;
			float selfHpPercent = HpPercentOf(Core.Me);
			if (!float.IsNaN(selfHpPercent))
			{
				_self.Add((now, selfHpPercent));
				Prune(_self, now - (long)keepWindowMs * 3L);
			}
		}
		IBattleChara target = _targetProvider();
		if (target == null)
		{
			if (_target.Count > 0 && now - _lastSeenTargetMs > 2000)
			{
				_target.Clear();
				_targetId = 0u;
			}
			return;
		}
		_lastSeenTargetMs = now;
		if (((IGameObject)target).EntityId != _targetId)
		{
			_target.Clear();
			_targetId = ((IGameObject)target).EntityId;
		}
		if (now - _lastTargetSampleMs >= sampleIntervalMs)
		{
			_lastTargetSampleMs = now;
			float targetHpPercent = HpPercentOf(target);
			if (!float.IsNaN(targetHpPercent))
			{
				_target.Add((now, targetHpPercent));
				Prune(_target, now - (long)keepWindowMs * 3L);
			}
		}
	}

	public float SelfDamageTakenPercent(int windowMs)
	{
		return CumulativeDropPercent(_self, windowMs);
	}

	public float SelfNetDeclinePerSecond(int windowMs)
	{
		return NetDeclinePerSecond(_self, windowMs);
	}

	public float TargetNetDeclinePerSecond(int windowMs)
	{
		return NetDeclinePerSecond(_target, windowMs);
	}

	public float TargetHpPercent()
	{
		IBattleChara target = _targetProvider();
		if (target != null)
		{
			return HpPercentOf(target);
		}
		return -1f;
	}

	public bool SurvivesLongerThan(double seconds, int windowMs)
	{
		if (!TargetDeathPredictor.TryPredictSecondsToDeath(this, windowMs, out var predictedSeconds))
		{
			return true;
		}
		return predictedSeconds > seconds;
	}

	private static float HpPercentOf(IBattleChara? chara)
	{
		if (chara == null)
		{
			return float.NaN;
		}
		try
		{
			if (((ICharacter)chara).MaxHp == 0)
			{
				return float.NaN;
			}
			return (float)((ICharacter)chara).CurrentHp / (float)((ICharacter)chara).MaxHp;
		}
		catch
		{
			return float.NaN;
		}
	}

	private static void Prune(List<(long T, float Hp)> list, long cutoff)
	{
		while (list.Count > 0 && list[0].T < cutoff)
		{
			list.RemoveAt(0);
		}
	}

	private static float CumulativeDropPercent(List<(long T, float Hp)> list, int windowMs)
	{
		long windowStartMs = Environment.TickCount64 - windowMs;
		float totalDropPercent = 0f;
		for (int i = 1; i < list.Count; i++)
		{
			(long, float) previous = list[i - 1];
			(long, float) current = list[i];
			if (current.Item1 >= windowStartMs && previous.Item1 >= windowStartMs - 500)
			{
				float dropPercent = previous.Item2 - current.Item2;
				if (dropPercent > 0f)
				{
					totalDropPercent += dropPercent;
				}
			}
		}
		return totalDropPercent * 100f;
	}

	private static float NetDeclinePerSecond(List<(long T, float Hp)> list, int windowMs)
	{
		if (list.Count < 2)
		{
			return 0f;
		}
		long windowStartMs = Environment.TickCount64 - windowMs;
		(long, float) oldestKept = list[0];
		foreach (var item in list)
		{
			if (item.T <= windowStartMs)
			{
				oldestKept = item;
				continue;
			}
			break;
		}
		(long, float) latest = list[list.Count - 1];
		double elapsedSeconds = (double)(latest.Item1 - oldestKept.Item1) / 1000.0;
		if (elapsedSeconds < 0.5)
		{
			return 0f;
		}
		float dropPercent = (oldestKept.Item2 - latest.Item2) * 100f;
		if (dropPercent <= 0f)
		{
			return 0f;
		}
		return (float)((double)dropPercent / elapsedSeconds);
	}
}
