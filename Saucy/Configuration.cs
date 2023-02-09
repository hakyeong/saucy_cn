using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Saucy
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool UseRecommendedDeck { get; set; } = false;

        public int SelectedDeckIndex { get; set; } = -1;

        public Stats Stats { get; set; } = new Stats();

        public bool PlaySound { get; set; } = false;
        public string SelectedSound { get; set; } = "Moogle";

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}
