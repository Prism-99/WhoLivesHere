using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using WhoLivesHereCore;

namespace WhoLivesHere.GMCM
{
    internal static class GMCM_Integration
    {
        private static IModHelper helper;
        private static IManifest manifest;
        private static WhoLivesHereConfig config;
        private static void ResetGuiVars()
        {
            //
            //  reset the exposed config items to their
            //  default values.
            //
            config.ToggleKey = new KeybindList(new Keybind(SButton.LeftControl, SButton.N), new Keybind(SButton.RightControl, SButton.N));
        }
        public static void Initialize(IModHelper ohelper, IManifest omanifest, WhoLivesHereConfig oconfig)
        {
            helper = ohelper;
            manifest = omanifest;
            config = oconfig;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: manifest,
                reset: ResetGuiVars,
                save: () => helper.WriteConfig(config),
                 titleScreenOnly: false
            );
            //
            //  create config GUI
            //
             configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Who Lives Here",
                tooltip: () => ""
            );
            configMenu.AddKeybindList(
              mod: manifest,
              name: () => "Toggle Key",
              tooltip: () => "Key to toggle occupant display",
               getValue: () => config.ToggleKey,
               setValue: value => config.ToggleKey = value

           );
        }
    }
}
