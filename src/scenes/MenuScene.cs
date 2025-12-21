// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using EnergySorter.globals;
using Godot;


namespace EnergySorter.scenes;

public partial class MenuScene : Node2D
{
	private PackedScene _gameScene;
	private const string GameScenePath = "res://src/scenes/GameScene.tscn";

	private AudioStreamPlayer2D _buttonSound;

	private menu.MainMenu _mainMenu;
	private menu.LevelMenu _levelSelection;
	private menu.DevMenu _devMenu;

	private LevelManager _levelManager;

	public override void _Ready()
	{
		_gameScene = ResourceLoader.Load<PackedScene>(GameScenePath);

		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");
		Debug.Assert(_gameScene != null, "Game scene could not be loaded in MenuScene");

		_mainMenu = GetNode<menu.MainMenu>("UI/LayoutControl/MainMenu");
		Debug.Assert(_mainMenu != null, "MainMenu could not be found in MenuScene");

		_levelSelection = GetNode<menu.LevelMenu>("UI/LayoutControl/LevelMenu");
		Debug.Assert(_levelSelection != null, "LevelMenu could not be found in MenuScene");

		_devMenu = GetNode<menu.DevMenu>("UI/LayoutControl/DevMenu");
		Debug.Assert(_devMenu != null, "DevMenu could not be found in MenuScene");

		_levelManager = LevelManager.Instance;
		Debug.Assert(_levelManager != null, "LevelManager instance is null in MenuScene");
	}

	public async void GoToGame()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();

			Debug.Assert(_gameScene != null, "Game scene is not assigned in the MenuScene");
			GetTree().ChangeSceneToPacked(_gameScene);
		}
		catch (Exception ex)
		{
			GD.PushError($"GoToGame error: {ex}");
		}
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

	public async void GotoLevelSelection()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			if (_levelManager.UnlockedLevel == 1)
			{
				GoToGame();
				return;
			}

			_mainMenu.Hide();
			_levelSelection.Show();
		}
		catch (Exception ex)
		{
			GD.PushError($"GotoLevelSelection error: {ex}");
		}
	}

	public async void GotoDevMenu()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			_mainMenu.Hide();
			_devMenu.Show();
		}
		catch (Exception ex)
		{
			GD.PushError($"GotoDevMenu error: {ex}");
		}
	}

	public async void BackToMainMenu()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			_levelSelection.Hide();
			_devMenu.Hide();
			_mainMenu.Show();
		}
		catch (Exception ex)
		{
			GD.PushError($"BackToMainMenu error: {ex}");
		}
	}
}