using System;
using System.Globalization;

using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using Swatch.Windows;

namespace Swatch;

public sealed class Swatch : IDalamudPlugin {
	[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static IFramework Framework { get; private set; } = null!;
	[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static IPluginLog Log { get; private set; } = null!;
	[PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;

	private readonly WindowSystem _windowSystem = new("Swatch");
	private readonly IDtrBarEntry _dtrEntry;

	private int _beats;
	private DateTime _nextUpdateAfter;

	private const string CommandName = "/swatch";
	private const string DtrTitle = "Swatch";

	public Configuration Configuration { get; init; }

	private ConfigWindow ConfigWindow { get; init; }

	public Swatch() {
		this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

		this.ConfigWindow = new ConfigWindow(this);
		this._windowSystem.AddWindow(this.ConfigWindow);
		PluginInterface.UiBuilder.Draw += this._windowSystem.Draw;
		PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigUi;

		CommandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand) {
			HelpMessage = "open swatch-dtr config"
		});

		this._dtrEntry = DtrBar.Get(DtrTitle);
		this._dtrEntry.Tooltip = ".beat time";
		this._dtrEntry.OnClick = args => this.ConfigWindow.Toggle();

		this._beats = GetBeats();
		this.SetNextUpdate();
		this.SetLabel();

		Framework.Update += this.SwatchUpdate;

		Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
	}

	private void OnCommand(string command, string args) => this.ToggleConfigUi();

	private void ToggleConfigUi() => this.ConfigWindow.Toggle();

	private void SwatchUpdate(IFramework _) {
		if (DateTime.UtcNow < this._nextUpdateAfter) return; // skip updating if we're too far from the beat change window

		var newBeat = GetBeats();
		if (newBeat <= this._beats) return; // if we get the same beat, don't fire an update

		this._beats = newBeat;
		this.SetNextUpdate();
		this.SetLabel();
	}

	private static int GetBeats() {
		var ms = GetUtcMs();
		return MsToBeats(ms);
	}

	private static int GetUtcMs() {
		var time = DateTime.UtcNow;
		time += TimeSpan.FromHours(1);
		return ((time.Hour * 60 + time.Minute) * 60 + time.Second) * 1000 + time.Millisecond;
	}

	private static int MsToBeats(int ms) => (int)Math.Floor((double)Math.Abs(ms / 86400));

	public void SetLabel() {
		var label = "";
		if (this.Configuration.ShowInternetLabel)
			label += "internet time ";
		label += "@ " + this._beats.ToString(CultureInfo.InvariantCulture);
		this._dtrEntry.Text = label;
	}

	private void SetNextUpdate() {
		var targetBeat = this._beats + 1;
		var msDiff = (targetBeat * 86400) - GetUtcMs();
		this._nextUpdateAfter = DateTime.UtcNow.AddMilliseconds(msDiff - 100); // 0.1ms wiggle room
	}

	public void Dispose() {
		// Unregister all actions to not leak anything during disposal of plugin
		PluginInterface.UiBuilder.Draw -= this._windowSystem.Draw;
		PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigUi;

		this._windowSystem.RemoveAllWindows();
		this.ConfigWindow.Dispose();

		this._dtrEntry.Remove();

		CommandManager.RemoveHandler(CommandName);
	}
}
