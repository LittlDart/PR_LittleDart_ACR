using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Common.Ui;

internal static class ChangelogSection
{
	
	private const float BodyHeight = 148f;

	
	public static void Draw()
	{
		if (!ImGui.CollapsingHeader("更新日志###ld_changelog", (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.TextMuted);
		ImGui.TextUnformatted("当前版本：" + RotationInfo.Version);
		ImGui.SameLine(0f, 8f);
		ImGui.TextUnformatted("·  Build " + UiBuildInfo.Stamp);
		ImGui.PopStyleColor();
		ImGui.Separator();

		ImGui.BeginChild("##ld_changelog_body", new Vector2(0f, BodyHeight), false);
		foreach (Changelog.Entry entry in Changelog.Entries)
		{
			ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Accent);
			ImGui.TextUnformatted(entry.Version + "  ·  " + entry.Date);
			ImGui.PopStyleColor();
			ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Text);
			ImGui.TextWrapped(entry.Note);
			ImGui.PopStyleColor();
			ImGui.Spacing();
		}
		ImGui.EndChild();
	}
}
