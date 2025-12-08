// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using Godot;

namespace EnergySorter.nodes;

public partial class Spark : AnimatedSprite2D
{
	private const int Velocity = 250;
	private const float RotationSpeed = 2.5f;
	private AudioStreamPlayer2D _zapSound;

	private Vector2I _destination;

	public Vector2I Origin
	{
		set => GlobalPosition = value;
	}

	public Vector2I Destination
	{
		set => _destination = value;
	}

	public override void _Ready()
	{
		Play();
		_zapSound = GetNode<AudioStreamPlayer2D>("Zap");
		_zapSound.Play();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Rotation += RotationSpeed * (float)delta;
		Position = Position.MoveToward(new Vector2(_destination.X, _destination.Y), Velocity * (float)delta);
		if (Position.DistanceTo(new Vector2(_destination.X, _destination.Y)) < 10) QueueFree();
	}
}