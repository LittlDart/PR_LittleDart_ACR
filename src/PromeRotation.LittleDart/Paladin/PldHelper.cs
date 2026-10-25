using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using LittleDart.Common;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using static PromeRotation.Helpers.JobGaugeHelper;
using PromeRotation.Managers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin;

public class PldHelper
{
	public enum SupportTargetKind
	{
		Partner,
		Healer,
		LowestHp
	}

	private const float MeleeRangeFallback = 3f;

	private const float CastableAllyRange = 30f;

	private const int AoeEnemyCountRange = 5;

	public static bool HasTankStance()
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.HasStatus(me, PldSkills.钢铁信念buff);
		}
		return false;
	}

	public static IBattleChara? GetCurrentTarget()
	{
		return Core.Target;
	}

	public static double GetComboTimeLeftSeconds()
	{
		return ActionHelper.GetComboLeftTime();
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static bool IsComboAlive()
	{
		return GetComboTimeLeftSeconds() > 0.0;
	}

	public static uint GetLastComboSpellId()
	{
		return ActionHelper.GetLastComboID();
	}

	public static bool HasAura(uint auraId, int timeLeft = 0)
	{
		IBattleChara me = Core.Me;
		if (me == null)
		{
			return false;
		}
		if (!BattleCharaExtensions.HasStatus(me, auraId))
		{
			return false;
		}
		if (timeLeft <= 0)
		{
			return true;
		}
		return BattleCharaExtensions.GetStatusLeftTime(me, auraId) * 1000f >= (float)timeLeft;
	}

	public static bool IsRecentlyCast(uint spellId, int ms = 1500)
	{
		return ActionHelper.RecentlyUsed(spellId, ms);
	}

	public static int GetAuraStack(uint auraId)
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.GetStatusStackCount(me, auraId);
		}
		return 0;
	}

	public static bool HasCombo()
	{
		return GetLastComboSpellId() != 0 && IsComboAlive();
	}

	
	public static bool IsAtRoyalAuthorityStep()
	{
		return GetLastComboSpellId() == PldSkills.暴乱剑 && IsComboAlive();
	}

	public static bool HasDivineMight()
	{
		return HasAura(PldSkills.神圣魔法效果提高);
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static bool CanStartConfiteor()
	{
		
		
		return HasAura(PldSkills.悔罪预备);
	}

	public static bool CanUseEmpoweredHolySpirit()
	{
		if (!HasDivineMight())
		{
			return false;
		}
		return IsAtRoyalAuthorityStep();
	}

	public static int GetAtonementChainStep()
	{
		if (HasAura(PldSkills.葬送剑预备))
		{
			return 3;
		}
		if (HasAura(PldSkills.祈告剑预备))
		{
			return 2;
		}
		if (HasAura(PldSkills.赎罪剑预备))
		{
			return 1;
		}
		return 0;
	}

	public static bool IsAtonementChainRunning()
	{
		return GetAtonementChainStep() != 0;
	}

	public static bool HasValidTarget()
	{
		return GetCurrentTarget() != null;
	}

	
	
	
	
	
	
	
	public static float GetMeleeRange()
	{
		float actionRange = default;
		if (ActionHelper.TryGetActionRange(PldSkills.先锋剑, out actionRange) && actionRange > 0f)
		{
			return actionRange;
		}
		return MeleeRangeFallback;
	}

	public static bool IsInMeleeRange()
	{
		if (GetCurrentTarget() == null)
		{
			return false;
		}
		return GetDistanceToTargetCircle() <= GetMeleeRange();
	}

	public static bool IsTargetFar()
	{
		if (!HasValidTarget())
		{
			return false;
		}
		return !IsInMeleeRange();
	}

	public static float GetDistanceToTarget()
	{
		IBattleChara currentTarget = GetCurrentTarget();
		IBattleChara me = Core.Me;
		if (currentTarget == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, currentTarget) - ((IGameObject)me).HitboxRadius - ((IGameObject)currentTarget).HitboxRadius;
	}

	public static float GetDistanceToTargetCircle()
	{
		IBattleChara currentTarget = GetCurrentTarget();
		IBattleChara me = Core.Me;
		if (currentTarget == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, currentTarget) - ((IGameObject)currentTarget).HitboxRadius;
	}

	public static bool PassesRangedHolySpiritGate()
	{
		return PassesRangedGate(PldSettings.Instance.RangedHolySpiritMinDistance);
	}

	public static bool PassesRangedShieldLobGate()
	{
		return PassesRangedGate(PldSettings.Instance.RangedShieldLobMinDistance);
	}

	public static bool IsDailyMode()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance != null && instance.Mode == 1)
		{
			return true;
		}
		return PromeSettings.Instance.GetQt(PldQt.DailyMode);
	}

	private static bool PassesRangedGate(int minDistance)
	{
		if (!HasValidTarget())
		{
			return false;
		}
		return GetDistanceToTarget() >= (float)minDistance;
	}

	public static bool IsMoving()
	{
		return IsMovingForCast();
	}

	public static bool IsMovingForCast()
	{
		return MoveManager.IsLocalPlayerActuallyMoving;
	}

	public static double GetFofCooldownSeconds()
	{
		return ActionHelper.GetActionCooldown(PldSkills.战逃反应);
	}

	public static uint GetBurstStarterSkill()
	{
		if (!ActionHelper.IsUnlocked(PldSkills.绝对统治))
		{
			return PldSkills.安魂祈祷技能;
		}
		return PldSkills.绝对统治;
	}

	public static double GetBurstStarterCooldownSeconds()
	{
		return ActionHelper.GetActionCooldown(GetBurstStarterSkill());
	}

	public static bool IsFofActive()
	{
		return HasAura(PldSkills.战逃反应buff);
	}

	public static bool IsFofRecentCast()
	{
		return ActionHelper.RecentlyUsed(PldSkills.战逃反应, 1500);
	}

	public static bool IsFofInside()
	{
		if (!IsFofActive())
		{
			return IsFofRecentCast();
		}
		return true;
	}

	public static bool IsFofOutside()
	{
		return !IsFofInside();
	}

	public static int GetNearbyEnemyCount(int range)
	{
		int enemyCount = 0;
		foreach (IGameObject item in (IEnumerable<IGameObject>)Svc.Objects)
		{
			IBattleChara enemy = (IBattleChara)(object)((item is IBattleChara) ? item : null);
			if (enemy != null && BattleCharaExtensions.IsEnemy(enemy) && GetDistanceTo(enemy) <= (float)range)
			{
				enemyCount++;
			}
		}
		return enemyCount;
	}

	public static double GetTargetAuraTimeLeft(uint auraId)
	{
		IBattleChara currentTarget = GetCurrentTarget();
		if (currentTarget == null)
		{
			return 0.0;
		}
		return BattleCharaExtensions.GetStatusLeftTime(currentTarget, auraId);
	}

	public static IBattleChara? GetAnotherTank()
	{
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && IsTankJob(item))
			{
				return item;
			}
		}
		return null;
	}

	public static IBattleChara? GetLowestHpPartner(float maxDistance = 30f)
	{
		IBattleChara lowestHpPartner = null;
		float lowestHpPercent = float.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && !(GetDistanceTo(item) > maxDistance))
			{
				float hpPercent = PartyHelper.GetHpPercent(item);
				if (hpPercent < lowestHpPercent)
				{
					lowestHpPercent = hpPercent;
					lowestHpPartner = item;
				}
			}
		}
		return lowestHpPartner;
	}

	public static IBattleChara? GetSupportTarget(SupportTargetKind kind)
	{
		return kind switch
		{
			SupportTargetKind.Partner => GetAnotherTank(), 
			SupportTargetKind.Healer => GetLowestHpHealer(), 
			SupportTargetKind.LowestHp => GetLowestAbsoluteHpPartner(), 
			_ => (IBattleChara?)null, 
		};
	}

	public static IBattleChara? GetLowestAbsoluteHpPartner()
	{
		IBattleChara lowestHpPartner = null;
		uint lowestHp = uint.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && ((ICharacter)item).CurrentHp < lowestHp)
			{
				lowestHp = ((ICharacter)item).CurrentHp;
				lowestHpPartner = item;
			}
		}
		return lowestHpPartner;
	}

	public static IBattleChara? GetLowestHpHealer()
	{
		IBattleChara lowestHpHealer = null;
		uint lowestHp = uint.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && IsHealerJob(item) && !(GetDistanceTo(item) > 30f) && ((ICharacter)item).CurrentHp < lowestHp)
			{
				lowestHp = ((ICharacter)item).CurrentHp;
				lowestHpHealer = item;
			}
		}
		return lowestHpHealer;
	}

	private static bool IsSupportCandidate(IBattleChara? member)
	{
		if (member == null)
		{
			return false;
		}
		IBattleChara me = Core.Me;
		if (me != null && ((IGameObject)member).EntityId == ((IGameObject)me).EntityId)
		{
			return false;
		}
		return ((ICharacter)member).CurrentHp != 0;
	}

	private static bool IsTankJob(IBattleChara member)
	{
		return HealerHelper.IsTank((Job)((ICharacter)member).ClassJob.RowId);
	}

	private static bool IsHealerJob(IBattleChara member)
	{
		return HealerHelper.IsHealer((Job)((ICharacter)member).ClassJob.RowId);
	}

	public static float GetDistanceTo(IBattleChara? target)
	{
		IBattleChara me = Core.Me;
		if (target == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, target) - ((IGameObject)target).HitboxRadius;
	}

	public static float GetActionRange(uint actionId)
	{
		float actionRange = default;
		if (!ActionHelper.TryGetActionRange(actionId, out actionRange))
		{
			return 0f;
		}
		return actionRange;
	}

	public static bool IsReadyWithCanCast(uint actionId)
	{
		if (!AbilityGate.ReadyNow(actionId))
		{
			return false;
		}
		return IsInCastRange(actionId);
	}

	public static bool IsReadyAndInRangeForNextGcd(uint actionId)
	{
		if (AbilityGate.ReadyForNextGcd(actionId))
		{
			return IsInCastRange(actionId);
		}
		return false;
	}

	private static bool IsInCastRange(uint actionId)
	{
		float actionRange = default;
		if (!ActionHelper.TryGetActionRange(actionId, out actionRange) || actionRange <= 0f)
		{
			return IsInMeleeRange();
		}
		return GetDistanceToTargetCircle() <= actionRange;
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	public const int AoeEffectRange = 5;

	
	public static float GetAoeRadius()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return AoeEffectRange;
		}
		return instance.AoeRadius;
	}

	
	
	
	
	
	public static bool IsAoeClusterMet()
	{
		return AoeQueue.ShouldUseAoeIgnoringTarget();
	}

	
	
	
	
	
	
	
	
	
	public static IBattleChara? GetNearestEnemy(float range)
	{
		if (Core.Me == null)
		{
			return null;
		}
		IBattleChara nearest = null;
		float nearestDistance = float.MaxValue;
		foreach (IGameObject obj in Svc.Objects)
		{
			if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
			{
				continue;
			}
			if (!BattleCharaExtensions.IsEnemy(enemy))
			{
				continue;
			}
			float distance = GetDistanceTo(enemy);
			if (distance > range || distance >= nearestDistance)
			{
				continue;
			}
			nearestDistance = distance;
			nearest = enemy;
		}
		return nearest;
	}

	
	private static bool IsSelfCenteredAction(uint actionId)
	{
		ActionTargetType target = default;
		return ActionHelper.TryResolveActionTargetType(actionId, out target) && target == ActionTargetType.Self;
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	private static float GetCastRadius(uint actionId)
	{
		float actionRange = GetActionRange(actionId);
		if (actionRange > 0f)
		{
			return actionRange;
		}
		return IsSelfCenteredAction(actionId) ? AoeEffectRange : GetMeleeRange();
	}

	
	public static bool HasEnemyInCastRange(uint actionId)
	{
		return GetNearestEnemy(GetCastRadius(actionId)) != null;
	}

	
	
	
	
	
	
	
	
	
	
	public static IBattleChara? GetCastTarget(uint actionId)
	{
		if (IsAoeClusterMet())
		{
			return GetNearestEnemy(GetCastRadius(actionId));
		}
		return GetCurrentTarget();
	}

	
	
	
	
	
	
	
	public static bool HasEnemyToFight()
	{
		return IsAoeClusterMet() ? GetNearestEnemy(AoeEffectRange) != null : IsInMeleeRange();
	}

	
	
	
	
	
	
	
	
	
	public static bool HasTargetOrAoeCluster()
	{
		if (GetCurrentTarget() != null)
		{
			return true;
		}
		return IsAoeClusterMet() && GetNearestEnemy(AoeEffectRange) != null;
	}

	
	
	
	
	
	
	
	
	public static bool IsReadyAndInRangeForNextGcdAoe(uint actionId)
	{
		if (!AbilityGate.ReadyForNextGcd(actionId))
		{
			return false;
		}
		return IsAoeClusterMet() ? HasEnemyInCastRange(actionId) : IsInCastRange(actionId);
	}

	
	public static bool IsReadyWithCanCastAoe(uint actionId)
	{
		if (!AbilityGate.ReadyNow(actionId))
		{
			return false;
		}
		return IsAoeClusterMet() ? HasEnemyInCastRange(actionId) : IsInCastRange(actionId);
	}

	public static void SetTankStance(bool on)
	{
		ActionQueueManager.Enqueue(PldAction.Self(on ? PldSkills.钢铁信念 : PldSkills.解除钢铁信念, ActionType.OffGcd), false);
	}

	public static void ClearHighPriorityQueues()
	{
		ActionQueueManager.ClearAllQueues();
		Log("已清扫技能队列（PR：ClearAllQueues 会连强制/普通队列一起清）");
		Toast("已清扫技能队列", 1200);
	}

	public static void Log(string message)
	{
		LogTagged("[LittleDart-PLD] " + message);
	}

	public static void LogTagged(string taggedMessage)
	{
		PluginLog.Information(taggedMessage);
	}

	
	
	
	
	
	public static void Toast(string message, int time = 2000, int style = 1)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance != null && !instance.UseToast)
		{
			return;
		}
		float seconds = Math.Clamp(time / 1000f, 1f, 10f);
		try
		{
			HintHelper.ShowToast2(message, seconds, (style == 2) ? HintHelper.HintType.Warning : HintHelper.HintType.Info);
		}
		catch (Exception exception)
		{
			Log("Toast 失败（不影响主流程）：" + exception.Message);
		}
		Log("[提示] " + message);
	}

	public static void ToastModeBanner(bool daily)
	{
		Toast("当前模式：" + (daily ? "日常" : "高难"), 1000, daily ? 1 : 2);
	}

	public static int GetOath()
	{
		try
		{
			return PLD.Oath;
		}
		catch
		{
			return 0;
		}
	}

	private static float PlanarDistance(IBattleChara a, IBattleChara b)
	{
		Vector3 position = ((IGameObject)a).Position;
		Vector3 position2 = ((IGameObject)b).Position;
		float deltaX = position.X - position2.X;
		float deltaZ = position.Z - position2.Z;
		return MathF.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
	}
}
