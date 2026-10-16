using PromeRotation.Data;
using PromeRotation.Rotation;

namespace PromeRotation.Sample.Events;

/// <summary>
/// 事件回调。框架在进战 / 脱战 / 无目标 / 换地图时叫这里。
/// </summary>
/// <remarks>
/// <para>
/// <b>为什么必须认真写这一组空方法</b>：ACR 是一份长期活着的对象，
/// 上一场战斗的临时状态（本地施放记录、缓存的目标、缓冲的计时）会带进下一场，
/// 表现是「第二把起手不对」「脱战后还在试图出手」。
/// <b>进战/脱战/无目标/换地图都要把本地状态清干净。</b>
/// </para>
/// <para>
/// 这里的 <c>OnBattleEnded</c> 顺便重置了 <c>PromeSettings.Instance.OpenerHasBeenExecuted</c>，
/// 这是抄 PR 自带的 <c>SimpleRotationEventHandler</c> 的做法——
/// 不重置的话下一次开怪起手不会重新执行。
/// </para>
/// <para>
/// ⚠️ 和构造函数对称：如果这里 <c>+=</c> 订阅了什么（例如 Dalamud 的事件），
/// 就必须有一个对应的 <c>Dispose</c> 去 <c>-=</c>。PR 会重新加载 ACR，
/// 漏了的后果是回调触发两次、甚至卸载不掉。
/// </para>
/// </remarks>
public sealed class SampleRotationEventHandler : IRotationEventHandler
{
    /// <summary>每帧都会叫一次。</summary>
    public void OnUpdate()
    {
    }

    /// <summary>不在战斗中时每帧叫一次。</summary>
    public void OnOutOfBattleUpdate()
    {
    }

    /// <summary>进入战斗。</summary>
    public void OnBattleStarted()
    {
    }

    /// <summary>战斗中每帧叫一次。</summary>
    public void OnBattleUpdate()
    {
    }

    /// <summary>当前没有可攻击目标。</summary>
    public void OnNoTarget()
    {
    }

    /// <summary>战斗结束。★ 本地状态要在这里清干净。</summary>
    public void OnBattleEnded()
    {
        PromeSettings.Instance.OpenerHasBeenExecuted = false;
    }

    /// <summary>换地图。</summary>
    public void OnTerritoryChanged(ushort territoryId)
    {
    }
}
