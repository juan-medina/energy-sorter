// SPDX-FileCopyrightText: 2025 Juan Medina
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace EnergySorter.globals;

public partial class LevelManager : Node
{
	private int _currentLevel = 1;
	private int _unlockedLevel = 1;
	private const string LevelsPath = "res://levels/levels.txt";
	private const string SaveFile = "user://save.cfg";

	public static LevelManager Instance { get; private set; }

	private List<string> Levels { get; } = [];

	public int CurrentLevel
	{
		get => _currentLevel;
		set
		{
			var previousLevel = _currentLevel;
			_currentLevel = Math.Clamp(value, 1, TotalLevels);
			if (_currentLevel > UnlockedLevel)
			{
				UnlockedLevel = _currentLevel;
				Save();
			}
			else if (previousLevel != _currentLevel) Save();
		}
	}

	public int UnlockedLevel
	{
		get => _unlockedLevel;
		private set => _unlockedLevel = Math.Clamp(value, 1, TotalLevels);
	}

	public int TotalLevels => Levels.Count;
	public bool IsInitialized { get; private set; }


	public override void _Ready()
	{
		Instance = this;
		Load();
	}

	private void LoadLevels()
	{
		Levels.Clear();

		if (!FileAccess.FileExists(LevelsPath))
		{
			GD.PushError($"Levels file not found at path: {LevelsPath}");
			IsInitialized = false;
			return;
		}

		try
		{
			using var file = FileAccess.Open(LevelsPath, FileAccess.ModeFlags.Read);
			var text = file.GetAsText();
			var entries = text.Split('\n');
			foreach (var e in entries)
			{
				var trimmed = e.Trim();
				if (trimmed.Length > 0)
					Levels.Add(trimmed);
			}

			Debug.Assert(Levels.Count > 0, "No levels were loaded from the levels file.");
			IsInitialized = Levels.Count > 0;
		}
		catch (Exception ex)
		{
			GD.PushError($"LoadLevels error: {ex}");
			IsInitialized = false;
		}
	}

	public string GetCurrentLevelData() => GetLevelData(CurrentLevel);

	private string GetLevelData(int number)
	{
		if (number > 0 && number <= TotalLevels) return Levels[number - 1];
		GD.PushError($"GetLevelData: requested level {number} out of range (1..{TotalLevels})");
		return string.Empty;
	}

	public void NextLevel()
	{
		if (CurrentLevel < TotalLevels) CurrentLevel++;
	}

	private void Load()
	{
		LoadLevels();

		var config = new ConfigFile();
		var err = config.Load(SaveFile);
		if (err != Error.Ok && err != Error.FileNotFound)
		{
			GD.PushError($"NextLevel: failed to load max level from {SaveFile}, error: {err}");
			return;
		}

		UnlockedLevel = config.GetValue("levels", "unlocked-level", 1).AsInt32();
		CurrentLevel = config.GetValue("levels", "current-level", 1).AsInt32();
	}

	private void Save()
	{
		var config = new ConfigFile();
		config.SetValue("levels", "unlocked-level", UnlockedLevel);
		config.SetValue("levels", "current-level", CurrentLevel);
		var err = config.Save(SaveFile);
		if (err != Error.Ok)
		{
			GD.PushError($"NextLevel: failed to save max level to {SaveFile}, error: {err}");
		}
	}

	public bool IsLastLevel() => CurrentLevel >= TotalLevels;
}