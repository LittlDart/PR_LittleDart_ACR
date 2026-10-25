using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationJobProfile
{
	public string JobName = string.Empty;

	public string LogPrefix = "[LittleDart/Mit]";

	public string CommandHint = string.Empty;

	public IReadOnlyList<MitigationSkillDef> Registry = Array.Empty<MitigationSkillDef>();

	public IReadOnlyList<LevelSkillRow> LevelTable = Array.Empty<LevelSkillRow>();

	public uint ReprisalSkillId;

	public MitigationThresholds Thresholds = new MitigationThresholds();

	public Action RefreshThresholds = () =>
	{
	};

	public Func<bool> CanRun = () => true;

	public Func<bool> IsDailyMode = () => false;

	public Func<bool> AutoMitigationQt = () => false;

	public Func<bool> StopAttackQt = () => false;

	public Func<bool> DailyModeQt = () => false;

	public Func<IBattleChara?> Me = () => (IBattleChara?)null;

	public Func<IBattleChara?> CurrentTarget = () => (IBattleChara?)null;

	public Func<bool> HasValidTarget = () => false;

	public Func<bool> HasTankStance = () => true;

	public Func<uint, bool> HasSelfAura = (uint _) => false;

	public Func<uint, double> TargetAuraTimeLeft = (uint _) => 0.0;

	public Func<uint, int, bool> IsRecentlyCast = (uint _, int _) => false;

	public Func<MitigationSkillDef, bool> IsReadyWithCanCast = (MitigationSkillDef _) => false;

	public Func<MitigationSkillDef, bool> HasResourceFor = (MitigationSkillDef _) => true;

	public Func<IBattleChara, bool> IsEnemy = (IBattleChara _) => false;

	public Func<float, int> NearbyEnemyCount = (float _) => 0;

	public Func<IBattleChara, float> DistanceTo = (IBattleChara _) => float.MaxValue;

	public Func<float> DistanceToTarget = () => float.MaxValue;

	public Func<bool> DiagnosticsEnabled = () => false;

	public Func<bool?> IsLocalPlayerMoving = () => (bool?)null;

	public Func<bool?> InBossRoom = () => (bool?)null;

	public Func<double, bool> TargetSurvivesLongerThan = (double _) => true;

	public Action<string> Log = (string _) =>
	{
	};

	public Func<MitigationSkillDef, PAction> BuildAction = (MitigationSkillDef def) =>
	{
		return new PAction(0u, (ActionType)1, (ActionTargetType)0);
	};

	public IReadOnlyList<MitigationSelfCareRule> SelfCareRules = Array.Empty<MitigationSelfCareRule>();

	public MitigationSkillDef? FindSkill(uint skillId)
	{
		foreach (MitigationSkillDef item in Registry)
		{
			if (item.SkillId == skillId)
			{
				return item;
			}
		}
		return null;
	}

	public List<MitigationSkillDef> PoolForLevel(int level)
	{
		return MitigationPool.Build(Registry, LevelTable, level);
	}

}
