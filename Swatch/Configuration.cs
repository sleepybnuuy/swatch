using Dalamud.Configuration;

using System;

namespace Swatch;

[Serializable]
public class Configuration : IPluginConfiguration {
	public int Version { get; set; } = 0;
	public bool ShowInternetLabel { get; set; } = false;

	// The below exists just to make saving less cumbersome
	public void Save() {
		Swatch.PluginInterface.SavePluginConfig(this);
	}
}
