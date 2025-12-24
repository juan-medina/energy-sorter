// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Godot;

namespace EnergySorter.globals;

public partial class VersionDisplay : CanvasLayer
{
	private RichTextLabel _label;
	private static readonly List<string> Colors = ["#F000F0", "#FF0000", "#FFA500", "#FFFF00", "#00FF00"];

	public override void _Ready()
	{
		var version = ProjectSettings.GetSetting("application/config/version").AsString();
		var parts = version.Split('.');
		Debug.Assert(parts.Length == 4, "Version string does not have four parts.");
		if (parts.Length != 4) return;

		var text = new StringBuilder("v.");


		for (var i = 0; i < parts.Length; i++)
		{
			text.Append(GetBbcodeTextColor(parts[i], Colors[i]));
			if (i < parts.Length - 1) text.Append('.');
		}

		_label = GetNode<RichTextLabel>("Text");
		_label.Text = text.ToString();
	}

	private static string GetBbcodeTextColor(string part, string color) => $"[color={color}]{part}[/color]";
}