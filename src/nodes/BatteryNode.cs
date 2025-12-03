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
	private static readonly Color[] EnergyColors =
	[
		new(31f / 255f, 119f / 255f, 180f / 255f), // #1F77B4 blue
		new(255f / 255f, 127f / 255f, 14f / 255f), // #FF7F0E orange
		new(44f / 255f, 160f / 255f, 44f / 255f), // #2CA02C green
		new(214f / 255f, 39f / 255f, 40f / 255f), // #D62728 red
		new(148f / 255f, 103f / 255f, 189f / 255f), // #9467BD purple
		new(140f / 255f, 86f / 255f, 75f / 255f), // #8C564B brown
		new(227f / 255f, 119f / 255f, 194f / 255f), // #E377C2 pink
		new(255f / 255f, 215f / 255f, 0f / 255f), // #FFD700 gold
		new(0f / 255f, 128f / 255f, 128f / 255f), // #008080 teal
		new(23f / 255f, 190f / 255f, 207f / 255f) // #17BECF cyan
	];

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
		}

		var limit = Math.Min(energies.Length, Battery.MaxEnergy);
		for (var idx = 0; idx < limit; idx++)
		{
			var type = energies[idx];
			var sprite = _energySprites[idx];
			Debug.Assert(type >= 1 && type <= EnergyColors.Length, "Update Visuals: Energy type is out of range");
			if (type < 1 || type > EnergyColors.Length) continue;
			sprite.Modulate = EnergyColors[type-1];
			sprite.Visible = true;
		}
	}
}