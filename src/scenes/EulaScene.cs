// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using EnergySorter.globals;
using Godot;

namespace EnergySorter.scenes;

public partial class EulaScene : Node2D
{
	private const string MenuScenePath = "res://src/scenes/MenuScene.tscn";

	private AudioStreamPlayer2D _buttonSound;
	private PackedScene _menuScene;

	public override void _Ready()
	{
		_menuScene = ResourceLoader.Load<PackedScene>(MenuScenePath);
		Debug.Assert(_menuScene != null, "Game scene could not be loaded in MenuScene");

		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");
		Debug.Assert(_buttonSound != null, "Button sound could not be found in EulaScene");
	}

	public async void ButtonSound()
	{
		try
		{
			if (_buttonSound == null) return;
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());
		}
		catch (Exception ex)
		{
			GD.PushError($"ButtonSound error: {ex}");
		}
	}

	private async void OnAgreeButtonUp()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();

			GetTree().ChangeSceneToPacked(_menuScene);

		}
		catch (Exception ex)
		{
			GD.PushError($"ExitGame error: {ex}");
		}
	}

	private async void OnDeclineButtonUp()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			GetTree().Quit();
		}
		catch (Exception ex)
		{
			GD.PushError($"ExitGame error: {ex}");
		}
	}
}