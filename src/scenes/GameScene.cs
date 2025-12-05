// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;
using EnergySorter.model;
using Godot;
using BatteryNode = EnergySorter.nodes.BatteryNode;

namespace EnergySorter.scenes;

public partial class GameScene : Node2D
{
	private const int MaxBatteries = 12;

	private Puzzle _puzzle;
	private Puzzle _savedPuzzle;

	private readonly List<BatteryNode> _batteries = [];
	private Label _messageLabel;

	public override void _Ready()
	{
		_messageLabel = GetNode<Label>("UI/LayoutControl/MessageLabel");
		for (var i = 0; i < MaxBatteries; i++)
		{
			var batteryNode = GetNode<BatteryNode>($"Battery{i + 1:00}");
			_batteries.Add(batteryNode);
			batteryNode.OnClicked += OnBatteryClicked;
		}

		NewPuzzle();
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

	private void OnNewButtonUp()
	{
		NewPuzzle();
		_messageLabel.Text = string.Empty;
		EnableAllBatteries();
	}

	private void OnResetButtonUp()
	{
		_puzzle = _savedPuzzle.Clone();
		UpdateBatteriesVisuals();
		_messageLabel.Text = string.Empty;
		EnableAllBatteries();
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
			_messageLabel.Text = "All energy is sorted! You win!";

		DisableAllBatteries();
	}

	private void DisableAllBatteries() => ChangeAllBatteriesEnabling(false);

	private void EnableAllBatteries() => ChangeAllBatteriesEnabling(true);

	private void ChangeAllBatteriesEnabling(bool enable)
	{
		foreach (var battery in _batteries) battery.Enabled = enable;
	}
}