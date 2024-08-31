using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace Brandy
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public List<MarkInfo> MarkInfos { get; set; }

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            if (MarkInfos == null)
            {
                MarkInfos = new List<MarkInfo>()
                {
                    new MarkInfo()
                    {
                        Name = "Default"
                    }
                };
            }
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
