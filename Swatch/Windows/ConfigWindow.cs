using System;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Swatch.Windows;

public class ConfigWindow : Window, IDisposable {
	private readonly Configuration _config;
	private readonly Swatch _plugin;

	public ConfigWindow(Swatch plugin) : base("Swatch Config") {
		this._config = plugin.Configuration;
		this._plugin = plugin;
	}

	public void Dispose() { }

	public override void Draw() {
		var updated = false;
		ImGui.TextWrapped("Swatch is a DTR (Server Info Bar) plugin to show the current time in Swatch's .beats format, inspired by Phantasy Star Online.");
		ImGui.TextWrapped("Each .beat is 86.4 seconds long, with 1000 .beats in a day - every region globally has the same .beat time, so you can coordinate across the web without timezone considerations.");
		ImGui.TextWrapped("You can edit the ordering of your DTR-bar in Dalamud's Server Info Bar settings.");

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		var showInternet = this._config.ShowInternetLabel;
		if (ImGui.Checkbox("Show 'internet time' label", ref showInternet)) {
			this._config.ShowInternetLabel = showInternet;
			this._plugin.SetLabel();
			updated = true;
		}

		if (updated) this._config.Save();
	}
}
