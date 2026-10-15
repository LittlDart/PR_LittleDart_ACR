using System;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

public abstract class GnbSupportHotkeyBase : IHotkeyLogic
{
		protected abstract uint SkillId { get; }

		protected abstract string SkillName { get; }

		protected abstract GnbHelper.SupportTargetKind TargetKind { get; }

		protected abstract string IconFile { get; }

		protected abstract float ConfiguredRange { get; }

		protected virtual bool IsGcd => false;

	internal uint IconActionId => SkillId;

	internal string? CustomIconPath => GnbHotkeyIcon.Resolve(IconFile);

		protected float EffectiveRange
	{
		get
		{
			float actionRange = GnbHelper.GetActionRange(SkillId);
			if (!(actionRange <= 0f))
			{
				return MathF.Min(ConfiguredRange, actionRange);
			}
			return ConfiguredRange;
		}
	}

		private string TargetKindName => TargetKind switch
	{
		GnbHelper.SupportTargetKind.Partner => "搭档坦克",
		GnbHelper.SupportTargetKind.Healer => "治疗",
		GnbHelper.SupportTargetKind.LowestHp => "血量最低的队友",
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
		return true;
	}

	public bool IsActive()
	{
		if (!ActionHelper.IsUnlocked(SkillId) || !IsSkillReady() || !HasResource())
		{
			return false;
		}
		IBattleChara? supportTarget = GnbHelper.GetSupportTarget(TargetKind);
		if (supportTarget != null)
		{
			return GnbHelper.GetDistanceTo(supportTarget) <= EffectiveRange;
		}
		return false;
	}

	public void OnClick()
	{
		if (!ActionHelper.IsUnlocked(SkillId))
		{
			GnbFeedback.Fail($"{SkillName} 失败：技能尚未解锁（技能 ID {SkillId}）");
			return;
		}
		if (!IsSkillReady())
		{
			GnbFeedback.Fail(SkillName + " 失败：技能未就绪");
			return;
		}
		if (!HasResource())
		{
			GnbFeedback.Fail(SkillName + " 失败：资源不足");
			return;
		}
		IBattleChara? supportTarget = GnbHelper.GetSupportTarget(TargetKind);
		if (supportTarget == null)
		{
			GnbFeedback.Fail(SkillName + " 失败：找不到合适的目标（" + TargetKindName + "）");
			return;
		}
		float distanceTo = GnbHelper.GetDistanceTo(supportTarget);
		if (distanceTo > EffectiveRange)
		{
			GnbFeedback.Fail($"{SkillName} 失败：{supportTarget.Name} 距离 {distanceTo:F1}m 超出 {EffectiveRange:F0}m");
			return;
		}
		HotkeyQueueManager.TryEnqueue(GnbHotkeys.OnTarget(SkillId, IsGcd ? ActionType.Gcd : ActionType.OffGcd, supportTarget));
		GnbFeedback.Info($"{SkillName} → {supportTarget.Name}（{distanceTo:F1}m）");
	}
}
