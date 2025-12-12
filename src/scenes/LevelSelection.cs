// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace EnergySorter.scenes;

public partial class LevelSelection : Control
{
	private MenuScene _menuScene;

	private const int DecadesButtonsCount = 8;
	private const int LevelButtonsCount = 10;

	private readonly List<Button> _levelButtons = [];

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
		}

		for (var level = 1; level <= LevelButtonsCount; level++)
		{
			var button = GetNode<Button>($"Levels/{level:00}");
			Debug.Assert(button != null, $" level buttons {level} is not found");
			var levelNumber = level;
			button.ButtonUp += () => OnLevelButtonUp(levelNumber);
			_levelButtons.Add(button);
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
		for (var button = 0; button < LevelButtonsCount; button++) _levelButtons[button].Text = $"{from + button}";
	}

	private void OnLevelButtonUp(int level) => _menuScene.GoToGame(_levelButtons[level - 1].Text.ToInt());
}