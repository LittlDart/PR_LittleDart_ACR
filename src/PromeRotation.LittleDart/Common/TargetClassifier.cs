using System;
using System.Collections.Generic;

namespace LittleDart.Common;

/// <summary>当前这场交战的目标形态。</summary>
public enum TargetProfile
{
	/// <summary>判不出来（还没进战 / 没目标 / 血量速率还没采到）。<b>不要</b>拿它当 boss 或群怪。</summary>
	Unknown = 0,

	/// <summary>单体 boss —— 一个目标、活得久。</summary>
	SingleBoss = 1,

	/// <summary>群怪 / 拉怪堆 —— 目标多，或者单个但活得短。</summary>
	Pack = 2,
}

/// <summary>
/// 「单体 boss 还是群怪」的分类器（2026-10-14 玩家需求）。
///
/// <para><b>为什么不读 boss rank</b>：查过 PR 公开接口面、Lumina 的 <c>BNpcBase</c> 行、
/// 以及 SDK 自带的 <c>FFXIVClientStructs</c>，**都没有**「这个怪是不是 boss」的读数 ——
/// 详见 <c>docs\23</c> §12。所以这里改用**可观测量**判：
/// 身边敌人数 + 目标按实测血量下降速率预测的存活时间。</para>
///
/// <para><b>为什么用「活得久」当主判据</b>：单体 boss 要打几十秒到几分钟，小怪通常几秒到十几秒
/// 就死了 —— 这个差距比任何静态标记都稳，而且项目里已经有现成的
/// <see cref="HpTracker.SurvivesLongerThan"/>（对 boss 场景反而最准）。</para>
///
/// <para><b>读不到速率时算「活得久」</b>（<see cref="HpTracker.SurvivesLongerThan"/> 的既有约定）——
/// 战斗刚开始 / 目标满血时速率是 0，若判成「马上要死」会得出错误的 Pack。
/// 这条与骑士减伤那句「目标 10 秒内将死就省大招」是**相反**的两侧，不要互相转发。</para>
/// </summary>
public static class TargetClassifier
{
	/// <summary>身边多少米内算「这一场交战里」。与绝枪侧 <c>GetNearbyEnemyCount(5)</c> 用的 5m 是两回事。</summary>
	public const float EngageRadius = 15f;

	/// <summary>存活时间超过这么多秒 ⇒ 认为它是 boss 档（不是几秒就化的小怪）。</summary>
	public const double BossSurvivesSeconds = 30.0;

	/// <summary>敌人数 ≥ 这个值 ⇒ 直接算群怪（不用等血量速率）。</summary>
	public const int PackEnemyCount = 3;

	/// <summary>
	/// 分类。
	/// </summary>
	/// <param name="enemyCount">交战半径内的敌人数（由调用方注入，通常是 <c>GetNearbyEnemyCount(15)</c>）。</param>
	/// <param name="targetSurvivesLongerThan">
	/// 目标是否活得比给定秒数久。由调用方注入 <c>HpTracker.SurvivesLongerThan</c>，
	/// 好让本类不直接依赖绝枪侧的胶水。</param>
	public static TargetProfile Classify(int enemyCount, Func<double, bool> targetSurvivesLongerThan)
	{
		if (enemyCount <= 0)
		{
			return TargetProfile.Unknown;
		}
		// ① 数量本身就是最强信号：≥3 只不用再问血量
		if (enemyCount >= PackEnemyCount)
		{
			return TargetProfile.Pack;
		}
		// ② 1~2 只：靠「活得久不久」分。boss 活得久；小怪几秒就化
		bool survives;
		try
		{
			survives = targetSurvivesLongerThan(BossSurvivesSeconds);
		}
		catch
		{
			return TargetProfile.Unknown;
		}
		return survives ? TargetProfile.SingleBoss : TargetProfile.Pack;
	}

	public static string Describe(TargetProfile profile)
	{
		return profile switch
		{
			TargetProfile.SingleBoss => "单体 boss",
			TargetProfile.Pack => "群怪",
			_ => "判不出（未进战 / 无目标 / 血量速率未采到）",
		};
	}

	/// <summary>给诊断面板 / 日志用的一行说明（含两个原始读数，便于上机校准阈值）。</summary>
	public static string DescribeWithInputs(TargetProfile profile, int enemyCount, bool? survives30s)
	{
		string survivesText = survives30s switch
		{
			true => $"活得 > {BossSurvivesSeconds:F0}s",
			false => $"活不到 {BossSurvivesSeconds:F0}s",
			_ => "存活时间读不出（按「活得久」算）",
		};
		return $"目标形态={Describe(profile)}（交战半径 {EngageRadius:F0}m 内 {enemyCount} 只；{survivesText}）";
	}

	/// <summary>
	/// 需要「2 只以上但都是 boss 级」这类更细的分档时用。
	/// 当前 <see cref="Classify"/> 不区分「单 boss + 小弟」这种双目标 boss 战 ——
	/// 那种情况会落进 <see cref="TargetProfile.Pack"/>，是**故意**的保守选择
	/// （见 docs\23 §12 的「已知局限」）。
	/// </summary>
	public static IReadOnlyList<string> KnownLimitations { get; } = new[]
	{
		"双目标 boss 战（boss + 常驻小弟）会被判成群怪 —— 因为判据只看数量与存活时间，不看 rank。",
		"拉怪途中（怪还在往你这跑）会被判成群怪 —— 这正是玩家想要的「拉怪堆」语义。",
		"刚进战的前 2.5 秒血量速率还没采够 ⇒ 可能先返回 Unknown 再变成 SingleBoss。",
	};
}
