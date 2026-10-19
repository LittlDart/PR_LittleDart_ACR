using System;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;

namespace LittleDart.Paladin.GCD;

/// <summary>
/// 骑士「群体档」的**总闸门**（唯一实现）。
///
/// <para>★★ 2026-10-16 玩家口径：「当自身 5 米内 ≥ 3 只怪物 ⇒ **不管当前目标，直接释放 AOE 技能**」。
/// 于是这里拆成两个入口，唯一的区别就是**要不要先把目标选好**：</para>
///
/// <list type="table">
/// <item>
///   <term><see cref="ShouldUseAoe"/></term>
///   <description><b>旧闸门，原样冻住</b>：QT + **有有效目标** + 敌人数。
///   ★ 现在**只有悔罪连在用**（<c>FofAoeConfiteorGcd</c>：悔罪 16459 的 Action 表 Range = 25、
///   CanTargetSelf = 否 ⇒ 它本来就必须选敌人）。玩家这一轮的口径是「**悔罪连不动**」
///   ⇒ 这一条连语义带调用方一起不许改。</description>
/// </item>
/// <item>
///   <term><see cref="ShouldUseAoeIgnoringTarget"/></term>
///   <description><b>新闸门</b>：QT + 敌人数 + **5 米内挑得出最近的一只**，完全不读 <c>Core.Target</c>。
///   用在自身中心的那四格：全蚀斩 7381 / 日珥斩 16457 / 圣环 16458（普通 + 战逃内）——
///   它们的 Action 表实测都是 Range = 0、EffectRange = 5、CanTargetSelf = 是
///   ⇒ **不需要选中目标**，原来那两句 <c>HasValidTarget() / IsInMeleeRange()</c>
///   让「玩家没清掉目标」变成整条 AOE 轴的前置条件。</description>
/// </item>
/// </list>
///
/// <para>基数（敌人数）仍是老口径：<c>PldSettings.AoeRadius</c> 米内 ≥ <c>PldSettings.AoeEnemyCount</c> 只，
/// 按 <c>Environment.TickCount64</c> 做 1 帧缓存（同一个 tick 内多个判定器只数一次）。</para>
/// </summary>
internal static class AoeQueue
{
	private static long _enemyCountTick = long.MinValue;

	private static int _enemyCountCache;

	/// <summary>
	/// 群体档**旧闸门**（QT + 有有效目标 + 敌人数）—— ★ 2026-10-16 起**只有悔罪连在用**
	/// （<c>FofAoeConfiteorGcd</c>）。玩家口径「悔罪连不动」⇒ 语义与调用方一起冻住。
	/// 自身中心 AOE 技能请用 <see cref="ShouldUseAoeIgnoringTarget"/>。
	/// </summary>
	public static bool ShouldUseAoe()
	{
		return PldHelper.HasValidTarget() && QtAndEnemyCountOk();
	}

	/// <summary>
	/// ★★ 2026-10-16 玩家口径：**AOE 轴不看当前选定的目标** ——
	/// QT「AOE」开着 + 自身 <c>AoeRadius</c> 米内 ≥ 设置门槛 只怪 + **5 米内挑得出最近的一只**。
	///
	/// <para>为什么要加最后那半句：<c>PldHelper.GetNearbyEnemyCount</c> 是**老口径**（不过滤尸体），
	/// 只有「挑得出最近的一只」（<c>PldHelper.GetNearestEnemy</c>，过滤 <c>IsDead</c> / <c>IsTargetable</c>）
	/// 才能保证「判定成功 ⇒ 出手时真有活怪可打」（铁律②）。半径用技能的 EffectRange（5 米），
	/// 不是数怪用的 <c>AoeRadius</c>（本机存档 6 米）—— 详见 <c>PldHelper.AoeEffectRange</c>。</para>
	/// </summary>
	public static bool ShouldUseAoeIgnoringTarget()
	{
		return QtAndEnemyCountOk() && PldHelper.GetNearestEnemy(PldHelper.AoeEffectRange) != null;
	}

	/// <summary>
	/// 每帧采样一次敌人数（<c>PldRotationEventHandler.OnUpdate</c> 驱动）。
	///
	/// <para>★ 2026-10-18 官方口径审计 P1（S5 / C2）：原来采样写在 <see cref="QtAndEnemyCountOk"/>
	/// 里 —— 那是决策器 <c>Check()</c> 的调用链，PR 每帧还会用 <c>UpdateDebugStatus()</c> 再调一次
	/// （impl:91722）⇒ <c>Check()</c> 会写缓存。现在采样只在 Tick 里做，<c>Check()</c> 只读快照。</para>
	/// </summary>
	public static void Tick()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseAoe))
		{
			return;
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount != _enemyCountTick)
		{
			_enemyCountTick = tickCount;
			_enemyCountCache = PldHelper.GetNearbyEnemyCount((int)instance.AoeRadius);
		}
	}

	private static bool QtAndEnemyCountOk()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseAoe))
		{
			return false;
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return false;
		}
		return _enemyCountCache >= instance.AoeEnemyCount;
	}
}
