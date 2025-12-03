// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Godot;

namespace EnergySorter.model;

public class Battery
{
	private enum State
	{
		Empty,
		Full,
		Closed,
	}

	public const int MaxEnergy = 4;

	private readonly List<Color> _energies = new(MaxEnergy);
	private State _currentState  = State.Empty;

	public Color[] Energies => [.. _energies];

	public void AddEnergy(Color color) => _energies.Add(color);
}
