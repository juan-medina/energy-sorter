// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using EnergySorter.model;
using Godot;
using BatteryNode = EnergySorter.nodes.BatteryNode;

namespace EnergySorter.scenes;

public partial class MainScene : Node2D
{
	private const int MaxBatteries = 12;

	private Puzzle _puzzle;
	private readonly List<BatteryNode> _batteries = [];

	public override void _Ready()
	{
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
		UpdateBatteriesVisuals();
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

	private void OnNewButtonUp() => NewPuzzle();

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
		}
	}
}