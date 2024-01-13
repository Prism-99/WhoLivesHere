using StardewModdingAPI.Utilities;
using StardewModdingAPI;

namespace WhoLivesHereCore
{
    internal class WhoLivesHereConfig
    {
        public KeybindList ToggleKey { get; set; } = new KeybindList(new Keybind(SButton.LeftControl,SButton.N), new Keybind(SButton.RightControl,SButton.N));

    }
}
