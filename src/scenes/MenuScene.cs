// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Godot;

namespace EnergySorter.scenes;

public partial class MenuScene : Node2D
{
	private PackedScene _nextScene;
	private const string NextScenePath = "res://src/scenes/GameScene.tscn";

	private Button _playButton;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("UI/LayoutControl/PlayButton");
		_nextScene = ResourceLoader.Load<PackedScene>(NextScenePath);
		Debug.Assert(_nextScene != null, "Next scene could not be loaded in MenuScene");
	}

	private void OnPlayButtonUp()
	{
		Debug.Assert(_nextScene != null, "Next scene is not assigned in the MenuScene");
		GetTree().ChangeSceneToPacked(_nextScene);
	}
}