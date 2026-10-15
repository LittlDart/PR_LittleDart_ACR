using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public abstract class PldBracketQueueGcd : PldResolverBase
{
	private PldResolverBase[]? _queue;

	protected abstract int BracketLevel { get; }

	private PldResolverBase[] Queue => _queue ?? (_queue = CreateQueue());

	protected abstract PldResolverBase[] CreateQueue();

	protected override CheckResult CheckReal()
	{
		IBattleChara me = Core.Me;
		if (((me != null) ? ((ICharacter)me).Level : 0) != BracketLevel)
		{
			return new CheckResult(false, $"不是我的等级档（要精确 {BracketLevel} 级）");
		}
		PldResolverBase[] queue = Queue;
		for (int i = 0; i < queue.Length; i++)
		{
			CheckResult queueCheck = queue[i].Check();
			if (queueCheck.Success)
			{
				return new CheckResult(true, $"等级档 {BracketLevel}：{queueCheck.Message}");
			}
		}
		return new CheckResult(false, $"等级档 {BracketLevel}：本档队列里没有可用技能");
	}

	public override PAction GetAction()
	{
		PldResolverBase[] queue = Queue;
		foreach (PldResolverBase pldResolverBase in queue)
		{
			if (pldResolverBase.Check().Success)
			{
				return pldResolverBase.GetAction();
			}
		}
		return Queue[0].GetAction();
	}
}
