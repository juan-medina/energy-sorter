// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Godot;

namespace EnergySorter.scenes;

public partial class MenuScene : Node2D
{
	[Export] private PackedScene _nextScene;

	private Button _playButton;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("UI/LayoutControl/PlayButton");
	}

	private void OnPlayButtonUp()
	{
		Debug.Assert(_nextScene != null, "Next scene is not assigned in the MenuScene");
		GetTree().ChangeSceneToPacked(_nextScene);
	}
}