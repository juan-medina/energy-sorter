// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using Godot;

namespace EnergySorter.globals;

public partial class VersionDisplay : CanvasLayer
{
	private RichTextLabel _label;

	public override void _Ready()
	{
		var version = ProjectSettings.GetSetting("application/config/version").AsString();

		_label = GetNode<RichTextLabel>("Text");

		_label.Text = $"v{version}";
	}
}