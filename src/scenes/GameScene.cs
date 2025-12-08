// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EnergySorter.globals;
using EnergySorter.model;
using EnergySorter.nodes;
using Godot;


namespace EnergySorter.scenes;

public partial class GameScene : Node2D
{
	private const string LevelsPath = "res://levels/levels.txt";
	private const int MaxBatteries = 12;

	private Puzzle _puzzle;
	private Puzzle _savedPuzzle;

	private readonly List<BatteryNode> _batteries = [];
	private Label _messageLabel;
	private Label _levelLabel;
	private Button _resetButton;
	private Button _nextButton;

	private PackedScene _backScene;
	private const string BackScenePath = "res://src/scenes/MenuScene.tscn";

	private AudioStreamPlayer2D _buttonSound;

	private readonly List<string> _levelsImports = [];
	private int _currentLevelNumber = 1;

	public override void _Ready()
	{
		_messageLabel = GetNode<Label>("UI/LayoutControl/MessageLabel");
		_levelLabel = GetNode<Label>("UI/LayoutControl/LevelLabel");

		_resetButton = GetNode<Button>("UI/LayoutControl/Buttons/ResetButton");
		_nextButton = GetNode<Button>("UI/LayoutControl/Buttons/NextButton");

		_backScene = ResourceLoader.Load<PackedScene>(BackScenePath);
		Debug.Assert(_backScene != null, "Back scene could not be loaded in GameScene");

		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");

		for (var i = 0; i < MaxBatteries; i++)
		{
			var batteryNode = GetNode<BatteryNode>($"Battery{i + 1:00}");
			_batteries.Add(batteryNode);
			batteryNode.OnClicked += OnBatteryClicked;
		}

		ReadLevels();
		LoadLevel(_currentLevelNumber);
	}

	private void ReadLevels()
	{
		if (!FileAccess.FileExists(LevelsPath))
		{
			Debug.WriteLine($"Levels file not found at path: {LevelsPath}");
			return;
		}

		using var file = FileAccess.Open(LevelsPath, FileAccess.ModeFlags.Read);
		var stringLevels = file.GetAsText();
		var levels = stringLevels.Split('\n');
		foreach (var level in levels)
		{
			var trimmedLevel = level.Trim();
			if (trimmedLevel.Length > 0) _levelsImports.Add(trimmedLevel);
		}

		Debug.Assert(_levelsImports.Count > 0, "No levels were loaded from the levels file.");
		Debug.WriteLine("Total Levels Loaded: " + _levelsImports.Count);
	}

	private void LoadLevel(int number)
	{
		Debug.Assert(number > 0 && number <= _levelsImports.Count, "Level number out of range.");
		var levelData = _levelsImports[number - 1];
		_puzzle = Puzzle.Import(levelData);
		_savedPuzzle = _puzzle.Clone();

		_levelLabel.Text = $"Level: {number} / {_levelsImports.Count}";
		_nextButton.Hide();
		_resetButton.Show();

		UpdateBatteriesVisuals();
	}

	private void NewPuzzle()
	{
		_puzzle = new Puzzle(6, 2);
		_savedPuzzle = _puzzle.Clone();
		UpdateBatteriesVisuals();
		Debug.WriteLine($"New Puzzle Created: {_puzzle.Export()}");
	}

	private void UpdateBatteriesVisuals()
	{
		for (var i = 0; i < MaxBatteries; i++) _batteries[i].Visible = false;

		var total = _puzzle.Batteries.Length;
		for (var i = 0; i < total; i++)
		{
			_batteries[i].Visible = true;
			_batteries[i].SetModel(_puzzle.Batteries[i]);
		}
	}

	private BatteryNode _selectedBattery;

	private void OnBatteryClicked(BatteryNode batteryNode)
	{
		if (_selectedBattery == null)
		{
			if (batteryNode.IsEmpty() || batteryNode.IsClosed()) return;
			_selectedBattery = batteryNode;
			_selectedBattery.Select();
		}
		else
		{
			if (_selectedBattery == batteryNode)
			{
				_selectedBattery.Deselect();
				_selectedBattery = null;
				return;
			}

			if (!batteryNode.CanGetEnergyFrom(_selectedBattery)) return;
			batteryNode.TransferEnergyFrom(_selectedBattery);
			_selectedBattery.Deselect();
			_selectedBattery = null;
			CheckEndCondition();
		}
	}

	private void CheckEndCondition()
	{
		if (!_puzzle.IsSolved)
		{
			if (_puzzle.HasMoreMoves) return;
			_messageLabel.Text = "Can't transfer any energy! You loose!";
		}
		else
		{
			_resetButton.Hide();

			if (_currentLevelNumber == _levelsImports.Count)
			{
				_messageLabel.Text = "Congratulations! You've completed all levels!";
				_nextButton.Hide();
			}
			else
			{
				_messageLabel.Text = "All energy is sorted! You win!";
				_nextButton.Show();
			}
		}

		DisableAllBatteries();
	}

	private void DisableAllBatteries() => ChangeAllBatteriesEnabling(false);

	private void EnableAllBatteries() => ChangeAllBatteriesEnabling(true);

	private void ChangeAllBatteriesEnabling(bool enable)
	{
		foreach (var battery in _batteries) battery.Enabled = enable;
	}

	private async void OnNextButtonUp()
	{
		try
		{
			_buttonSound.Play();

			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();

			_messageLabel.Text = string.Empty;
			_currentLevelNumber++;
			LoadLevel(_currentLevelNumber);
			EnableAllBatteries();
		}
		catch (Exception ex)
		{
			GD.PushError($"OnNextButtonUp error: {ex}");
		}
	}

	private async void OnBackButtonUp()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			await Fader.Instance.OutIn();
			GetTree().ChangeSceneToPacked(_backScene);
		}
		catch (Exception ex)
		{
			GD.PushError($"OnNextButtonUp error: {ex}");
		}
	}

	private async void OnResetButtonUp()
	{
		try
		{
			_buttonSound.Play();
			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());

			_puzzle = _savedPuzzle.Clone();
			UpdateBatteriesVisuals();
			_messageLabel.Text = string.Empty;
			EnableAllBatteries();
		}
		catch (Exception ex)
		{
			GD.PushError($"OnNextButtonUp error: {ex}");
		}
	}
}