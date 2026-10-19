namespace LittleDart.Common.Mitigation;

public interface IMitigationScheduler
{
	bool Enabled { get; }

	void Tick();

	bool ShouldCast(uint skillId);

	void NotifyCast(uint skillId);

	void Invalidate();

	void Reset();

	MitigationDecision? GetCurrentDecision();
}
