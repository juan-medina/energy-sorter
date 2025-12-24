// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using EnergySorter.model;
using Godot;

namespace EnergySorter.scenes.menu;

public partial class DevMenu : Control
{
	private MenuScene _menuScene;

	private SpinBox _batterySpinBox;
	private SpinBox _energiesSpinBox;


	private SpinBox _levelsSpinBox;
	private Button _generateButton;
	private Button _cancelButton;

	private ProgressBar _progressBar;
	private Label _statusLabel;

	private Button _backButton;

	private int _currentLevel;
	private int _currentIteration;

	private AcceptDialog _acceptDialog;

	private Thread _workerThread;
	private CancellationTokenSource _cts;

	private readonly HashSet<string> _generatedLevels = [];

	public override void _Ready()
	{
		Debug.Assert(GetParent() != null, "GetParent() != null");
		Debug.Assert(GetParent().GetParent() != null, "GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() != null, "GetParent().GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() is MenuScene,
			"GetParent().GetParent().GetParent() is not MenuScene");

		_menuScene = GetParent().GetParent().GetParent<MenuScene>();

		_batterySpinBox = GetNode<SpinBox>("BG/Layout/SpinsRow/Batteries/Spin");
		Debug.Assert(_batterySpinBox != null, "_batterySpinBox != null");
		_energiesSpinBox = GetNode<SpinBox>("BG/Layout/SpinsRow/Energies/Spin");
		Debug.Assert(_energiesSpinBox != null, "_energiesSpinBox != null");
		_levelsSpinBox = GetNode<SpinBox>("BG/Layout/SpinsRow/Levels/Spin");
		Debug.Assert(_levelsSpinBox != null, "_levelsSpinBox != null");

		_generateButton = GetNode<Button>("BG/Layout/ButtonRow/Generate");
		Debug.Assert(_generateButton != null, "_generateButton != null");
		_cancelButton = GetNode<Button>("BG/Layout/ButtonRow/Cancel");
		Debug.Assert(_cancelButton != null, "_cancelButton != null");

		_progressBar = GetNode<ProgressBar>("BG/Layout/ProgressRow/Bar");
		Debug.Assert(_progressBar != null, "_progressBar != null");

		_statusLabel = GetNode<Label>("BG/Layout/StatusRow/Label");
		Debug.Assert(_statusLabel != null, "_statusLabel != null");

		_acceptDialog = GetNode<AcceptDialog>("AcceptDialog");

		_backButton = GetNode<Button>("BackButton");
		Debug.Assert(_backButton != null, "_backButton != null");
	}

	private void MessageBox(string title, string text)
	{
		_acceptDialog.Title = title;
		_acceptDialog.DialogText = text;
		_acceptDialog.PopupCentered();
	}

	private void OnGenerateButtonUp()
	{
		if (_batterySpinBox.Value < 3)
		{
			MessageBox("Error", "Batteries must be at least 3.");
			return;
		}

		if (_energiesSpinBox.Value <= 0)
		{
			MessageBox("Error", "Energies must be greater than zero.");
			return;
		}

		if (_energiesSpinBox.Value > _batterySpinBox.Value - 1)
		{
			MessageBox("Error", "Energies must be less than batteries minus one.");
			return;
		}

		if (_levelsSpinBox.Value <= 0)
		{
			MessageBox("Error", "Levels must be greater than zero.");
			return;
		}

		_backButton.Disabled = true;
		_batterySpinBox.Editable = false;
		_energiesSpinBox.Editable = false;
		_levelsSpinBox.Editable = false;
		_generateButton.Disabled = true;
		_cancelButton.Disabled = false;
		_progressBar.Value = 0;
		_progressBar.MaxValue = _levelsSpinBox.Value;
		_statusLabel.Text = "";
		_currentLevel = 1;
		_currentIteration = 1;
		UpdateStatus();
	}

	private void StartThread()
	{
		var batteries = (int)_batterySpinBox.Value;
		var energies = (int)_energiesSpinBox.Value;

		_cts?.Dispose();
		_cts = new CancellationTokenSource();

		_workerThread = new Thread(() => GenerateLevel(batteries, energies, _cts.Token))
		{
			IsBackground = true
		};
		_workerThread.Start();
	}

	private void UpdateStatus()
	{
		_progressBar.Value = _currentLevel - 1;
		_statusLabel.Text = $"Generating: level {_currentLevel} iteration {_currentIteration}...";

		if (!(_currentLevel > _levelsSpinBox.Value)) return;
		_backButton.Disabled = false;
		_batterySpinBox.Editable = true;
		_energiesSpinBox.Editable = true;
		_levelsSpinBox.Editable = true;
		_generateButton.Disabled = false;
		_cancelButton.Disabled = true;
		_statusLabel.Text = "Generation complete!";
	}

	private void OnCancelButtonUp()
	{
		_cts?.Cancel();
		if (_workerThread is { IsAlive: true }) _workerThread.Join(200);

		_backButton.Disabled = false;
		_batterySpinBox.Editable = true;
		_energiesSpinBox.Editable = true;
		_levelsSpinBox.Editable = true;
		_generateButton.Disabled = false;
		_cancelButton.Disabled = true;
		_progressBar.Value = 0;
		_statusLabel.Text = "";

		_workerThread = null;
		_cts?.Dispose();
		_cts = null;
	}

	private void OnBackButtonUp()
	{
		_menuScene.BackToMainMenu();
	}

	private void OnIterationCompleted() => _currentIteration++;

	private void OnLevelCompleted(string level, int steps)
	{
		_workerThread = null;
		if (_generatedLevels.Contains(level)) return;

		Debug.WriteLine($"Level {_currentLevel} generated:\n{level} steps: {steps}\n");
		_currentLevel++;
		_generatedLevels.Add(level);
	}


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!_generateButton.Disabled) return;
		if (_workerThread == null)
		{
			if (_currentLevel <= _levelsSpinBox.Value)
			{
				StartThread();
			}
		}

		UpdateStatus();
	}

	public new void Show()
	{
		_batterySpinBox.Value = 0;
		_energiesSpinBox.Value = 0;
		_levelsSpinBox.Value = 0;
		_progressBar.Value = 0;
		_progressBar.MaxValue = 100;
		_statusLabel.Text = "";

		base.Show();
	}

	private void GenerateLevel(int batteries, int energies, CancellationToken token)
	{
		var levelGenerated = false;
		var export = "";
		var steps = 0;
		while (!levelGenerated && !token.IsCancellationRequested)
		{
			OnIterationCompleted();
			if (token.IsCancellationRequested) break;

			var puzzle = new Puzzle(batteries, energies);
			if (puzzle.IsSolved || puzzle.ContainsClosedBattery) continue;

			puzzle.Sort();
			if (token.IsCancellationRequested) break;

			steps = puzzle.Solve();
			if (token.IsCancellationRequested) break;

			if (steps <= 0) continue;
			levelGenerated = true;
			puzzle.Sort();
			export = puzzle.Export();
		}

		if (token.IsCancellationRequested) return;
		OnLevelCompleted(export, steps);
	}
}