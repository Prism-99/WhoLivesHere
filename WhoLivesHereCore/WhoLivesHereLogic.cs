using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.Utilities;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Buildings;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using WhoLivesHereCore;
#if !v16
using Rectangle = Microsoft.Xna.Framework.Rectangle;
#endif


namespace WhoLivesHere
{
    /// <summary>
    /// Entry module
    /// </summary>
    internal class WhoLivesHereLogic
    {
        private static bool Visible = false;
        private SDVLogger logger;
        private KeybindList toggleKey;
        private static Texture2D notHomeTexture;
        private static Texture2D notFedTexture;
        private static Texture2D unoccupiedTexture;
        private WhoLivesHereConfig config;
        private readonly static Dictionary<string, Tuple<Rectangle, Texture2D>> animalCache = new Dictionary<string, Tuple<Rectangle, Texture2D>>();
        public void Initialize(IModHelper helper, SDVLogger ologger, WhoLivesHereConfig oConfig)
        {
            logger = ologger;
            config = oConfig;
            //
            //  add button hook to toggle display
            //
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            //
            //  apply required harmony patches
            //
            VersionLevel.ApplyPatches(helper.ModRegistry.ModID, ologger);
            //
            //  set toggle key
            //
            toggleKey = config.ToggleKey;
            //
            //  set assorted background colours
            //
            //  not at home
            Color[] colors = new Color[] { Color.SandyBrown };
            notHomeTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            notHomeTexture.SetData<Color>(colors);
            //  not fed
            colors = new Color[] { Color.PaleVioletRed };
            notFedTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            notFedTexture.SetData<Color>(colors);
            //  unoccupied
            colors = new Color[] { Color.MediumPurple };
            unoccupiedTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            unoccupiedTexture.SetData<Color>(colors);
        }
        /// <summary>
        /// Check if toggle key was pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (toggleKey.JustPressed())
            {
                Visible = !Visible;
                logger.Log($"WhoLivesHere toggled, visible:  {Visible}", LogLevel.Debug);
            }
        }
        /// <summary>
        /// Draw building inhabitants on buiding image
        /// </summary>
        /// <param name="__instance">Building to be drawn</param>
        /// <param name="b">SpriteBatch from the game</param>
        internal static void BuildingDraw_Suffix(Building __instance, SpriteBatch b)
        {
            if (Visible)
            {
                //
                //  verify building has an indoors
                //
                if (__instance.indoors.Value != null)
                {
                    int iPointer = 0;
                    int columns = 6;
                    //
                    //  check for an AnimalHouse
                    //
                    if (__instance.indoors.Value is AnimalHouse house)
                    {
                        int topX = 60;
                        int topY = -120;
                        //
                        //  adjust display coordinates for a Coop
                        if (house.Name.Contains("Coop", StringComparison.CurrentCultureIgnoreCase))
                        {
                            topX = 30;
                            topY = -170;
                        }
                        //
                        //  loop through the house animals and draw their
                        //  image above their home building
                        //
                        foreach (var animalId in house.animalsThatLiveHere.OrderBy(p => p))
                        {
                            bool isHome = true;

                            string animalType = string.Empty;
                            FarmAnimal animal;
                            //
                            //  look for the animal in the building
                            if (house.animals.TryGetValue(animalId, out animal))
                                animalType = animal.type.Value;
                            //
                            //  look for the animal outdoors
                            else if (house.GetParentLocation()?.animals.TryGetValue(animalId, out animal)??false)
                            {
                                animalType = animal.type.Value;
                                isHome = false;
                            }

                            if (!string.IsNullOrEmpty(animalType))
                            {

                                if (TryGetGetAnimalImage(animalType, out Texture2D animalImage, out Rectangle sourceRectangle))
                                {
                                    //
                                    //  set offset from building location to top corner of
                                    //  animal display
                                    //
                                    int xOffset = (iPointer == 0 ? 0 : iPointer % columns) * 70;
                                    int yOffset = (iPointer == 0 ? 0 : iPointer / columns) * 70;

                                    if (animalImage == null)
                                    {
                                        //
                                        //  if we did not get an image, display text
                                        //  of the missing animal type
                                        //
                                        b.DrawString(Game1.dialogueFont, animalType, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)__instance.tileX.Value * 64 + topX + xOffset, (int)__instance.tileY.Value * 64 + topY + yOffset)), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                                    }
                                    else
                                    {
                                        int iScale = sourceRectangle.Height == 16 ? 3 : 2;
                                        Vector2 rotationOrigin = new Vector2(sourceRectangle.Width / 2f, sourceRectangle.Height / 2f);
                                        //
                                        //  draw colour status boxes
                                        //
                                        //
                                        //  calculate height of colour box(es)
                                        //
                                        int splits = 0;
                                        if (!VersionLevel.WasFeed(animal))
                                            splits++;
                                        if (!isHome)
                                            splits++;

                                        int splitIndex = 0;
                                        int splitHeight = splits == 0 ? sourceRectangle.Height : sourceRectangle.Height / splits;
                                        //
                                        //  based on # of data points to be display
                                        //  set the colour box height
                                        //
                                        Rectangle backgroundBox;
                                        if (splits > 1)
                                        {
                                            backgroundBox = new Rectangle( 0, 0, sourceRectangle.Width, splitHeight);
                                        }
                                        else
                                        {
                                            backgroundBox = sourceRectangle;
                                        }
                                        if (!VersionLevel.WasFeed(animal))
                                        {
                                            b.Draw(notFedTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)__instance.tileX.Value * 64 + topX + xOffset, (int)__instance.tileY.Value * 64 + topY + yOffset)), backgroundBox, Color.White, 0, rotationOrigin, iScale, SpriteEffects.None, 0.99f);
                                            splitIndex++;
                                        }
                                        if (!isHome)
                                        {
                                            //
                                            //  calculate top position of the
                                            //  colour box
                                            //
                                            int splitDrop = 0;
                                            if (splitIndex > 0)
                                            {
                                                splitDrop = splitHeight* (splitIndex+1);
                                            }
                                            b.Draw(notHomeTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)__instance.tileX.Value * 64 + topX + xOffset, (int)__instance.tileY.Value * 64 + topY + yOffset+ splitDrop)), backgroundBox, Color.White, 0, rotationOrigin, iScale, SpriteEffects.None, 0.99f);
                                        }
                                        //
                                        //  draw animal portrait
                                        //
                                        b.Draw(animalImage, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)__instance.tileX.Value * 64 + topX + xOffset, (int)__instance.tileY.Value * 64 + topY + yOffset)), sourceRectangle, Color.White, 0, rotationOrigin, iScale, SpriteEffects.None, 1);
                                    }
                                    iPointer++;
                                }
                            }
                        }
                        //
                        //  add status boxes for empty house slots
                        //
                        if (house.animalsThatLiveHere.Count < VersionLevel.MaxCapacity(__instance))
                        {
                            int placeHolderSize = 32;
                            Vector2 rotationOrigin = new Vector2(placeHolderSize/2, placeHolderSize/2);
                            for (int filler= house.animalsThatLiveHere.Count; filler< VersionLevel.MaxCapacity(__instance); filler++)
                            {
                                int xOffset = (iPointer == 0 ? 0 : iPointer % columns) * 70;
                                int yOffset = (iPointer == 0 ? 0 : iPointer / columns) * 70;

                                b.Draw(unoccupiedTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)__instance.tileX.Value * 64 + topX + xOffset, (int)__instance.tileY.Value * 64 + topY + yOffset )), new Rectangle(0,0, placeHolderSize, placeHolderSize), Color.White, 0, rotationOrigin, 2, SpriteEffects.None, 0.99f);
                               
                                iPointer++;
                            }
                        }
                    }
                }
            }
        }

        //private static bool TryGetAnimalPortraitDetails(string animalType, out Point portraitSize, out string textName)
        //{
        //    portraitSize = Point.Zero;
        //    textName = null;
        //    Dictionary<string, string> animal_data = Game1.content.Load<Dictionary<string, string>>("Data/FarmAnimals");

        //    if (animal_data.TryGetValue(animalType, out var animalData))
        //    {
        //        int.TryParse(animalData.Split("/")[16], out int SpriteWidth);
        //        int.TryParse(animalData.Split("/")[17], out int SpriteHeight);
        //        portraitSize = new Point(SpriteWidth, SpriteHeight);
        //        textName = "Animals/" + animalType;
        //        return true;
        //    }

        //    return false;
        //}
        /// <summary>
        /// Get the animal spritesheet
        /// </summary>
        /// <param name="animalType">Requested type of animal</param>
        /// <param name="spriteSheet">Return SpriteSheet</param>
        /// <param name="sourceRectangle">Return Source Rectangle of the image</param>
        /// <returns>True if the animal spritessheet is found</returns>
        private static bool TryGetGetAnimalImage(string animalType, out Texture2D ?spriteSheet, out Rectangle sourceRectangle)
        {
            spriteSheet =null;
            sourceRectangle = Rectangle.Empty;

            if (string.IsNullOrEmpty(animalType)) return false;
            //
            // check for cached result
            //
            if (animalCache.TryGetValue(animalType, out Tuple<Rectangle, Texture2D>? textureDetails))
            {                
                spriteSheet = textureDetails.Item2;
                sourceRectangle = textureDetails.Item1;
                return true;
            }

            if (VersionLevel.TryGetAnimalPortraitDetails(animalType, out Point imageDimensions, out string spriteSheetName))
            {
                try
                {
                    Texture2D animalSpriteSheet = Game1.content.Load<Texture2D>(spriteSheetName);

                    if (animalSpriteSheet == null)
                    {
                        //StardewLogger.LogWarning("GetAnimalImage", $"Could not find image for a '{sAnimalType}'");
                    }
                    else
                    {
                        int height = 16;
                        int width = 16;
                        //
                        //  get image dimensions
                        //
                        if (imageDimensions != Point.Zero)
                        {
                            height = imageDimensions.Y;
                            width = imageDimensions.X;
                        }
                        //
                        //  add results to the cache
                        animalCache.Add(animalType, Tuple.Create(new Rectangle(0, 0, width, height), animalSpriteSheet));
                        //
                        //  set return values
                        //
                        spriteSheet = animalSpriteSheet;
                        sourceRectangle = new Rectangle(0, 0, width, height);
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }
    }
}
