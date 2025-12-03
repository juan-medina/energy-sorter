// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EnergySorter.model;
using Godot;

namespace EnergySorter.nodes;

public partial class BatteryNode : Area2D
{
	private Battery _battery;
	private readonly List<Sprite2D> _energySprites = [];

	public override void _Ready()
	{
		_battery = new Battery();
		for (var i = 0; i < Battery.MaxEnergy; i++)
		{
			_energySprites.Add(GetNode<Sprite2D>($"Body/Energy{i + 1}"));
		}
	}

	public void SetModel(Battery battery)
	{
		_battery = battery;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		var energies = _battery.Energies;
		Debug.Assert(energies.Length <= Battery.MaxEnergy,
			$"Update Visuals: A battery can not have more than {Battery.MaxEnergy} energy");

		foreach (var sprite in _energySprites)
		{
			sprite.Visible = false;
			sprite.Modulate = Colors.White;
			sprite.Visible = false;
		}

		var limit = Math.Min(energies.Length, Battery.MaxEnergy);
		for (var idx = 0; idx < limit; idx++)
		{
			var color = energies[idx];
			var sprite = _energySprites[idx];
			sprite.Modulate = color;
			sprite.Visible = true;
			idx++;
		}
	}
}