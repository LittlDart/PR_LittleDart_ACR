namespace LittleDart.Common.Mitigation;

public readonly struct ReserveRequirement(int big, int @short, int team)
{
	public readonly int Big = big;

	public readonly int Short = @short;

	public readonly int Team = team;

	public override string ToString()
	{
		return $"大减×{Big} 短减×{Short} 团队×{Team}";
	}
}
