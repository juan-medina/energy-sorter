using System;
using Godot;

namespace EnergySorter.globals;

public partial class Fader : Control
{
	private enum State
	{
		None,
		In,
		Out
	}

	private State _state = State.None;

	[Signal]
	public delegate void FadeOutCompletedEventHandler();

	public static Fader Instance { get; private set; }

	public override void _Ready() => Instance = this;

	private float _fadeTime;
	private float _alpha;
	private float _alphaTarget;
	private float _elapsed;

	public override void _Process(double delta)
	{
		if (!Visible) return;

		_elapsed += (float)delta;
		var current = Modulate;
		_alpha = _state == State.Out ? _elapsed / _fadeTime : _alpha = 1 - _elapsed / _fadeTime;

		current.A = Math.Clamp(_alpha, 0f, 1.0f);
		if (current.A == _alphaTarget)
		{
			_elapsed = 0;
			if (_state == State.Out)
			{
				_state = State.In;
				_alphaTarget = 0;
				EmitSignal(SignalName.FadeOutCompleted);
			}
			else
			{
				_state = State.None;
				Visible = false;
			}
		}

		Modulate = current;
	}

	public SignalAwaiter OutIn(float fadeTime = 0.5f)
	{
		Visible = true;
		Modulate = new Color(0, 0, 0, 0);
		_fadeTime = fadeTime;

		_state = State.Out;
		_alpha = 0;
		_alphaTarget = 1;
		_elapsed = 0;

		return ToSignal(this, SignalName.FadeOutCompleted);
	}
}