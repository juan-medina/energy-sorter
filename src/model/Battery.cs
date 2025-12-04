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
		Normal,
		Empty,
		Full,
		Closed,
	}

	public const int MaxEnergy = 4;

	private readonly List<int> _energies = new(MaxEnergy);
	private State _currentState = State.Empty;

	public int[] Energies => [.. _energies];

	public bool IsFull => _currentState == State.Full;
	public bool IsClosed => _currentState == State.Closed;
	public bool IsEmpty => _currentState == State.Empty;

	public Battery Clone()
	{
		var clone = new Battery();
		foreach (var energy in _energies) clone.AddEnergy(energy);
		return clone;
	}

	public void AddEnergy(int type)
	{
		Debug.Assert(type > 0 & type <= MaxEnergyTypes,
			$"A battery can only accept energy types between 1 and {MaxEnergyTypes}, value: {type}");

		Debug.Assert(_currentState != State.Closed,
			"A closed battery can not accept more energy");

		Debug.Assert(_currentState != State.Full, "A full battery can not accept more energy");

		_energies.Add(type);

		if (_energies.Count < MaxEnergy)
		{
			_currentState = State.Normal;
		}
		else
		{
			var firstType = _energies[0];
			_currentState = _energies.TrueForAll(x => x == firstType) ? State.Closed : State.Full;
		}
	}

	public void RemoveLastEnergy()
	{
		Debug.Assert(_energies.Count > 0, "Can't remove energy from an empty battery");
		Debug.Assert(_currentState != State.Closed, "Can't remove energy from a closed battery");

		_energies.RemoveAt(_energies.Count - 1);

		_currentState = State.Normal;
		if (_energies.Count == 0) _currentState = State.Empty;
	}

	public int[] GetTopEnergy()
	{
		if (_energies.Count == 0) return [];

		var lastEnergy = _energies[^1];
		var topEnergies = new List<int> { lastEnergy };

		for (var i = _energies.Count - 2; i >= 0; i--)
			if (_energies[i] == lastEnergy)
				topEnergies.Add(lastEnergy);

		return topEnergies.ToArray();
	}

	public bool CanGetEnergyFrom(Battery other)
	{
		// can't get energy if this battery is closed or full
		if (IsClosed || IsFull) return false;

		// edge case: other battery has no energy what send to this
		var otherTopEnergies = other.GetTopEnergy();
		Debug.Assert(otherTopEnergies.Length > 0, $"Can't get energy from battery with no energy");
		if (otherTopEnergies.Length == 0) return false;

		// we are empty, we can receive any energy
		if (_energies.Count == 0) return true;

		// the battery has not enough space to receive the energy
		if (_energies.Count + otherTopEnergies.Length > MaxEnergy) return false;

		// can receive only if the top energies are the same type
		var topEnergies = GetTopEnergy();
		return topEnergies[0] == otherTopEnergies[0];
	}

	public void TransferEnergyFrom(Battery other)
	{
		Debug.Assert(CanGetEnergyFrom(other), "TransferEnergyFrom: Can't transfer energy from the other battery");

		var otherTopEnergies = other.GetTopEnergy();
		foreach (var energy in otherTopEnergies)
		{
			AddEnergy(energy);
			other.RemoveLastEnergy();
		}
	}
}