using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace Picrosser
{
    public class PicrossGame : IMinigame
    {
        public enum MiniGameState
        {
            Start, //Welcomey!
            LevelSelect, //Hmm which do I play
            GameOn //Arson
        }

        internal bool isItDone;

        internal MiniGameState state;

        internal static Vector2 TopLeftCorner;

        internal static Texture2D Logo;

        internal static Texture2D BG;

        internal static Texture2D Chart;

        internal static Texture2D CrossPic;

        internal static Texture2D PaintPic;

        internal static Texture2D LevelBox;

        internal static Texture2D Locked;

        internal static Texture2D Complete;

        internal static int levelID;

        internal static int pageID;

        internal static int MaxNumberHeight;

        internal static int MaxNumberWidth;

        internal static int HintRights;

        internal static int Scale;

        internal static int FirstDimension;

        internal static int SecondDimension;

        internal static int MousePosX; //Standart mouse tile methods wont work because we're not using Vanilla's tiling

        internal static int MousePosY;

        internal static bool? LastClick;

        internal static Picross CurrentPicross;

        internal static ClickableTextureComponent Quit;

        internal static ClickableTextureComponent Save;

        internal static ClickableTextureComponent Replay;

        internal static ClickableTextureComponent Check;

        internal static ClickableTextureComponent Hint;

        internal static ClickableTextureComponent Return;

        internal static Rectangle[] LevelRects;

        internal static bool?[,] TileStatuses;

        internal static string Status = "";

        internal static string Info = "";

        internal static List<string[]> LevelBunch = new();

        internal static List<string> Unlocked = new();

        internal static List<string> X_Line_Numbers = new();

        internal static List<string> Y_Line_Numbers = new();

        public PicrossGame()
        {
            isItDone = false;
            state = MiniGameState.Start;
            TopLeftCorner = Utility.getTopLeftPositionForCenteringOnScreen(870, 550);

            Logo = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Logo.png");
            BG = ModEntry.Helper.ModContent.Load<Texture2D>("assets/BG.png");
            Chart = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Chart.png");
            CrossPic = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Cross.png");
            PaintPic = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Paint.png");
            LevelBox = ModEntry.Helper.ModContent.Load<Texture2D>("assets/LevelBox.png");
            Locked = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Locked.png");
            Complete = ModEntry.Helper.ModContent.Load<Texture2D>("assets/Complete.png");

            Quit = new("", new((int)TopLeftCorner.X + 850, 18, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Quit.png"), Rectangle.Empty, 4f);
            Check = new("", new((int)TopLeftCorner.X + 850, 108, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Check.png"), Rectangle.Empty, 4f);
            Save = new("", new((int)TopLeftCorner.X + 850, 198, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Save.png"), Rectangle.Empty, 4f);
            Replay = new("", new((int)TopLeftCorner.X + 850, 288, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Replay.png"), Rectangle.Empty, 4f);
            Hint = new("", new((int)TopLeftCorner.X + 850, 378, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Hint.png"), Rectangle.Empty, 4f);
            Return = new ("", new((int)TopLeftCorner.X + 850, 468, 84, 84), "", "", ModEntry.Helper.ModContent.Load<Texture2D>("assets/Return.png"), Rectangle.Empty, 4f);
            
            Scale = (int)ModEntry.ModConfig.Scale * 18;
            LevelRects = new Rectangle[6]
            {
                new(TopLeftCorner.ToPoint(), new(256)),
                new((int)TopLeftCorner.X + 320, (int)TopLeftCorner.Y, 256, 256),
                new((int)TopLeftCorner.X + 320 + 320, (int)TopLeftCorner.Y, 256, 256),
                new((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 256, 256),
                new((int)TopLeftCorner.X + 320, (int)TopLeftCorner.Y + 320, 256, 256),
                new((int)TopLeftCorner.X + 320 + 320, (int)TopLeftCorner.Y + 320, 256, 256)
            };
            int x = 0;
            int y = 0;
            string[] picrosses = new string[6];
            foreach (var item in ModEntry.ValidPicrosses)
            {
                y = x % 6;
                picrosses[y] = item.Key;
                if (y == 5 || x == ModEntry.ValidPicrosses.Count - 1)
                {
                    LevelBunch.Add(picrosses);
                    picrosses = new string[6];
                }
                x++;
            }
            LevelMenuPrep();
            HintRights = 5;
        }
        public static void startMe()
        {
            Game1.currentMinigame = new PicrossGame();
        }
        public bool tick(GameTime gameTime)
        {
            if (Check.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Check your\nprogress!";
            else if (Quit.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Quit the\nminigame!";
            else if (Save.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Save your\nprogress!";
            else if (Replay.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Restart this\npicross!";
            else if (Return.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Return to\nlevel-selection!";
            else if (Hint.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                Info = "Request a\nhint from the\ngame!";

            MousePosX = (Game1.getMouseX() - MaxNumberWidth + Scale) / Scale;
            MousePosY = (Game1.getMouseY() - MaxNumberHeight + Scale) / Scale;
            if (isItDone)
                unload();
            return isItDone;
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (state == MiniGameState.GameOn)
            {
                if (Quit.containsPoint(x, y))
                {
                    isItDone = true;
                }
                if (Replay.containsPoint(x, y))
                {
                    TileStatuses = new bool?[FirstDimension, SecondDimension];
                }
                if (Return.containsPoint(x, y))
                {
                    state = MiniGameState.LevelSelect;
                    LevelMenuPrep();
                    return;
                }
                if (Save.containsPoint(x, y))
                {
                    if (ModEntry.Progress.Continuing.ContainsKey(CurrentPicross.Name))
                        ModEntry.Progress.Continuing[CurrentPicross.Name] = TileStatuses;
                    else
                        ModEntry.Progress.Continuing.Add(CurrentPicross.Name, TileStatuses);
                    Status = "Saved!";
                }
                if (Hint.containsPoint(x, y))
                {
                    if (HintRights > 0)
                    {
                        List<(int X, int Y, bool? status)> list = new();
                        for (int indexX = 0; indexX < FirstDimension; indexX++)
                            for (int indexY = 0; indexY < SecondDimension; indexY++)
                                if (TileStatuses[indexX, indexY] is null)
                                    list.Add((indexX, indexY, TileStatuses[indexX, indexY]));
                        if (list.Count > 0)
                        {
                            int random = Game1.random.Next(0, list.Count);
                            TileStatuses[list[random].X, list[random].Y] = CurrentPicross.PicrossSolution[list[random].X, list[random].Y];
                            Status = $"Revealed: {list[random].X + 1}, {list[random].Y + 1}";
                            HintRights--;
                        }
                        else
                        {
                            Status = "Hint request failed!";
                        }
                    }
                    else if (HintRights == 0 && Game1.player.currentLocation.Name == "Club")
                        Status = "You've got no hint rights,\nbut you can buy 5 more only for 99.99$!\nPress the plus (+) button to buy!";
                    else
                        Status = "You don't have any hints left!";
                }
                if (Check.containsPoint(x, y))
                {
                    List<(int X, int Y, bool? status)> list = new();
                    List<(int X, int Y, bool? status)> SolutionTrues = new();
                    for (int indexX = 0; indexX < FirstDimension; indexX++)
                        for (int indexY = 0; indexY < SecondDimension; indexY++)
                        {
                            if (TileStatuses[indexX, indexY] is true)
                                list.Add((indexX, indexY, true));
                            if (CurrentPicross.PicrossSolution[indexX, indexY] is true)
                                SolutionTrues.Add((indexX, indexY, true));
                        }

                    for (int i = 0; i < list.Count; i++)
                        if (CurrentPicross.PicrossSolution[list[i].X, list[i].Y] != list[i].status)
                        {
                            Status = $"You've lost\nthe track! First\nerror: {list[i].X + 1},{list[i].Y + 1}";
                            break;
                        }
                        else
                            Status = "It's OK so\nfar.";

                    if (Status == "It's OK so\nfar." && SolutionTrues.Count == list.Count)
                    {
                        if (!ModEntry.Progress.Completed.Contains(CurrentPicross.Name))
                        {
                            Status = "Congrats!\nYou successfully\ncompleted the\npicross.\nSaving...";
                            ModEntry.Progress.Completed.Add(CurrentPicross.Name);
                            if (Game1.player.currentLocation.Name == "Club")
                            {
                                Game1.player.clubCoins += FirstDimension * SecondDimension * 5;
                                Status += $"\nAwarded {FirstDimension * SecondDimension * 5}\nQi coins!";
                                ModEntry.Progress.GainedQiCoinsFor.Add(CurrentPicross.Name);
                            }
                        }
                        else
                            Status = "Congrats!\nYou successfully\ncompleted the\npicross again.";
                    }
                }
            }
            if (state == MiniGameState.Start)
            {
                pageID = 0;
                LevelMenuPrep();
                state = MiniGameState.LevelSelect;
            }
            else if (state == MiniGameState.LevelSelect)
            {
                for (int i = 0; i < LevelRects.Length; i++)
                {
                    if (LevelRects[i].X <= x && x <= LevelRects[i].X + LevelRects[i].Width && LevelRects[i].Y <= y && y <= LevelRects[i].Y + LevelRects[i].Height)
                    {
                        levelID = i;
                        string which = LevelBunch[pageID][levelID];
                        picrossPrep(which);
                        break;
                    }
                }
            }
            else if (state == MiniGameState.GameOn)
                for (int XX = 0; XX < FirstDimension; XX++)
                    for (int YY = 0; YY < SecondDimension; YY++)
                        if (MousePosX == XX + 1 && MousePosY == YY + 1) //Checkbox Lands
                        { 
                            TileStatuses[XX, YY] = TileStatuses[XX, YY] switch { null => false, false => true, true => null };
                            LastClick = TileStatuses[XX, YY];
                        }
        }
        public void receiveKeyPress(Keys k)
        {
            if (k == Keys.Escape)
                isItDone = true;
            if (k == Keys.Down && pageID < LevelBunch.Count - 1)
                pageID++;
            if (k == Keys.Up && pageID > 0)
                pageID--;
            if (k == Keys.Add)
            {
                if (Status == "You've got no hint rights,\nbut you can buy 5 more only for 99.99$!\nPress the plus (+) button to buy!" && Game1.player.Money >= 100)
                {
                    HintRights += 5;
                    Game1.player.Money -= 100;
                    Status = "Purchase successful!";
                }
                else
                {
                    Status = "Purchase failed!";
                }
            }
        }
        public void receiveKeyRelease(Keys k) { }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);
            b.Draw(BG, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White);
            switch (state)
            {
                case MiniGameState.Start:
                    b.Draw(Logo, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0.5f);
                    break;
                case MiniGameState.LevelSelect:
                    for (int i = 0; i < LevelRects.Length; i++)
                    {
                        b.DrawString(Game1.dialogueFont, "Use up/down arrows on your keyboard to navigate!", new(TopLeftCorner.X, TopLeftCorner.Y - (2 * Game1.dialogueFont.LineSpacing)), new(79, 52, 57));
                        b.Draw(LevelBox, LevelRects[i], Color.White);
                        if (i < LevelBunch[pageID].Length)
                        {
                            if (ModEntry.Progress.Completed.Contains(LevelBunch[pageID][i]))
                                b.Draw(Complete, LevelRects[i], Color.White);
                            else if (Unlocked.Contains(LevelBunch[pageID][i]))
                                b.DrawString(Game1.dialogueFont, (i + 1 + (pageID * 6)).ToString(), new(LevelRects[i].X + 48, LevelRects[i].Y + 48), new(79, 52, 57));
                            else if (!string.IsNullOrEmpty(LevelBunch[pageID][i]))
                                b.Draw(Locked, LevelRects[i], Color.White);
                        }
                    }
                    break;
                case MiniGameState.GameOn:
                    Check.draw(b);
                    Quit.draw(b);
                    Save.draw(b);
                    Replay.draw(b);
                    Return.draw(b);
                    Hint.draw(b);
                    DrawChart(b);
                    b.DrawString(Game1.smallFont, FirstDimension.ToString(), new(Scale * FirstDimension + MaxNumberWidth, MaxNumberHeight), new Color(79, 52, 57));
                    b.DrawString(Game1.smallFont, SecondDimension.ToString(), new(MaxNumberWidth, Scale * SecondDimension + MaxNumberHeight), new Color(79, 52, 57));
                    for (int x = 0; x < FirstDimension; x++)
                        for (int y = 0; y < SecondDimension; y++)
                            if (TileStatuses[x, y] is not null)
                                b.Draw((bool)TileStatuses[x, y] ? PaintPic : CrossPic, new Rectangle(x * Scale + MaxNumberWidth, y * Scale + MaxNumberHeight, Scale, Scale), new Rectangle(0, 0, 18, 18), Color.White);
                    b.DrawString(Game1.dialogueFont, Status, new(TopLeftCorner.X + 950, 18), new Color(79, 52, 57));
                    b.DrawString(Game1.dialogueFont, Info, new(TopLeftCorner.X + 950, 250), new Color(79, 52, 57));

                    for (int i = 0; i < X_Line_Numbers.Count; i++)
                        b.DrawString(Game1.smallFont, X_Line_Numbers[i], new(i * Scale + MaxNumberWidth + Game1.smallFont.LineSpacing / Scale, 0), new Color(79, 52, 57));

                    for (int i = 0; i < Y_Line_Numbers.Count; i++)
                        b.DrawString(Game1.smallFont, Y_Line_Numbers[i], new(0, i * Scale + MaxNumberHeight + Game1.smallFont.LineSpacing / Scale), new Color(79, 52, 57));
                    break;
            }
            drawMouse(b);
            b.End();
        }
        public void picrossPrep(string name)
        {
            if (!string.IsNullOrEmpty(name) && ModEntry.ValidPicrosses.ContainsKey(name) && Unlocked.Contains(name))
            {
                CurrentPicross = ModEntry.ValidPicrosses[name];
                if (ModEntry.Progress.Continuing.ContainsKey(CurrentPicross.Name))
                    TileStatuses = ModEntry.Progress.Continuing[CurrentPicross.Name];
                else
                {
                    TileStatuses = new bool?[CurrentPicross.PicrossSolution.GetLength(0), CurrentPicross.PicrossSolution.GetLength(1)];
                    for (int x = 0; x < TileStatuses.GetLength(0); x++)
                        for (int y = 0; y < TileStatuses.GetLength(1); y++)
                            TileStatuses[x, y] = null;
                    HintRights = 5;
                }
                FirstDimension = TileStatuses.GetLength(0);
                SecondDimension = TileStatuses.GetLength(1);
                Status = "";
                StringBuilder stringBuilder = new();
                int sdsfs = 0;
                X_Line_Numbers.Clear();
                Y_Line_Numbers.Clear();
                for (int XX = 0; XX < FirstDimension; XX++)
                {
                    for (int YY = 0; YY < SecondDimension; YY++)
                    {
                        if (CurrentPicross.PicrossSolution[XX, YY])
                            sdsfs++;
                        else if (sdsfs > 0 && !CurrentPicross.PicrossSolution[XX, YY] && YY < SecondDimension)
                        {
                            stringBuilder.Append(sdsfs + "\n");
                            sdsfs = 0;
                        }
                        if (YY == SecondDimension - 1)
                        {
                            if (sdsfs > 0)
                                stringBuilder.Append(sdsfs);
                            else if (string.IsNullOrEmpty(stringBuilder.ToString()) && sdsfs == 0)
                                stringBuilder.Append(0);

                            X_Line_Numbers.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            sdsfs = 0;
                        }
                    }
                }
                for (int YY = 0; YY < SecondDimension; YY++)
                {
                    for (int XX = 0; XX < FirstDimension; XX++)
                    {
                        if (CurrentPicross.PicrossSolution[XX, YY])
                            sdsfs++;
                        else if (sdsfs > 0 && !CurrentPicross.PicrossSolution[XX, YY] && XX < FirstDimension)
                        {
                            stringBuilder.Append(sdsfs.ToString() + " ");
                            sdsfs = 0;
                        }
                        if (XX == FirstDimension - 1)
                        {
                            if (sdsfs > 0)
                                stringBuilder.Append(sdsfs.ToString() + " ");
                            else if (string.IsNullOrEmpty(stringBuilder.ToString()) && sdsfs == 0)
                                stringBuilder.Append(0);
                            Y_Line_Numbers.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            sdsfs = 0;
                        }
                    }
                }
                MaxNumberHeight = 0;
                MaxNumberWidth = 0;
                for (int i = 0; i < X_Line_Numbers.Count; i++)
                    if (Game1.smallFont.MeasureString(X_Line_Numbers[i]).Y > MaxNumberHeight)
                        MaxNumberHeight = (int)Game1.smallFont.MeasureString(X_Line_Numbers[i]).Y;
                for (int i = 0; i < Y_Line_Numbers.Count; i++)
                    if (Game1.smallFont.MeasureString(Y_Line_Numbers[i]).X > MaxNumberWidth)
                        MaxNumberWidth = (int)Game1.smallFont.MeasureString(Y_Line_Numbers[i]).X;
                state = MiniGameState.GameOn;
            }
        }
        public void LevelMenuPrep()
        {
            for (int i = 0; i < LevelBunch.Count; i++)
            {
                for (int j = 0; j < LevelBunch[i].Length; j++)
                {
                    if (!string.IsNullOrEmpty(LevelBunch[i][j]))
                    {
                        if (ModEntry.ValidPicrosses.TryGetValue(LevelBunch[i][j], out Picross picross) && picross.MustBeSolvedFirst != Array.Empty<string>())
                        {
                            int asdsa = 0;
                            for (int a = 0; a < picross.MustBeSolvedFirst.Length; a++)
                                if (ModEntry.Progress.Completed.Contains(picross.MustBeSolvedFirst[a]))
                                    asdsa++;
                            if (picross.MustBeSolvedFirst.Length == asdsa)
                                Unlocked.Add(LevelBunch[i][j]);
                        }
                        else if (picross is not null)
                            Unlocked.Add(LevelBunch[i][j]);
                    }
                }
            }
        }
        public void drawMouse(SpriteBatch b, bool ignore_transparency = false, int cursor = -1)
        {
            if (!Game1.options.hardwareCursor)
            {
                float transparency;
                transparency = Game1.mouseCursorTransparency;
                if (ignore_transparency)
                {
                    transparency = 1f;
                }
                if (cursor < 0)
                {
                    cursor = (Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0;
                }
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, cursor, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
            }
        }
        public void DrawChart(SpriteBatch b)
        {
            b.Draw(Chart, new Rectangle(MaxNumberWidth - Scale, MaxNumberHeight - Scale, Scale, Scale), new Rectangle(36, 0, 18, 18), Color.White);            
            for (int x = 0; x < FirstDimension + 1; x++)
            {
                b.Draw(Chart, new Rectangle(MaxNumberWidth - Scale + x * Scale, 0, Scale, MaxNumberHeight), new Rectangle(18, 0, 18, 18), Color.White);
                if (x > 0)
                    b.Draw(Chart, new Rectangle(x * Scale + MaxNumberWidth - Scale, MaxNumberHeight - Scale, Scale, Scale), new Rectangle(36, 0, 18, 18), Color.White);
                for (int y = 0; y < SecondDimension + 1; y++)
                {
                    b.Draw(Chart, new Rectangle(0, MaxNumberHeight - Scale + y * Scale, MaxNumberWidth, Scale), new Rectangle(0, 0, 18, 18), Color.White);

                    if (y > 0 && x > 0)
                    {
                        b.Draw(Chart, new Rectangle(MaxNumberWidth - Scale, y * Scale + MaxNumberHeight - Scale, Scale, Scale), new Rectangle(36, 0, 18, 18), Color.White);
                        b.Draw(Chart, new Rectangle(x * Scale + MaxNumberWidth - Scale, y * Scale + MaxNumberHeight - Scale, Scale, Scale), new Rectangle(36, 0, 18, 18), Color.White);
                    }
                }
            }
            b.DrawString(Game1.dialogueFont, $"{MousePosX},{MousePosY}", Vector2.Zero, new Color(79, 52, 57));
        }
        public void changeScreenSize()
        {
            TopLeftCorner = Utility.getTopLeftPositionForCenteringOnScreen(870, 550);

            Quit.bounds = new((int)TopLeftCorner.X + 850, 18, 84, 84);
            Check.bounds = new((int)TopLeftCorner.X + 850, 108, 84, 84);
            Save.bounds = new((int)TopLeftCorner.X + 850, 198, 84, 84);
            Replay.bounds = new((int)TopLeftCorner.X + 850, 288, 84, 84);
            Hint.bounds = new((int)TopLeftCorner.X + 850, 378, 84, 84);
            Return.bounds = new((int)TopLeftCorner.X + 850, 468, 84, 84);

            LevelRects = new Rectangle[6]
            {
                new(TopLeftCorner.ToPoint(), new(256)),
                new((int)TopLeftCorner.X + 320, (int)TopLeftCorner.Y, 256, 256),
                new((int)TopLeftCorner.X + 320 + 320, (int)TopLeftCorner.Y, 256, 256),
                new((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 256, 256),
                new((int)TopLeftCorner.X + 320, (int)TopLeftCorner.Y + 320, 256, 256),
                new((int)TopLeftCorner.X + 320 + 320, (int)TopLeftCorner.Y + 320, 256, 256)
            };
        }

        public void unload()
        {
            Unlocked = new();
            CurrentPicross = null;
            TileStatuses = new bool?[,]{ };
        }

        public string minigameId() => "ThePicrosser";
        public void leftClickHeld(int x, int y) 
        {
            if (state == MiniGameState.GameOn)
            {
                for (int XX = 0; XX < FirstDimension; XX++)
                    for (int YY = 0; YY < SecondDimension; YY++)
                        if (MousePosX == XX + 1 && MousePosY == YY + 1) //Checkbox Lands
                            TileStatuses[XX, YY] = LastClick;
            }
        }
        public void receiveEventPoke(int data) { }
        public void releaseLeftClick(int x, int y) { }
        public void receiveRightClick(int x, int y, bool playSound = true) { }
        public void releaseRightClick(int x, int y) { }
        public bool forceQuit() => isItDone;
        public bool overrideFreeMouseMovement() => false;
        public bool doMainGameUpdates() => false;
    }
}
