// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnergySorter.model;
using Godot;

namespace EnergySorter.scenes;

public partial class GeneratorScene : Node2D
{
	private struct LevelData
	{
		public string Export;
		public int Steps;
		public int FullBatteries;
		public int EmptyBatteries;
	}

	private readonly List<LevelData> _levels = [];

	private void OnGeneratorButtonUp()
	{
		GenerateLevels(150, 2, 1);
		GenerateLevels(150, 3, 2);
		GenerateLevels(150, 4, 2);
		GenerateLevels(150, 5, 2);
		GenerateLevels(150, 6, 1);
		GenerateLevels(150, 7, 2);
		GenerateLevels(150, 8, 2);
		GenerateLevels(150, 9, 2);
		GenerateLevels(150, 10, 2);

		for (var i = 0; i < _levels.Count; i++)
		{
			var level = _levels[i];
			Console.WriteLine(
				$"Level {i + 1:00}: {level.Export} Steps: {level.Steps} Full: {level.FullBatteries} Empty: {level.EmptyBatteries}");
		}

		foreach (var level in _levels) Console.WriteLine(level.Export);
	}

	private void GenerateLevels(int iterations, int fullBatteries, int emptyBatteries)
	{
		List<LevelData> stepLevel = [];
		for (var i = 1; i <= iterations; i++)
		{
			Debug.WriteLine($"Full: {fullBatteries} Empty: {emptyBatteries} - Generated level {i}/{iterations}");
			var puzzle = new Puzzle(fullBatteries, emptyBatteries);
			if (puzzle.IsSolved || puzzle.ContainsClosedBattery)
			{
				i--;
				continue;
			}

			var export = puzzle.Export();
			var steps = puzzle.Solve();
			var level = new LevelData
			{
				Export = export,
				Steps = steps,
				FullBatteries = fullBatteries,
				EmptyBatteries = emptyBatteries,
			};
			stepLevel.Add(level);
		}

		stepLevel.Sort((a, b) => a.Steps.CompareTo(b.Steps));
		var byStep = -1;
		foreach (var level in stepLevel.Where(level => level.Steps != byStep))
		{
			_levels.Add(level);
			byStep = level.Steps;
		}
	}
}