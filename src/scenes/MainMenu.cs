// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using Godot;

namespace EnergySorter.scenes;

public partial class MainMenu : Control
{
	private Button _playButton;
	private MenuScene _menuScene;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("PlayButton");
		_menuScene = GetParent().GetParent<MenuScene>();
	}

	private void OnPlayButtonUp()
	{
		_playButton.Disabled = true;
		_menuScene.GoToGame();
	}
}