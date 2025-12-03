// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EnergySorter.model;

public class Puzzle
{
	private const int MaxBatteries = 12;

	private readonly List<Battery> _batteries = [];
	public Battery[] Batteries => _batteries.ToArray();

	public Puzzle(int fullBatteries, int emptyBatteries)
	{
		// Summary: It constructs a puzzle by creating a fixed set of battery slots, reserving some as empty,
		// picking a few distinct colors, and then randomly dropping a fixed number of energy units
		// of each chosen color into the non-empty slots until every chosen color has placed its full quota.
		//
		// The objective is to create random puzzles with a mix of full and empty batteries so the player
		// can sort the energies by color
		Debug.Assert(fullBatteries + emptyBatteries <= MaxBatteries,
			$"A puzzle can not have more than {MaxBatteries} batteries");

		var limitFullBatteries = Math.Min(fullBatteries, MaxBatteries - emptyBatteries);

		Debug.Assert(limitFullBatteries <= Battery.MaxEnergyTypes,
			$"A puzzle can not have more than {Battery.MaxEnergyTypes} full batteries with unique type");

		limitFullBatteries = Math.Min(limitFullBatteries, Battery.MaxEnergyTypes);

		var randomEmptyBatteriesIndex = new HashSet<int>();
		var random = new Random();
		while (randomEmptyBatteriesIndex.Count < emptyBatteries)
		{
			var index = random.Next(0, MaxBatteries);
			randomEmptyBatteriesIndex.Add(index);
		}

		var randomEnergyType = new HashSet<int>();
		while (randomEnergyType.Count < limitFullBatteries)
		{
			var type = random.Next(0, Battery.MaxEnergyTypes);
			randomEnergyType.Add(type);
		}

		var totalBatteries = limitFullBatteries + emptyBatteries;
		for (var i = 0; i < totalBatteries; i++)
		{
			_batteries.Add(new Battery());
		}

		var energyTypeEnumerator = randomEnergyType.GetEnumerator();
		while (energyTypeEnumerator.MoveNext())
		{
			var type = energyTypeEnumerator.Current + 1;
			var energyToDrop = Battery.MaxEnergy;
			while (energyToDrop > 0)
			{
				var batteryIndex = random.Next(0, totalBatteries);
				if (randomEmptyBatteriesIndex.Contains(batteryIndex)) continue;

				var possibleBattery = _batteries[batteryIndex];
				if (possibleBattery.IsFull) continue;

				possibleBattery.AddEnergy(type);
				energyToDrop--;
			}
		}
	}

	public string Export()
	{
		var result = "";

		foreach (var battery in _batteries)
		{
			var energies = battery.Energies;
			var batteryStr = energies.Aggregate("", (current, type) => current + $"{type + 1:x}");

			var remain = Battery.MaxEnergy - energies.Length;
			for (var i = 0; i < remain; i++) batteryStr += "0";

			result += batteryStr;
		}

		return result;
	}
}