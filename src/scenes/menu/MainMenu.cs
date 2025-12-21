// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Godot;

namespace EnergySorter.scenes.menu;

public partial class MainMenu : Control
{
	private const string SettingsPath = "user://settings.cfg";

	private MenuScene _menuScene;
	private bool _devMode;

	private Control _devSeparation;
	private Button _devButton;


	public override void _Ready()
	{
		Debug.Assert(GetParent() != null, "GetParent() != null");
		Debug.Assert(GetParent().GetParent() != null, "GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() != null, "GetParent().GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() is MenuScene,
			"GetParent().GetParent().GetParent() is not MenuScene");

		_menuScene = GetParent().GetParent().GetParent<MenuScene>();
		_devSeparation = GetNode<Control>("ButtonContainer/DevSeparation");
		_devButton = GetNode<Button>("ButtonContainer/DevButton");

		var settings = new ConfigFile();
		var err = settings.Load(SettingsPath);
		if (err != Error.Ok)
		{
			GD.PushError($"Could not load settings file at {SettingsPath}: {err}");
			return;
		}

		_devMode = settings.GetValue("dev", "mode", false).AsBool();
		_devSeparation.Visible = _devMode;
		_devButton.Visible = _devMode;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		var settings = new ConfigFile();
		settings.SetValue("dev", "mode", _devMode);
		settings.Save(SettingsPath);
	}

	private void OnPlayButtonUp()
	{
		_menuScene.GotoLevelSelection();
	}

	private void OnDevButtonUp()
	{
		_menuScene.GotoDevMenu();
	}
}