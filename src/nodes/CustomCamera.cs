// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using Godot;

namespace EnergySorter.nodes;

public partial class CustomCamera : Camera2D
{
	private static readonly Vector2 DesignResolution = new(640, 360);

	public override void _Ready()
	{
		base._Ready();
		MakeCurrent();
		GetViewport().SizeChanged += OnViewportSizeChanged;
		OnViewportSizeChanged();
	}

	public override void _ExitTree()
	{
		GetViewport().SizeChanged -= OnViewportSizeChanged;
		base._ExitTree();
	}

	private void OnViewportSizeChanged()
	{
		var zoom = GetViewportRect().Size.Y / DesignResolution.Y;
		Zoom = new Vector2(zoom, zoom);
	}
}