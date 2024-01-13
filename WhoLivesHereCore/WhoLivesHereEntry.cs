using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.Utilities;
using System;
using System.Collections.Generic;
using StardewModdingAPI;
using WhoLivesHere.GMCM;
using WhoLivesHereCore;

namespace WhoLivesHere
{
    internal class WhoLivesHereEntry : Mod
    {
        public IModHelper Helper ;
         private SDVLogger logger;
        private static bool Visible = false;
        private WhoLivesHereConfig config;
        private WhoLivesHereLogic whoLogic;
        public override void Entry(IModHelper helper)
        {
            logger = new SDVLogger(Monitor, helper.DirectoryPath, helper);
            Helper=helper;

            config = helper.ReadConfig<WhoLivesHereConfig>();

            GMCM_Integration.Initialize(helper, ModManifest, config);

            whoLogic = new WhoLivesHereLogic();
            whoLogic.Initialize(helper, logger,config);
        }

    }
}
