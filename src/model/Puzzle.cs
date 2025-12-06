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

	private Puzzle()
	{
	}

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
			var index = random.Next(0, limitFullBatteries + emptyBatteries);
			randomEmptyBatteriesIndex.Add(index);
		}


		var totalBatteries = limitFullBatteries + emptyBatteries;
		for (var i = 0; i < totalBatteries; i++)
		{
			_batteries.Add(new Battery());
		}

		for (var type = 1; type <= limitFullBatteries; type++)
		{
			var energyToDrop = Battery.MaxEnergy;
			while (energyToDrop > 0)
			{
				var batteryIndex = random.Next(0, totalBatteries);
				if (randomEmptyBatteriesIndex.Contains(batteryIndex)) continue;

				var possibleBattery = _batteries[batteryIndex];
				if (possibleBattery.IsFull || possibleBattery.IsClosed) continue;

				possibleBattery.AddEnergy(type);
				energyToDrop--;
			}
		}
	}

	public Puzzle Clone()
	{
		var clone = new Puzzle();
		foreach (var battery in _batteries) clone._batteries.Add(battery.Clone());
		return clone;
	}

	public static Puzzle Import(string data)
	{
		Debug.Assert(data.Length % Battery.MaxEnergy == 0,
			"The puzzle data length must be a multiple of the battery max energy.");

		var puzzle = new Puzzle();

		var batteries = data.Select((c, i) => new { Char = c, Index = i })
			.GroupBy(x => x.Index / Battery.MaxEnergy)
			.Select(g => new string(g.Select(x => x.Char).ToArray()));

		foreach (var batteryData in batteries)
		{
			var battery = new Battery();

			foreach (var type in batteryData
				         .Select(ch => Convert.ToInt32(ch.ToString(), 16))
				         .Where(type => type > 0))
				battery.AddEnergy(type);

			puzzle._batteries.Add(battery);
		}

		return puzzle;
	}

	public string Export()
	{
		var sb = new System.Text.StringBuilder();

		foreach (var energies in _batteries.Select(battery => battery.Energies.Where(t => t > 0).ToArray()))
		{
			foreach (var type in energies) sb.Append(type.ToString("x"));

			var remain = Math.Max(0, Battery.MaxEnergy - energies.Length);
			for (var i = 0; i < remain; i++) sb.Append('0');
		}

		return sb.ToString();
	}


	public bool IsSolved => _batteries.All(b => b.IsEmpty || b.IsClosed);

	public bool HasMoreMoves =>
		// Summary: It checks if we are able to move any energy from one battery to another
		_batteries.Where(source => !source.IsEmpty && !source.IsClosed).Any(source =>
			_batteries.Where(target => !ReferenceEquals(target, source) && !target.IsClosed && !target.IsFull)
				.Any(target => target.CanGetEnergyFrom(source)));


	public int Solve()
	{
		var best = int.MaxValue;
		var visited = new Dictionary<string, int>();

		// start from a clone of the current puzzle
		var start = Clone();
		if (start.IsSolved) return 0;

		SolveRecursive(start, 0, ref best, visited);
		return best == int.MaxValue ? -1 : best;
	}

	private static void SolveRecursive(Puzzle state, int depth, ref int best, Dictionary<string, int> visited)
	{
		// prune if we already have a better or equal solution
		if (depth >= best) return;

		// use Export() as the canonical state key
		var key = state.Export();
		if (visited.TryGetValue(key, out var prevDepth) && prevDepth <= depth) return;
		visited[key] = depth;

		if (state.IsSolved)
		{
			if (depth < best) best = depth;
			return;
		}

		var n = state._batteries.Count;
		for (var src = 0; src < n; src++)
		{
			var from = state._batteries[src];
			if (from.IsEmpty || from.IsClosed) continue;

			for (var dst = 0; dst < n; dst++)
			{
				if (src == dst) continue;

				var to = state._batteries[dst];
				if (to.IsFull || to.IsClosed) continue;
				if (!to.CanGetEnergyFrom(from)) continue;

				var next = state.Clone();
				next._batteries[dst].TransferEnergyFrom(next._batteries[src]);

				SolveRecursive(next, depth + 1, ref best, visited);
			}
		}
	}

	public bool ContainsClosedBattery => _batteries.Any(b => b.IsClosed);
}