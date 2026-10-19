using LittleDart.Common.Mitigation;

namespace LittleDart.Paladin.Mitigation;

public sealed class MitigationScheduler : IMitigationScheduler
{
	public static MitigationScheduler Instance { get; } = new MitigationScheduler();

	public MitigationEngine Engine { get; } = new MitigationEngine(PldMitigationProfile.Instance);

	public MitigationState State => Engine.State;

	public bool Enabled => Engine.Enabled;

	private MitigationScheduler()
	{
	}

	public void Tick()
	{
		Engine.Tick();
	}

	public bool ShouldCast(uint skillId)
	{
		return Engine.ShouldCast(skillId);
	}

	public void NotifyCast(uint skillId)
	{
		Engine.NotifyCast(skillId);
	}

	public void Invalidate()
	{
		Engine.Invalidate();
	}

	public void Reset()
	{
		Engine.Reset();
	}

	public MitigationDecision? GetCurrentDecision()
	{
		return Engine.GetCurrentDecision();
	}

	public string DescribeActiveMits()
	{
		return Engine.DescribeActiveMits();
	}

	public string DescribeStatus()
	{
		return Engine.DescribeStatus();
	}
}
