using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace EnergySorter.model;

public class Puzzle
{
	private const int MaxBatteries = 12;

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

	private readonly List<Battery> _batteries = [];
	public Battery[] Batteries => _batteries.ToArray();

	public Puzzle(int fullBatteries, int emptyBatteries)
	{
		Debug.Assert(fullBatteries + emptyBatteries <= MaxBatteries,
			$"A puzzle can not have more than {MaxBatteries} batteries");

		var limitFullBatteries = Math.Min(fullBatteries, MaxBatteries - emptyBatteries);

		Debug.Assert(limitFullBatteries <= EnergyColors.Length,
			$"A puzzle can not have more than {EnergyColors.Length} full batteries with unique colors");

		limitFullBatteries = Math.Min(limitFullBatteries, EnergyColors.Length);

		var randomEmptyBatteriesIndex = new HashSet<int>();
		var random = new Random();
		while (randomEmptyBatteriesIndex.Count < emptyBatteries)
		{
			var index = random.Next(0, MaxBatteries);
			randomEmptyBatteriesIndex.Add(index);
		}

		var randomColorsIndex = new HashSet<int>();
		while (randomColorsIndex.Count < limitFullBatteries)
		{
			var index = random.Next(0, EnergyColors.Length);
			randomColorsIndex.Add(index);
		}

		var totalBatteries = limitFullBatteries + emptyBatteries;
		for (var i = 0; i < totalBatteries; i++)
		{
			_batteries.Add(new Battery());
		}

		var colorsEnumerator = randomColorsIndex.GetEnumerator();
		while (colorsEnumerator.MoveNext())
		{
			var index = colorsEnumerator.Current;
			var energyToDrop = Battery.MaxEnergy;
			while (energyToDrop > 0)
			{
				var batteryIndex = random.Next(0, totalBatteries);
				if (randomEmptyBatteriesIndex.Contains(batteryIndex))
				{
					continue;
				}

				var possibleBattery = _batteries[batteryIndex];
				if (possibleBattery.IsFull)
				{
					continue;
				}

				possibleBattery.AddEnergy(EnergyColors[index]);
				energyToDrop--;
			}
		}
	}
}