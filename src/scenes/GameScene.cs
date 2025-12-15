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

	private PackedScene _sparkNode;

	private int _lastSecondToNextRounded = -1;
	private double _lastSecondToNext = -1;

	private LevelManager _levelManager;

	public override void _Ready()
	{
		_messageLabel = GetNode<Label>("UI/LayoutControl/MessageLabel");
		_levelLabel = GetNode<Label>("UI/LayoutControl/LevelLabel");

		_resetButton = GetNode<Button>("UI/LayoutControl/Buttons/ResetButton");
		_nextButton = GetNode<Button>("UI/LayoutControl/Buttons/NextButton");

		_backScene = ResourceLoader.Load<PackedScene>(BackScenePath);
		Debug.Assert(_backScene != null, "Back scene could not be loaded in GameScene");

		_buttonSound = GetNode<AudioStreamPlayer2D>("Button");

		_sparkNode = ResourceLoader.Load<PackedScene>("res://src/nodes/Spark.tscn");

		for (var i = 0; i < MaxBatteries; i++)
		{
			var batteryNode = GetNode<BatteryNode>($"Battery{i + 1:00}");
			_batteries.Add(batteryNode);
			batteryNode.OnClicked += OnBatteryClicked;
		}

		_levelManager = LevelManager.Instance;
		Debug.Assert(_levelManager != null, "LevelManager instance is null in GameScene");
		LoadLevel();
	}

	private void LoadLevel()
	{
		Debug.Assert(_levelManager.IsInitialized, "LevelManager not ready.");
		_puzzle = Puzzle.Import(_levelManager.GetCurrentLevelData());
		_savedPuzzle = _puzzle.Clone();

		_levelLabel.Text = $"Level: {_levelManager.CurrentLevel} / {_levelManager.TotalLevels}";
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
			ShootSpark(_selectedBattery, batteryNode);
			batteryNode.TransferEnergyFrom(_selectedBattery);
			_selectedBattery.Deselect();
			_selectedBattery = null;
			CheckEndCondition();
		}
	}

	private void ShootSpark(BatteryNode from, BatteryNode to)
	{
		const int sparksCount = 5;
		const float radius = 12f;

		var targetPos = new Vector2(to.Position.X, to.Position.Y);

		for (var i = 0; i < sparksCount; i++)
		{
			var angle = (float)(i * (Math.PI * 2) / sparksCount);
			var offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
			var originPos = new Vector2(from.Position.X, from.Position.Y) + offset;

			var spark = _sparkNode.Instantiate<Spark>();
			spark.Origin = new Vector2I((int)originPos.X, (int)originPos.Y);
			spark.Destination = new Vector2I((int)targetPos.X, (int)targetPos.Y);
			spark.Modulate = from.TopColor();

			AddChild(spark);
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

			if (_levelManager.IsLastLevel())
			{
				_messageLabel.Text = "Congratulations! You've completed all levels!";
				_nextButton.Hide();
			}
			else
			{
				_messageLabel.Text = "All energy is sorted, level completed!";
				CountdownToNext();
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
			_lastSecondToNextRounded = -1;
			_buttonSound.Play();

			await ToSignal(_buttonSound, nameof(_buttonSound.Finished).ToLowerInvariant());
			_nextButton.Hide();

			await Fader.Instance.OutIn();

			_messageLabel.Text = string.Empty;
			_levelManager.NextLevel();
			LoadLevel();
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

	private void CountdownToNext()
	{
		_nextButton.Show();
		_lastSecondToNext = 5.0;
		_lastSecondToNextRounded = 5;
		_nextButton.Text = $"Next ({_lastSecondToNextRounded})";
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_lastSecondToNextRounded == -1) return;

		_lastSecondToNext -= delta;
		var rounded = (int)Math.Ceiling(_lastSecondToNext);
		if (rounded == _lastSecondToNextRounded) return;
		_lastSecondToNextRounded = rounded;
		_nextButton.Text = $"Next ({_lastSecondToNextRounded})";
		if (rounded > 0) return;
		OnNextButtonUp();
	}
}