using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public abstract class PldBracketQueueGcd : PldResolverBase
{
	private readonly PldResolverBase[] _queue;

	protected PldBracketQueueGcd(PldResolverBase[] queue)
	{
		_queue = queue;
	}

	protected abstract int BracketLevel { get; }

	private PldResolverBase[] Queue => _queue;

	protected override CheckResult CheckReal()
	{
		IBattleChara me = Core.Me;
		if (((me != null) ? ((ICharacter)me).Level : 0) != BracketLevel)
		{
			return new CheckResult(false, $"不是我的等级档（要精确 {BracketLevel} 级）");
		}
		if (TryPick(out _, out string message))
		{
			return new CheckResult(true, $"等级档 {BracketLevel}：{message}");
		}
		return new CheckResult(false, $"等级档 {BracketLevel}：本档队列里没有可用技能");
	}

	
	private bool TryPick(out PldResolverBase? pick, out string message)
	{
		PldResolverBase[] queue = Queue;
		for (int i = 0; i < queue.Length; i++)
		{
			CheckResult queueCheck = queue[i].Check();
			if (queueCheck.Success)
			{
				pick = queue[i];
				message = queueCheck.Message;
				return true;
			}
		}
		pick = null;
		message = string.Empty;
		return false;
	}

	public override PAction GetAction()
	{
		if (TryPick(out PldResolverBase? pick, out _))
		{
			return pick!.GetAction();
		}
		
		
		return PldAction.Of(PldSkills.先锋剑, ActionType.Gcd);
	}
}
