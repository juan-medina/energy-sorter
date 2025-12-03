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

	private readonly List<BatteryNode> _batteries = [];
	private readonly List<Battery> _batteryModels = [];

    public override void _Ready()
    {
		for (var i = 0; i < MaxBatteries; i++)
        {
			var batteryModel = new Battery();
			batteryModel.AddEnergy(Colors.Red);
			_batteryModels.Add(batteryModel);
			var battery = GetNode<BatteryNode>($"Battery{i+1:00}");
			battery.SetModel(batteryModel);
			_batteries.Add(battery);
        }
    }
}
