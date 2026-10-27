using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LittleDart.Common.Ui;

public sealed class UiWindowLayout
{
	private const float MinimumWidth = 240f;

	private const float MinimumHeight = 160f;

	private const float MaximumSize = 8192f;

	private const float PositionLimit = 16384f;

	private readonly string _key;

	private bool _applied;

	private bool _hasCache;

	private Vector2 _lastPosition;

	private Vector2 _lastSize;

	public UiWindowLayout(string key)
	{
		_key = key;
	}

	
	public void Apply(Window window)
	{
		if (_applied)
		{
			return;
		}
		_applied = true;
		UiWindowLayoutState state = InterfaceSettings.Instance.GetLayout(_key);
		if (!state.HasPosition || !state.HasSize || !IsSane(state.W, state.H, state.X, state.Y))
		{
			return;
		}
		window.Position = new Vector2(state.X, state.Y);
		
		
		
		window.Size = new Vector2(state.W, state.H) / UiTheme.UiScale;
		_lastPosition = new Vector2(state.X, state.Y);
		_lastSize = new Vector2(state.W, state.H);
		_hasCache = true;
	}

	
	public void Track()
	{
		Vector2 position = ImGui.GetWindowPos();
		Vector2 size = ImGui.GetWindowSize();
		if (!IsSane(size.X, size.Y, position.X, position.Y))
		{
			return;
		}
		_lastPosition = position;
		_lastSize = size;
		_hasCache = true;
		if (ImGui.IsAnyItemActive() || ImGui.IsMouseDown(ImGuiMouseButton.Left))
		{
			return;
		}
		Persist();
	}

	
	public void Flush()
	{
		Persist();
	}

	private void Persist()
	{
		if (!_hasCache)
		{
			return;
		}
		UiWindowLayoutState state = InterfaceSettings.Instance.GetLayout(_key);
		bool unchanged = state.HasPosition && state.HasSize
			&& MathF.Abs(state.X - _lastPosition.X) < 0.5f
			&& MathF.Abs(state.Y - _lastPosition.Y) < 0.5f
			&& MathF.Abs(state.W - _lastSize.X) < 0.5f
			&& MathF.Abs(state.H - _lastSize.Y) < 0.5f;
		if (unchanged)
		{
			return;
		}
		state.HasPosition = true;
		state.X = _lastPosition.X;
		state.Y = _lastPosition.Y;
		state.HasSize = true;
		state.W = _lastSize.X;
		state.H = _lastSize.Y;
		InterfaceSettings.Instance.Save();
	}

	
	private static bool IsSane(float width, float height, float x, float y)
	{
		if (float.IsNaN(width) || float.IsNaN(height) || float.IsNaN(x) || float.IsNaN(y))
		{
			return false;
		}
		if (width < MinimumWidth || height < MinimumHeight || width > MaximumSize || height > MaximumSize)
		{
			return false;
		}
		return MathF.Abs(x) <= PositionLimit && MathF.Abs(y) <= PositionLimit;
	}
}
