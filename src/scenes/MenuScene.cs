// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using EnergySorter.globals;
using Godot;


namespace EnergySorter.scenes;

public partial class MenuScene : Node2D
{
	private PackedScene _nextScene;
	private const string NextScenePath = "res://src/scenes/GameScene.tscn";

	private Button _playButton;
	private AudioStreamPlayer2D _buttonSound;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("UI/LayoutControl/PlayButton");
		_nextScene = ResourceLoader.Load<PackedScene>(NextScenePath);
		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");

		Debug.Assert(_nextScene != null, "Next scene could not be loaded in MenuScene");
	}

	private async void OnPlayButtonUp()
	{
		try
		{
			_buttonSound.Play();
			_playButton.Disabled = true;
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();

			Debug.Assert(_nextScene != null, "Next scene is not assigned in the MenuScene");
			GetTree().ChangeSceneToPacked(_nextScene);
		}
		catch (Exception ex)
		{
			GD.PushError($"OnPlayButtonUp error: {ex}");
		}
	}
}