// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Godot;

namespace EnergySorter.scenes.menu;

public partial class DevMenu : Control
{
	private MenuScene _menuScene;

	public override void _Ready()
	{
		Debug.Assert(GetParent() != null, "GetParent() != null");
		Debug.Assert(GetParent().GetParent() != null, "GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() != null, "GetParent().GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() is MenuScene,
			"GetParent().GetParent().GetParent() is not MenuScene");

		_menuScene = GetParent().GetParent().GetParent<MenuScene>();
	}

	private void OnBackButtonUp()
	{
		_menuScene.BackToMainMenu();
	}
}