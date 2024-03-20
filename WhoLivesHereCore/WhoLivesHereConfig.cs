using StardewModdingAPI.Utilities;
using StardewModdingAPI;

namespace WhoLivesHereCore
{
    internal class WhoLivesHereConfig
    {
        public KeybindList ToggleKey { get; set; } = new KeybindList(new Keybind(SButton.LeftControl,SButton.N), new Keybind(SButton.RightControl,SButton.N));
        public int AutoOnTime { get; set; } = 0;
        public int AutoOffTime { get; set;} = 0;
        public int PageDelay { get; set; } = 250;
        public bool HideEmptyTabs { get; set; } = true;
        public bool ShowAnimalCount { get; set; } = false;
        public bool ShowMissingHay { get; set; } = true;
    }
}
