using LittleDart.Common.Mitigation;

namespace LittleDart.Gunbreaker.Mitigation;

public sealed class GnbMitigationScheduler : IMitigationScheduler
{
	public static GnbMitigationScheduler Instance { get; } = new GnbMitigationScheduler();

	public MitigationEngine Engine { get; } = new MitigationEngine(GnbMitigationProfile.Instance);

	public MitigationState State => Engine.State;

	public bool Enabled => Engine.Enabled;

	private GnbMitigationScheduler()
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

	public void AuditAfterPull()
	{
		Engine.AuditAfterPull();
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
