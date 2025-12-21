// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using EnergySorter.globals;
using Godot;

namespace EnergySorter.scenes.menu;

public partial class LevelMenu : Control
{
	private MenuScene _menuScene;

	private const int DecadesButtonsCount = 10;
	private const int LevelButtonsCount = 10;

	private readonly List<Button> _levelButtons = [];
	private readonly List<CheckButton> _decadeButtons = [];

	private LevelManager _levelManager;

	public override void _Ready()
	{
		Debug.Assert(GetParent() != null, "GetParent() != null");
		Debug.Assert(GetParent().GetParent() != null, "GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() != null, "GetParent().GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() is MenuScene,
			"GetParent().GetParent().GetParent() is not MenuScene");

		_menuScene = GetParent().GetParent().GetParent<MenuScene>();

		for (var decade = 1; decade <= DecadesButtonsCount; decade++)
		{
			var checkButton = GetNode<CheckButton>($"Decades/{decade:00}");
			Debug.Assert(checkButton != null, $" decade buttons {decade} is not found");
			var decadeNumber = (decade - 1) * 10 + 1;
			checkButton.ButtonUp += () => OnDecadesButtonUp(decadeNumber);
			_decadeButtons.Add(checkButton);
		}

		for (var level = 1; level <= LevelButtonsCount; level++)
		{
			var button = GetNode<Button>($"Levels/{level:00}");
			Debug.Assert(button != null, $" level buttons {level} is not found");
			var levelNumber = level;
			button.ButtonUp += () => OnLevelButtonUp(levelNumber);
			_levelButtons.Add(button);
		}

		_levelManager = LevelManager.Instance;
		Debug.Assert(_levelManager != null, "LevelManager instance is null in MenuScene");

		var selectedDecade = (_levelManager.CurrentLevel - 1) / 10;
		_decadeButtons[selectedDecade].SetPressed(true);
		OnDecadesButtonUp(selectedDecade * 10 + 1);

		for (var decade = 0; decade < DecadesButtonsCount; decade++)
		{
			var from = decade * 10 + 1;
			_decadeButtons[decade].Disabled = _levelManager.UnlockedLevel < from;
			_decadeButtons[decade].FocusMode = _decadeButtons[decade].Disabled ? FocusModeEnum.None : FocusModeEnum.All;
		}
	}

	private void OnBackButtonUp()
	{
		_menuScene.BackToMainMenu();
	}

	private void OnGoButtonUp()
	{
		_menuScene.GoToGame();
	}

	private void OnDecadesButtonUp(int from)
	{
		_menuScene.ButtonSound();
		for (var button = 0; button < LevelButtonsCount; button++)
		{
			var level = from + button;
			var buttonText = level == _levelManager.CurrentLevel ? $">{from + button}<" : $"{from + button}";
			var levelButton = _levelButtons[button];
			levelButton.Text = buttonText;
			levelButton.Disabled = level > _levelManager.UnlockedLevel;
		}
	}

	private void OnLevelButtonUp(int button)
	{
		_levelManager.CurrentLevel = _levelButtons[button - 1].Text.ToInt();
		_menuScene.GoToGame();
	}
}