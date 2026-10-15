using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using PromeRotation.Data;

namespace LittleDart.Common.Mitigation.Calc;

public sealed class ThreatContext
{
		public const float PackRadius = 15f;

		public const float CastScanRadius = 30f;

		public const int PackThreshold = 3;

		public const int AoeMitigationLockFuseMs = 8000;

	private readonly MitigationJobProfile _p;

		private (ulong SourceId, uint ActionId)? _teamMitigationLock;

		private long _teamMitigationLockAtMs;

	public ThreatContext(MitigationJobProfile profile)
	{
		_p = profile;
	}

		public Criteria CurrentCriteria
	{
		get
		{
			bool? inBossRoom = ReadInBossRoom();
			if (!inBossRoom.HasValue)
			{
				return Criteria.Pack;
			}
			return inBossRoom.Value ? Criteria.BossRoom : Criteria.Pack;
		}
	}

	public enum Criteria
	{
				Pack = 0,

				BossRoom = 1,
	}

	public static string Describe(Criteria criteria)
	{
		return criteria == Criteria.BossRoom
			? "boss 房·读条判据"
			: "非 boss 房·群怪判据";
	}

		public bool IsCastingDeathSentence()
	{
		return AnyEnemyCastingAction(PromeBattleData.TankDeathActions, remainingMs: 0);
	}

		public bool IsCastingAoe(int warnMs)
	{
		return FindIncomingAoe(warnMs).HasValue;
	}

		public bool CanUseTeamMitigation()
	{
		return !_teamMitigationLock.HasValue;
	}

		public void TickAoeLock(int warnMs)
	{
		if (!_teamMitigationLock.HasValue)
		{
			return;
		}

		if (Environment.TickCount64 - _teamMitigationLockAtMs >= AoeMitigationLockFuseMs)
		{
			ReleaseTeamMitigationLock("保险丝到点");
			return;
		}
		(ulong s, uint a)? incoming = FindIncomingAoe(warnMs);
		if (!incoming.HasValue || incoming.Value.s != _teamMitigationLock.Value.SourceId
			|| incoming.Value.a != _teamMitigationLock.Value.ActionId)
		{
			ReleaseTeamMitigationLock("这次 AOE 读条已结束");
		}
	}

		public void OnTeamMitigationCast(uint skillId)
	{
		(ulong s, uint a)? incoming = FindIncomingAoe(_p.Thresholds.AoeWarnMs);
		if (!incoming.HasValue)
		{

			_teamMitigationLock = (0UL, skillId);
		}
		else
		{
			_teamMitigationLock = (incoming.Value.s, incoming.Value.a);
		}
		_teamMitigationLockAtMs = Environment.TickCount64;
	}

	public void ResetAoeLock()
	{
		_teamMitigationLock = null;
		_teamMitigationLockAtMs = 0L;
	}

	public string DescribeAoeLock()
	{
		if (!_teamMitigationLock.HasValue)
		{
			return "未上锁";
		}
		long ageMs = Environment.TickCount64 - _teamMitigationLockAtMs;
		return $"已上锁（来源={_teamMitigationLock.Value.SourceId} 技能={_teamMitigationLock.Value.ActionId} 已 {ageMs}ms）";
	}

	private void ReleaseTeamMitigationLock(string reason)
	{
		_teamMitigationLock = null;
		_teamMitigationLockAtMs = 0L;
		if (_p.DiagnosticsEnabled())
		{
			_p.Log($"{_p.LogPrefix}/AOE锁 释放：{reason}");
		}
	}

		private (ulong SourceId, uint ActionId)? FindIncomingAoe(int warnMs)
	{
		IReadOnlySet<uint> actionIds = PromeBattleData.AoeActions;
		if (actionIds == null || actionIds.Count == 0)
		{
			return null;
		}
		IBattleChara? me = _p.Me();
		if (me == null)
		{
			return null;
		}
		(ulong SourceId, uint ActionId)? best = null;
		float bestRemainMs = float.MaxValue;
		try
		{
			foreach (IGameObject obj in Svc.Objects)
			{
				if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
				{
					continue;
				}
				if (obj.ObjectKind != ObjectKind.BattleNpc || !_p.IsEnemy(enemy))
				{
					continue;
				}
				if (_p.DistanceTo(enemy) > CastScanRadius)
				{
					continue;
				}
				if (!enemy.IsCasting || enemy.CastActionId == 0 || !actionIds.Contains(enemy.CastActionId))
				{
					continue;
				}
				float remainMs = Math.Max(0f, enemy.TotalCastTime - enemy.CurrentCastTime) * 1000f;
				if (warnMs > 0 && remainMs > warnMs)
				{
					continue;
				}
				if (remainMs < bestRemainMs)
				{
					bestRemainMs = remainMs;
					best = (obj.EntityId, enemy.CastActionId);
				}
			}
		}
		catch
		{
			return null;
		}
		return best;
	}

		public int PackEnemyCount()
	{
		return _p.NearbyEnemyCount(PackRadius);
	}

	public bool IsPack()
	{
		return PackEnemyCount() >= PackThreshold;
	}

		public bool IsPackCastingAoe(int warnMs)
	{
		return AnyEnemyCastingAction(PromeBattleData.AoeActions, warnMs);
	}

		private bool AnyEnemyCastingAction(IReadOnlySet<uint> actionIds, int remainingMs)
	{
		if (actionIds == null || actionIds.Count == 0)
		{
			return false;
		}
		IBattleChara? me = _p.Me();
		if (me == null)
		{
			return false;
		}
		try
		{
			foreach (IGameObject obj in Svc.Objects)
			{
				if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
				{
					continue;
				}

				if (obj.ObjectKind != ObjectKind.BattleNpc)
				{
					continue;
				}
				if (!_p.IsEnemy(enemy))
				{
					continue;
				}
				if (_p.DistanceTo(enemy) > CastScanRadius)
				{
					continue;
				}
				if (!enemy.IsCasting || enemy.CastActionId == 0)
				{
					continue;
				}
				if (!actionIds.Contains(enemy.CastActionId))
				{
					continue;
				}
				if (remainingMs > 0)
				{
					float remainMs = Math.Max(0f, enemy.TotalCastTime - enemy.CurrentCastTime) * 1000f;
					if (remainMs > remainingMs)
					{
						continue;
					}
				}
				return true;
			}
		}
		catch
		{
			return false;
		}
		return false;
	}

	private bool? ReadInBossRoom()
	{
		try
		{
			return _p.InBossRoom();
		}
		catch
		{
			return null;
		}
	}
}
