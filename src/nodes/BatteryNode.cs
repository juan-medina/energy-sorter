// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EnergySorter.model;
using Godot;

namespace EnergySorter.nodes;

public partial class BatteryNode : Area2D
{
	[Signal]
	public delegate void OnClickedEventHandler(BatteryNode node);

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
	private Sprite2D _bodySprite;

	private bool _enabled = true;

	public bool Enabled
	{
		get => _enabled;
		set
		{
			_enabled = value;
			_state = State.Normal;
			UpdateVisuals();
		}
	}

	public override void _Ready()
	{
		_battery = new Battery();
		for (var i = 0; i < Battery.MaxEnergy; i++) _energySprites.Add(GetNode<Sprite2D>($"Body/Energy{i + 1}"));

		_bodySprite = GetNode<Sprite2D>("Body");
	}

	public enum State
	{
		Normal,
		Selected,
		Hovered,
	}

	private State _state = State.Normal;

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
			sprite.Modulate = EnergyColors[type - 1];
			sprite.Visible = true;
		}

		var scale = 1.0f;
		if (!_battery.IsClosed)
			scale = _state switch
			{
				State.Selected => 1.5f,
				State.Hovered => 1.2f,
				_ => 1.0f
			};

		_bodySprite.Scale = new Vector2(scale, scale);
		_bodySprite.Modulate = IsClosed() ? EnergyColors[energies[0] - 1] : Colors.White;
	}

	private void OnMouseEntered()
	{
		if(!_enabled) return;
		if (_state != State.Selected)
			_state = State.Hovered;
		UpdateVisuals();
	}

	private void OnMouseExited()
	{
		if(!_enabled) return;
		if (_state != State.Selected)
			_state = State.Normal;
		UpdateVisuals();
	}

	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	private void OnInputEvent(Node viewPort, InputEvent inputEvent, int shapeIdx)
	{
		if(!_enabled) return;
		if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) return;
		EmitSignal(SignalName.OnClicked, this);
	}

	public void Select()
	{
		_state = State.Selected;
		UpdateVisuals();
	}

	public void Deselect()
	{
		_state = State.Normal;
		UpdateVisuals();
	}

	public bool IsEmpty() => _battery.IsEmpty;
	public bool IsClosed() => _battery.IsClosed;

	public bool CanGetEnergyFrom(BatteryNode other) => _battery.CanGetEnergyFrom(other._battery);

	public void TransferEnergyFrom(BatteryNode other)
	{
		_battery.TransferEnergyFrom(other._battery);

		UpdateVisuals();
		other.UpdateVisuals();
	}
}