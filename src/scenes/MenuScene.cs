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

	private AudioStreamPlayer2D _buttonSound;

	private MainMenu _mainMenu;
	private LevelSelection _levelSelection;

	public override void _Ready()
	{
		_nextScene = ResourceLoader.Load<PackedScene>(NextScenePath);

		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");
		Debug.Assert(_nextScene != null, "Next scene could not be loaded in MenuScene");

		_mainMenu = GetNode<MainMenu>("UI/LayoutControl/MainMenu");
		Debug.Assert(_mainMenu != null, "MainMenu could not be found in MenuScene");

		_levelSelection = GetNode<LevelSelection>("UI/LayoutControl/LevelSelection");
		Debug.Assert(_levelSelection != null, "LevelSelection could not be found in MenuScene");
	}

	public async void GoToGame()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();

			Debug.Assert(_nextScene != null, "Next scene is not assigned in the MenuScene");
			GetTree().ChangeSceneToPacked(_nextScene);
		}
		catch (Exception ex)
		{
			GD.PushError($"GoToGame error: {ex}");
		}
	}

	public async void GotoLevelSelection()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			_mainMenu.Hide();
			_levelSelection.Show();

			/*await Fader.Instance.OutIn();

			Debug.Assert(_nextScene != null, "Next scene is not assigned in the MenuScene");
			GetTree().ChangeSceneToPacked(_nextScene);*/
		}
		catch (Exception ex)
		{
			GD.PushError($"GotoLevelSelection error: {ex}");
		}
	}

	public async void BackToMainMenu()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			_levelSelection.Hide();
			_mainMenu.Show();
		}
		catch (Exception ex)
		{
			GD.PushError($"BackToMainMenu error: {ex}");
			throw;
		}
	}
}