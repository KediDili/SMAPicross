using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using xTile;
using System.Linq;
using System.Reflection.Emit;

namespace Picrosser
{
    public class ModEntry : Mod
    {
        internal static Dictionary<string, Picross> ValidPicrosses = new();
        internal static PicrossProgress Progress = new();
        internal static new IModHelper Helper;
        internal static Config ModConfig = new();
        internal static IGenericModConfigMenu GMCMAPI;

        public override void Entry(IModHelper helper)
        {
            Helper = helper;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Player.Warped += OnWarped;
            Helper.ConsoleCommands.Add("thepicrosser", "start the picrosser", StartThePicrosser);
            ModConfig = Helper.ReadConfig<Config>();
        }
        private void StartThePicrosser(string cmd, string[] args)
        {
            if (Context.IsWorldReady)
                PicrossGame.startMe();
            else
                Monitor.Log("You need to load a save first!", LogLevel.Info);
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

            foreach (var item in Helper.ContentPacks.GetOwned())
            {
                Picross[] picrosses = item.ModContent.Load<Picross[]>("picrosses.json");

                for (int i = 0; i < picrosses.Length; i++)
                {                    
                    try
                    {
                        Texture2D solution = item.ModContent.Load<Texture2D>(picrosses[i].DrawingPath);
                        Color[] sourceData = new Color[solution.Width * solution.Height];
                        bool[,] boolVariantOfSolution = new bool[solution.Width, solution.Height];
                        solution.GetData(sourceData, 0, solution.Width * solution.Height);
                        for (int j = 0; j < sourceData.Length; j++)
                        {
                            int x = j % solution.Width;
                            int y = j / solution.Width;
                            boolVariantOfSolution.SetValue(sourceData[j] != Color.White && sourceData[j] != Color.Black, new int[] { x, y });
                        }
                        picrosses[i].PicrossSolution = boolVariantOfSolution;
                        picrosses[i].PackID = item.Manifest.UniqueID;
                    }
                    catch (Exception x)
                    {
                        Monitor.Log(x.ToString());
                        continue;
                    }
                    ValidPicrosses.Add(picrosses[i].Name, picrosses[i]);
                }
            }
            GMCMAPI = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
            if (GMCMAPI is null)
                return;
            GMCMAPI.Register(ModManifest, () => ModConfig = new Config(), () => Helper.WriteConfig(ModConfig));
            GMCMAPI.AddNumberOption(ModManifest, () => ModConfig.Scale, value => ModConfig.Scale = value, () => "Scale", () => "Used for how big the picross will be while playing.");
        }
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name == "Club")
            {
                List<string> intersection = Progress.Completed.Intersect(Progress.GainedQiCoinsFor).ToList();
                if (intersection.Count < Progress.Completed.Count)
                {
                    int coinsToGive = 0;
                    for (int i = 0; i < Progress.Completed.Count; i++)
                    {
                        if (!intersection.Contains(Progress.Completed[i]))
                        {
                            coinsToGive += ValidPicrosses[Progress.Completed[i]].PicrossSolution.Length * 5;
                            Progress.GainedQiCoinsFor.Add(Progress.Completed[i]);
                        }
                    }
                    Game1.player.clubCoins += coinsToGive;
                    Game1.addHUDMessage(new("Qi coins added as reward for past picrosses. Thank you for solving picrosses!", 2));
                }
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button.IsActionButton() && Game1.currentLocation.doesTileHaveProperty((int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y, "Action", "Buildings") == "ThePicrosser")
                StartThePicrosser(null, null);
        }
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Saloon"))
            {
                e.Edit(asset =>
                {
                    var toBeEdited = asset.AsMap();

                    Map sourceMap = Helper.ModContent.Load<Map>("assets/Arcade.tmx");
                    toBeEdited.PatchMap(sourceMap, targetArea: new Rectangle(32, 15, 1, 3), patchMode: PatchMapMode.Overlay);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Club"))
            {
                e.Edit(asset =>
                {
                    var toBeEdited = asset.AsMap();

                    Map sourceMap = Helper.ModContent.Load<Map>("assets/Arcade.tmx");
                    toBeEdited.PatchMap(sourceMap, targetArea: new Rectangle(23, 6, 1, 3), patchMode: PatchMapMode.Overlay);
                });
            }
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Progress = Helper.Data.ReadSaveData<PicrossProgress>("KediDili.Picrosser.Progress");
            Progress ??= new();
        }
        private void OnDayEnding(object sender, DayEndingEventArgs e) => Helper.Data.WriteSaveData("KediDili.Picrosser.Progress", Progress);
    }
}