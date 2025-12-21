// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EnergySorter.model;
using Godot;

namespace EnergySorter.scenes;

public partial class GeneratorScene : Node2D
{
	private struct LevelData
	{
		public string Export;
		public int Steps;
		public int Batteries;
		public int Energy;
	}

	private readonly List<LevelData> _levels = [];

	private void OnGeneratorButtonUp()
	{
		/*
		GenerateLevels(1600, 3, 2);
		GenerateLevels(1600, 4, 2);

		GenerateLevels(1600, 4, 3);
		GenerateLevels(1600, 5, 3);

		GenerateLevels(1600, 5, 4);
		GenerateLevels(1600, 6, 4);

		GenerateLevels(1600, 6, 5);
		GenerateLevels(1600, 7, 5);

		GenerateLevels(750, 7, 6);
		GenerateLevels(750, 8, 6);

		GenerateLevels(250, 8, 7);
		GenerateLevels(250, 9, 7);

		GenerateLevels(150, 10, 8);
		GenerateLevelWithPairs(50, 11, 9);
		*/

		GenerateLevelWithPairs(50, 12, 10);

		for (var i = 0; i < _levels.Count; i++)
		{
			var level = _levels[i];
			Console.WriteLine(
				$"Level {i + 1:00}: {level.Export} Steps: {level.Steps} Batteries: {level.Batteries} Energy: {level.Energy}");
		}

		foreach (var level in _levels)
		{
			Console.WriteLine(level.Export + "-" + level.Steps);
		}
	}

	private void GenerateLevelWithPairs(int iterations, int batteries, int energies)
	{
		var generatedLevels = new HashSet<string>();

		List<LevelData> stepLevel = [];

		var halfBatteries = batteries / 2;
		var halfEnergies = energies / 2;

		var remainingBatteries = batteries - halfBatteries;
		var remainingEnergies = energies - halfEnergies;

		for (var i = 1; i <= iterations; i++)
		{
			Debug.WriteLine($"Batteries: {batteries} Energies: {energies} - Generated level {i}/{iterations}");

			var puzzle1 = new Puzzle(halfBatteries, halfEnergies);
			if (puzzle1.IsSolved || puzzle1.ContainsClosedBattery)
			{
				i--;
				continue;
			}

			var puzzle2 = new Puzzle(remainingBatteries, remainingEnergies);
			if (puzzle2.IsSolved || puzzle2.ContainsClosedBattery)
			{
				i--;
				continue;
			}

			var steps1 = puzzle1.Solve();
			var steps2 = puzzle2.Solve();

			if (steps1 < 0 || steps2 < 0)
			{
				i--;
				continue;
			}

			puzzle2.ShiftEnergyType(halfEnergies);

			var puzzle = puzzle1 + puzzle2;
			puzzle.Sort();
			var export = puzzle.Export();


			if (!generatedLevels.Add(export)) continue;

			var level = new LevelData
			{
				Export = export,
				Steps = steps1 + steps2,
				Batteries = batteries,
				Energy = energies,
			};
			stepLevel.Add(level);
		}

		stepLevel.Sort((a, b) => a.Steps.CompareTo(b.Steps));
		_levels.AddRange(stepLevel);
	}

	private void GenerateLevels(int iterations, int batteries, int energies)
	{
		List<LevelData> stepLevel = [];
		for (var i = 1; i <= iterations; i++)
		{
			Debug.WriteLine($"Batteries: {batteries} Energies: {energies} - Generated level {i}/{iterations}");
			var puzzle = new Puzzle(batteries, energies);
			if (puzzle.IsSolved || puzzle.ContainsClosedBattery)
			{
				i--;
				continue;
			}

			puzzle.Sort();
			var export = puzzle.Export();
			var steps = puzzle.Solve();
			var level = new LevelData
			{
				Export = export,
				Steps = steps,
				Batteries = batteries,
				Energy = energies,
			};
			stepLevel.Add(level);
		}

		stepLevel.Sort((a, b) => a.Steps.CompareTo(b.Steps));
		_levels.AddRange(stepLevel);
	}
}