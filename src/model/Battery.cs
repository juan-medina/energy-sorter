// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Diagnostics;

namespace EnergySorter.model;

public class Battery
{
	public const int MaxEnergyTypes = 10;

	private enum State
	{
		Empty,
		Full,
		Closed,
	}

	public const int MaxEnergy = 4;

	private readonly List<int> _energies = new(MaxEnergy);
	private State _currentState = State.Empty;

	public int[] Energies => [.. _energies];

	public bool IsFull => _currentState == State.Full;

	public void AddEnergy(int type)
	{
		Debug.Assert(type > 0 & type <= MaxEnergyTypes,
			$"A battery can only accept energy types between 1 and {MaxEnergyTypes}, value: {type}");

		Debug.Assert(_currentState != State.Closed,
			"A closed battery can not accept more energy");

		Debug.Assert(_currentState != State.Full, "A full battery can not accept more energy");

		_energies.Add(type);

		if (_energies.Count < MaxEnergy) return;
		var firstType = _energies[0];
		_currentState = _energies.TrueForAll(x => x == firstType) ? State.Closed : State.Full;
	}
}