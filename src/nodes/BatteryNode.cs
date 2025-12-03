// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
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
		for(var i = 0; i < Battery.MaxEnergy; i++)
		{
			_energySprites.Add(GetNode<Sprite2D>($"Body/Energy{i+1}"));
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

		foreach(var sprite in _energySprites)
		{
			sprite.Visible = false;
			sprite.Modulate = Colors.White;
			sprite.Visible = false;
		}
		var idx = 0;
		foreach (var color in energies)
		{
			var sprite = _energySprites[idx];
			sprite.Modulate = color;
			sprite.Visible = true;
			idx++;
		}
	}
}
