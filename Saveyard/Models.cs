using System;
using System.Collections.Generic;

namespace Saveyard
{
    public class SaveEntry
    {
        public int Slot { get; set; }
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
    }

    public class ConfigPage
    {
        // Просто инициализируем пустой список
        public List<SaveEntry> Saves { get; set; } = new List<SaveEntry>();
    }

    public class SaveConfig
    {
        public string GameName { get; set; } = "American Dad";
        public string CategoryName { get; set; } = "Joe%";
        public string SavesDirectory { get; set; } = "C:\\Users\\admin\\Documents\\American Dad User Files";
        public string Prefix { get; set; } = "USADADsf";
        public string Postfix { get; set; } = ".sav";
        public List<ConfigPage> Pages { get; set; } = new List<ConfigPage>();
    }
}