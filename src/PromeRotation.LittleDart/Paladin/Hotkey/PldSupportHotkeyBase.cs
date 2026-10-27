using System;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Paladin.Hotkey;

public abstract class PldSupportHotkeyBase : IHotkeyLogic
{
	protected abstract uint SkillId { get; }

	protected abstract string SkillName { get; }

	protected abstract PldHelper.SupportTargetKind TargetKind { get; }

	protected abstract string IconFile { get; }

	protected abstract float ConfiguredRange { get; }

	protected virtual bool IsGcd => false;

	internal uint IconActionId => SkillId;

	internal string? CustomIconPath => PldHotkeyIcon.Resolve(IconFile);

	protected float EffectiveRange
	{
		get
		{
			float actionRange = PldHelper.GetActionRange(SkillId);
			if (!(actionRange <= 0f))
			{
				return MathF.Min(ConfiguredRange, actionRange);
			}
			return ConfiguredRange;
		}
	}

	private bool HasOathResource => !IsGcd;

	private string TargetKindName => TargetKind switch
	{
		PldHelper.SupportTargetKind.Partner => "搭档坦克", 
		PldHelper.SupportTargetKind.Healer => "治疗", 
		PldHelper.SupportTargetKind.LowestHp => "血量最低的队友", 
		_ => TargetKind.ToString(), 
	};

	protected virtual bool IsSkillReady()
	{
		if (!ActionHelper.IsReady(SkillId))
		{
			return ActionHelper.GetActionCharges(SkillId) >= 1f;
		}
		return true;
	}

	protected virtual bool HasResource()
	{
		return PldHelper.GetOath() >= 50;
	}

	public bool IsActive()
	{
		if (!ActionHelper.IsUnlocked(SkillId) || !IsSkillReady() || !HasResource())
		{
			return false;
		}
		IBattleChara supportTarget = PldHelper.GetSupportTarget(TargetKind);
		if (supportTarget != null)
		{
			return PldHelper.GetDistanceTo(supportTarget) <= EffectiveRange;
		}
		return false;
	}

	public void OnClick()
	{
		if (!ActionHelper.IsUnlocked(SkillId))
		{
			PldFeedback.Fail($"{SkillName} 失败：技能尚未解锁（技能 ID {SkillId}）");
			return;
		}
		if (!IsSkillReady())
		{
			PldFeedback.Fail(SkillName + " 失败：技能未就绪");
			return;
		}
		if (!HasResource())
		{
			PldFeedback.Fail(HasOathResource ? $"{SkillName} 失败：忠义值不足（{PldHelper.GetOath()}/50）" : $"{SkillName} 失败：蓝量不足（低于 {PldSettings.Instance?.ClemencyMinMp ?? 0}）");
			return;
		}
		IBattleChara supportTarget = PldHelper.GetSupportTarget(TargetKind);
		if (supportTarget == null)
		{
			PldFeedback.Fail(SkillName + " 失败：找不到合适的目标（" + TargetKindName + "）");
			return;
		}
		float distanceTo = PldHelper.GetDistanceTo(supportTarget);
		if (distanceTo > EffectiveRange)
		{
			PldFeedback.Fail($"{SkillName} 失败：{((IGameObject)supportTarget).Name} 距离 {distanceTo:F1}m 超出 {EffectiveRange:F0}m");
		}
		else
		{
			HotkeyQueueManager.TryEnqueue(PldHotkeys.OnTarget(SkillId, IsGcd ? ActionType.Gcd : ActionType.OffGcd, supportTarget));
			PldFeedback.Info($"{SkillName} → {((IGameObject)supportTarget).Name}（{distanceTo:F1}m）");
		}
	}
}
