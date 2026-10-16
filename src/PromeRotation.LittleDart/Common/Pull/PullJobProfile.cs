using System;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common.Mitigation;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Common.Pull;

public sealed class PullJobProfile
{
	public string JobName = string.Empty;

	public string LogPrefix = "[LittleDart/Pull]";

	public uint SkillId;

	public string QtKey = string.Empty;

	public Func<int> MinAggro = () => 3;

	public MitigationJobProfile Readings = new MitigationJobProfile();

	public Func<float> ActionRange = () => 0f;

	public float FallbackRange = 20f;

	public Func<PAction> BuildAction = () =>
	{
		return new PAction(0u, (ActionType)1, (ActionTargetType)1);
	};

	public Func<uint, bool> IsUnlocked = (uint id) => ActionHelper.IsUnlocked(id);

	public Func<uint, IBattleChara, bool> IsReadyOnTarget = (uint id, IBattleChara target) => ActionHelper.CanCast(id, target) && !ActionHelper.RecentlyUsed(id, 500);

	public Func<bool> DiagnosticsEnabled = () => false;

	public Action<string> Log = (string _) =>
	{
	};
}
