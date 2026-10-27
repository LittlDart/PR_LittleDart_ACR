using System.Collections.Generic;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;

namespace LittleDart.Gunbreaker.Opener;

public abstract class GnbOpenerBase : IOpener
{
	public abstract string OpenerName { get; }

	public abstract List<PAction> InCombatSequence { get; }

	public virtual void InitializeCountdown(CountDownHandler countdownHandler)
	{
	}

	// ★ 起手里的「开场爆发药」那一格：QT「开场爆发药」开着 **且** 背包里有药时才插进序列。
	//   调用方决定它插在哪儿（GnbOpener100 里插在**无情之后**、同一 oGCD 窗口 —— 见那边的注释）。
	//
	//   ★ 药的 ID 一律走 PR 的**通用接口** GameData.GetBestPotionId()（PromeRotation.Core）：
	//     · 它按当前职业给出一张「从新到旧」的药水表，返回背包里第一个有货的；
	//     · HQ 优先，且 PR 用 `itemId + 1000000` 表示 HQ —— 与 PAction 的口径一致（PR 自己的
	//       UsePotionAction 就是这么造的，impl:43652）；
	//     · 新版本药水上市时由 PR 本体维护那张表 ⇒ 本 ACR 一行都不用改，这正是这条需求的落点。
	//   ⚠️ 不要退回 GnbSkills 里写死的药水 ID（刚力之幻药8级 39727 / 7级 37840 …）：那是快照常量，
	//      版本一过就过期。
	//   ⚠️ 也不要自己读背包 / 自己判 HQ —— 那等于把 PR 的通用接口抄一份出来当第二真相源。
	//
	//   ★ 这一格自己挂 RequiresVerification = true：起手表里**每一格**都要挂（docs\19 A-1/A-2），
	//     而本方法可能一格都不加 ⇒ 由它自己负责，调用方不必回头改 list 的最后一格。
	protected static void AddPotionIfEnabled(List<PAction> sequence)
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.UsePotion))
		{
			return;
		}
		uint bestPotionId = GameData.GetBestPotionId();
		if (bestPotionId == 0)
		{
			return;
		}
		PAction potion = GnbAction.On(bestPotionId, ActionType.Item, ActionTargetType.Self);
		potion.RequiresVerification = true;
		sequence.Add(potion);
	}
}
