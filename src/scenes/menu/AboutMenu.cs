using System.Diagnostics;
using System.Text.RegularExpressions;
using Godot;

namespace EnergySorter.scenes.menu;

public partial class AboutMenu : Control
{
	private MenuScene _menuScene;

	public override void _Ready()
	{
		Debug.Assert(GetParent() != null, "GetParent() != null");
		Debug.Assert(GetParent().GetParent() != null, "GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() != null, "GetParent().GetParent().GetParent() != null");
		Debug.Assert(GetParent().GetParent().GetParent() is MenuScene,
			"GetParent().GetParent().GetParent() is not MenuScene");

		_menuScene = GetParent().GetParent().GetParent<MenuScene>();
	}

	private void OnBackButtonUp()
	{
		_menuScene.BackToMainMenu();
	}

	[GeneratedRegex(
		@"^(https?:\/\/)((([a-zA-Z0-9\-_]+\.)+[a-zA-Z]{2,})|localhost|\d{1,3}(\.\d{1,3}){3})(:\d+)?(\/\S*)?$",
		RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
	private static partial Regex UrlRegex();

	private static void OnRichTextLabelMetaClicked(Variant meta)
	{
		var url = meta.AsString();
		if (string.IsNullOrEmpty(url)) return;
		if (!UrlRegex().IsMatch(url)) return;
		OS.ShellOpen(url);
	}
}