using System;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 绝枪「支援面板」（2 列 × 3 行 = 六宫格）里每个按钮的公共骨架 ——
/// 与骑士 <c>PldSupportHotkeyBase</c> 逐格同构，只换目标选择器与资源闸门。
///
/// <para><b>面板是什么</b>：骑士那边叫「九宫格支援」（干预 / 深仁厚泽 / 保护 × 搭档 / 奶妈 / 血量最低）。
/// 绝枪只有两个技能能给队友 —— **刚玉之心**（25758，减伤）与**极光**（16151，自回血）——
/// 所以是 **2 技能 × 3 类目标 = 六宫格**，两列三行、刚玉在前极光在后。</para>
///
/// <para><b>目标三档</b>（与骑士同名同义，见 <see cref="GnbHelper.SupportTargetKind"/>）：
/// 搭档坦克 / 治疗 / 血量最低的队友。</para>
///
/// <para><b>为什么射程要取 <c>min(配置值, 技能实际射程)</c></b>：配置值是玩家口径的上限，
/// 技能自己的射程是硬约束 —— 两者取小，避免「按配置判 30m 通过、真发出去才发现超射程」。
/// 与骑士 <c>EffectiveRange</c> 同一写法。</para>
///
/// <para><see cref="IsActive"/> 只用来给按钮**上色 / 置灰**（PR 据此画激活态），
/// 不影响能不能点 —— 真正的出击条件在 <see cref="OnClick"/> 里**重判一遍**。</para>
/// </summary>
public abstract class GnbSupportHotkeyBase : IHotkeyLogic
{
	/// <summary>要发的技能 ID（一律走 <see cref="Skill.GnbSkills"/> 常量）。</summary>
	protected abstract uint SkillId { get; }

	/// <summary>按钮 / 报错文案里显示的技能名。</summary>
	protected abstract string SkillName { get; }

	/// <summary>目标档位（搭档 / 奶妈 / 血量最低）。</summary>
	protected abstract GnbHelper.SupportTargetKind TargetKind { get; }

	/// <summary>自定义图标文件名（<c>Resources\</c> 下，见 <see cref="GnbHotkeyIcon"/>）。</summary>
	protected abstract string IconFile { get; }

	/// <summary>玩家口径的射程上限（米）；实际用 <see cref="EffectiveRange"/>。</summary>
	protected abstract float ConfiguredRange { get; }

	/// <summary>技能是不是 GCD（刚玉 / 极光都是 oGCD，留这个口子只为与骑士骨架对称）。</summary>
	protected virtual bool IsGcd => false;

	internal uint IconActionId => SkillId;

	internal string? CustomIconPath => GnbHotkeyIcon.Resolve(IconFile);

	/// <summary>实际生效射程 = min(配置值, 技能自带射程)；技能读不出射程时退回配置值。</summary>
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

	/// <summary>目标档位的中文名（拼报错文案用）。</summary>
	private string TargetKindName => TargetKind switch
	{
		GnbHelper.SupportTargetKind.Partner => "搭档坦克",
		GnbHelper.SupportTargetKind.Healer => "治疗",
		GnbHelper.SupportTargetKind.LowestHp => "血量最低的队友",
		_ => TargetKind.ToString(),
	};

	/// <summary>技能就绪（含充能：还有层数但下一层在转也算就绪）。</summary>
	protected virtual bool IsSkillReady()
	{
		if (!ActionHelper.IsReady(SkillId))
		{
			return ActionHelper.GetActionCharges(SkillId) >= 1f;
		}
		return true;
	}

	/// <summary>
	/// 资源闸门。刚玉之心与极光**都不吃职业资源**（没有忠义值那种东西）⇒ 恒 true。
	/// 留成虚方法是为了与骑士骨架对称，将来有吃资源的支援技能时在这里加。
	/// </summary>
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
