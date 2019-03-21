using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Framework;
using Fusion.Build;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Frames;
using Fusion;
using Fusion.Core.Shell;
using System.IO;
using System.Windows.Forms;
using BEPUphysics.BroadPhaseSystems.SortAndSweep;
using Newtonsoft.Json;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;
using Keys = Fusion.Engine.Input.Keys;


namespace GISTest
{


    [Command("refreshServers", CommandAffinity.Default)]
    public class RefreshServerList : NoRollbackCommand
    {

        public RefreshServerList(Invoker invoker) : base(invoker)
        {
        }

        public override void Execute()
        {
            Invoker.Game.GameInterface.StartDiscovery(4, new TimeSpan(0, 0, 10));
        }

    }

    [Command("stopRefresh", CommandAffinity.Default)]
    public class StopRefreshServerList : NoRollbackCommand
    {

        public StopRefreshServerList(Invoker invoker) : base(invoker)
        {
        }

        public override void Execute()
        {
            Invoker.Game.GameInterface.StopDiscovery();
        }

    }




    public class CustomGameInterface : Fusion.Engine.Common.UserInterface
    {

        [GameModule("Console", "con", InitOrder.Before)]
        public GameConsole Console { get { return console; } }
        public GameConsole console;


        [GameModule("GUI", "gui", InitOrder.Before)]
        public FrameProcessor FrameProcessor { get { return userInterface; } }
        FrameProcessor userInterface;


        [Command("GlobeToSpb", CommandAffinity.Default)]
        public class GlobeToSpb : NoRollbackCommand
        {
            public GlobeToSpb(Invoker invoker) : base(invoker)
            {
            }

            public override void Execute()
            {
                (Invoker.Game.GameInterface as CustomGameInterface).GoToSpb();
            }
        }


        SpriteLayer testSpritelayerLayer;
        SpriteLayer uiLayer;
        RenderWorld masterView;
        RenderLayer viewLayer;
        DiscTexture debugFont;

        TilesGisLayer tiles;

        Vector2 prevMousePos;


        public void GoToSpb()
        {
            viewLayer.GlobeCamera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);
            viewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 100;
        }


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="engine"></param>
        public CustomGameInterface(Game game) : base(game)
        {
            console = new GameConsole(game, "conchars");
            userInterface = new FrameProcessor(game, @"Fonts\textFont");
        }


        /// <summary>
        ///
        /// </summary>
        public override void Initialize()
        {
            debugFont = Game.Content.Load<DiscTexture>("conchars");
            masterView = Game.RenderSystem.RenderWorld;

            Game.RenderSystem.RemoveLayer(masterView);
            masterView.Dispose();

            viewLayer = new RenderLayer(Game);
            Game.RenderSystem.AddLayer(viewLayer);

            testSpritelayerLayer = new SpriteLayer(Game.Instance.RenderSystem, 1024);
            viewLayer.SpriteLayers.Add(testSpritelayerLayer);

            uiLayer = new SpriteLayer(Game.RenderSystem, 1024);
            textFont = Game.Content.Load<SpriteFont>(@"fonts\textFont");

            Gis.Debug = new DebugGisLayer(Game);
            Gis.Debug.ZOrder = 0;
            viewLayer.GisLayers.Add(Gis.Debug);


            // Setup tiles
            tiles = new TilesGisLayer(Game, viewLayer.GlobeCamera);
            tiles.SetMapSource(TilesGisLayer.MapSource.DarkV9);
            tiles.ZOrder = 1;
            viewLayer.GisLayers.Add(tiles);


            Game.Keyboard.KeyDown += Keyboard_KeyDown;

            LoadContent();

            Game.Reloading += (s, e) => LoadContent();

            Game.Touch.Tap += args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
            Game.Touch.DoubleTap += args => System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
            Game.Touch.SecondaryTap += args => System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
            Game.Touch.Manipulate += args => System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta + "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);

            Game.Mouse.Move += (sender, args) =>
            {
                if (Game.Keyboard.IsKeyDown(Keys.LeftButton))
                {
                    viewLayer.GlobeCamera.MoveCamera(prevMousePos, args.Position);
                }
                if (Game.Keyboard.IsKeyDown(Keys.MiddleButton))
                {
                    viewLayer.GlobeCamera.RotateViewToPointCamera(args.Offset);
                }

                prevMousePos = args.Position;
            };

            Game.Mouse.Scroll += (sender, args) =>
            {
                viewLayer.GlobeCamera.CameraZoom(args.WheelDelta > 0 ? -0.1f : 0.1f);
            };


            viewLayer.SpriteLayers.Add(console.ConsoleSpriteLayer);
            viewLayer.SpriteLayers.Add(uiLayer);

            var grid = JsonConvert.DeserializeObject<GridMetaJSON.Rootobject>($"{File.ReadAllText("Data/gridTest/grid.json")}");

            var son = JsonConvert.DeserializeObject<GridJSON.Rootobject>($"{File.ReadAllText("Data/gridTest/1515866400.json")}");
            List<RectangleD> squares = new List<RectangleD>();
            List<RectangleD> gridSquares = new List<RectangleD>();
            List<List<Post>> gridPosts = new List<List<Post>>();

            Dictionary<int, double> yys = new Dictionary<int, double>();
            Dictionary<int, double> xxs = new Dictionary<int, double>();
            Dictionary<int, double> xxMin = new Dictionary<int, double>();
            Dictionary<int, double> xxMax = new Dictionary<int, double>();
            Dictionary<int, double> yyMin = new Dictionary<int, double>();
            Dictionary<int, double> yyMax = new Dictionary<int, double>();
            foreach (var f in grid.Data)
            {
                gridSquares.Add(new RectangleD(f.Value.TopLeft.X, f.Value.TopLeft.Y, f.Value.BotRight.X - f.Value.TopLeft.X,
                    f.Value.BotRight.Y - f.Value.TopLeft.Y));
                var ss = f.Key.Split('-');
                var x = Convert.ToInt32(ss[0]);
                var y = Convert.ToInt32(ss[1]);
                if (!yys.ContainsKey(x)) yys.Add(x, f.Value.TopLeft.Y);
                if (!xxs.ContainsKey(y)) xxs.Add(y, f.Value.TopLeft.X);

                //if (!xxMin.ContainsKey(y) || xxMin[y] > f.Value.TopLeft.X) xxMin[y] = f.Value.TopLeft.X;
                //if (!xxMax.ContainsKey(y) || xxMax[y] < f.Value.TopLeft.X) xxMax[y] = f.Value.TopLeft.X;
            }


            foreach (var f in son.Data)
            {
                if (!grid.Data.ContainsKey(f.Id)) continue;
                squares.Add(new RectangleD(grid.Data[f.Id].TopLeft.X, grid.Data[f.Id].TopLeft.Y, grid.Data[f.Id].BotRight.X - grid.Data[f.Id].TopLeft.X,
                    grid.Data[f.Id].BotRight.Y - grid.Data[f.Id].TopLeft.Y));
                gridPosts.Add(f.Posts.ToList());
            }

            double top = yys.Values.Min();
            double bottom = yys.Values.Max();
            double left = xxs.Values.Min();
            double right = xxs.Values.Max();

            //gridLayer.Flags = (int) LinesGisLayer.LineFlags.GEO_LINES;
            //foreach (var ix in xxs.Keys)
            //{
            //    gridLayer.AddLine(
            //        new List<DVector2>()
            //        {
            //            DMathUtil.DegreesToRadians(new DVector2(xxs[ix], top - (bottom - top) * 20)),
            //            DMathUtil.DegreesToRadians(new DVector2(xxs[ix], bottom + (bottom - top) * 20))
            //        }, 0.005f, Color.White);
            //}

            //foreach (var iy in yys.Keys)
            //{
            //    gridLayer.AddLine(
            //        new List<DVector2>()
            //        {
            //            DMathUtil.DegreesToRadians(new DVector2(left - (right - left) * 20, yys[iy])),
            //            DMathUtil.DegreesToRadians(new DVector2(right + (right - left) * 20, yys[iy]))
            //        }, 0.005f, Color.White);
            //}
            List<List<Gis.GeoPoint>> gridLines = new List<List<Gis.GeoPoint>>();
            foreach (var sq in grid.Data.Values)
            {
                gridLines.Add(
                    new List<DVector2>()
                    {
                        DMathUtil.DegreesToRadians(new DVector2(sq.TopLeft.X, sq.TopLeft.Y)),
                        DMathUtil.DegreesToRadians(new DVector2(sq.TopLeft.X, sq.BotRight.Y)),
                        DMathUtil.DegreesToRadians(new DVector2(sq.BotRight.X, sq.BotRight.Y)),
                        DMathUtil.DegreesToRadians(new DVector2(sq.BotRight.X, sq.TopLeft.Y)),
                        DMathUtil.DegreesToRadians(new DVector2(sq.TopLeft.X, sq.TopLeft.Y)),
                    }.Select(a => new Gis.GeoPoint()
                    {
                        Color = new Color(1, 1, 1, 0.1f),
                        Lon = a.X,
                        Lat = a.Y,
                        Tex0 = new Vector4(0.005f, 0, 0, 0),
                    }).ToList());
            }

            LinesGisLayer gridLayer = LinesGisLayer.CreateFromLines(Game, gridLines);
            gridLayer.UpdatePointsBuffer();

            viewLayer.GisLayers.Add(gridLayer);
            var sqs = new List<PolyGisLayer>();
            var maxPosts = gridPosts.Max(a => a.Count);
            for (int i = 0; i < squares.Count; i++)
            {
                var points = new List<DVector2>()
                {
                    DMathUtil.DegreesToRadians(squares[i].TopLeft), DMathUtil.DegreesToRadians(squares[i].TopRight),
                    DMathUtil.DegreesToRadians(squares[i].BottomRight), DMathUtil.DegreesToRadians(squares[i].BottomLeft)
                };
                PolyGisLayer square = PolyGisLayer.CreateFromContour(Game, points.ToArray(), Color.Lerp(new Color(new Vector4(1, 1, 1, 1)), new Color(new Vector4(0, 1, 0, 1)), (float)(Math.Log(gridPosts[i].Count) / Math.Log(maxPosts))), false);
                sqs.Add(square);
                //points.Add(points[0]);
                //points.Reverse();
                //PolyGisLayer cont = PolyGisLayer.CreatePolyFromLineTwo(points.ToArray(), 0.02f, false, new Color(new Vector4(0, 1, 0, 0.5f)));
                //viewLayer.GisLayers.Add(cont);
                viewLayer.GlobeCamera.Yaw = DMathUtil.DegreesToRadians(squares[i].Center.X);
                viewLayer.GlobeCamera.Pitch = DMathUtil.DegreesToRadians(-squares[i].Center.Y);
            }


            sqs.First().MergeList(sqs.Skip(1).ToList());
            foreach (var polyGisLayer in sqs.Skip(1))
            {
                polyGisLayer.Dispose();
            }
            var poly = sqs.First();
            viewLayer.GisLayers.Add(poly);
            var lines = File.ReadAllLines("Data/pointsTest/1514764800.json");

            List<DVector2> locations = new List<DVector2>();
            List<List<Post>> locPosts = new List<List<Post>>();

            for (int i = 0; i < lines.Count(); i++)
            {
                var lSon = JsonConvert.DeserializeObject<LocJSON.Rootobject>(lines[i]);
                locations.Add(new DVector2(lSon.Lon, lSon.Lat));
                locPosts.Add(lSon.Posts.ToList());
            }

            var maxLPosts = locPosts.Max(a => a.Count);
            var pl = new PointsGisLayer(Game, lines.Length, true);
            pl.TextureAtlas = Game.Content.Load<Texture2D>("circle");
            pl.SizeMultiplier = 0.25f;
            pl.ImageSizeInAtlas = new Vector2(36, 36);
            for (int i = 0; i < lines.Length; i++)
            {
                pl.PointsCpu[i] = new Gis.GeoPoint()
                {
                    Lon = DMathUtil.DegreesToRadians(locations[i].X),
                    Lat = DMathUtil.DegreesToRadians(locations[i].Y),
                    Color = Color.Lerp(new Color(new Vector4(1, 1, 1, 1)),
                    new Color(new Vector4(0, 1, 0, 1)),
                    (float)(Math.Log(locPosts[i].Count) / Math.Log(maxLPosts))),
                    Tex0 = new Vector4(0, 0, 0.1f, 0)
                };
            }
            pl.UpdatePointsBuffer();
            viewLayer.GisLayers.Add(pl);


            Game.Keyboard.KeyDown += (sender, args) =>
            {
                if (args.Key == Keys.D1) poly.IsVisible = !poly.IsVisible;
                if (args.Key == Keys.D2) pl.IsVisible = !pl.IsVisible;
            };
        }

        public class GridMetaJSON
        {

            public class Rootobject
            {
                public Dictionary<string, Node> Data { get; set; }
            }

            //public class Data
            //{
            //    public  nn { get; set; }

            //}

            public class Node
            {
                public Topleft TopLeft { get; set; }
                public Botright BotRight { get; set; }
            }

            public class Topleft
            {
                public double X { get; set; }
                public double Y { get; set; }
                public int Weight { get; set; }
                public object Content { get; set; }
            }

            public class Botright
            {
                public double X { get; set; }
                public double Y { get; set; }
                public int Weight { get; set; }
                public object Content { get; set; }
            }

        }

        public class GridJSON
        {

            public class Rootobject
            {
                public Array[] Data { get; set; }
            }

            public class Array
            {
                public string Id { get; set; }
                public Post[] Posts { get; set; }
            }
        }

        public class Post
        {
            public string ID { get; set; }
            public string Shortcode { get; set; }
            public string ImageURL { get; set; }
            public bool IsVideo { get; set; }
            public string Caption { get; set; }
            public int CommentsCount { get; set; }
            public int Timestamp { get; set; }
            public int LikesCount { get; set; }
            public bool IsAd { get; set; }
            public string AuthorID { get; set; }
            public string LocationID { get; set; }
        }

        public class LocJSON
        {

            public class Rootobject
            {
                public string ID { get; set; }
                public string Title { get; set; }
                public double Lat { get; set; }
                public double Lon { get; set; }
                public string Slug { get; set; }
                public Post[] Posts { get; set; }
            }

        }


        void LoadContent()
        {

        }


        void Keyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.F5)
            {

                Builder.SafeBuild();
                Game.Reload();
            }

            if (e.Key == Keys.LeftShift)
            {
                viewLayer.GlobeCamera.ToggleViewToPointCamera();
            }

            if (e.Key == Keys.Escape)
            {
                Game.Exit();
            }

            if (e.Key == Keys.A)
            {
                viewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 500;
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SafeDispose(ref uiLayer);
                tiles.Dispose();
                viewLayer.Dispose();
            }
            base.Dispose(disposing);
        }


        public override void RequestToExit()
        {
            Game.Exit();
        }


        int framesCount = 0;
        float time;
        float fps = 0;

        /// <summary>
        /// Updates internal state of interface.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
#if DEBUG
            PrintMessage("Tiles to render count: " + tiles.GetTilesToRenderCount());

            framesCount++;
            time += gameTime.ElapsedSec;

            if (time > 1.0f)
            {
                fps = framesCount / time;

                framesCount = 0;
                time = time - (int)time;
            }
            PrintMessage("FPS: " + fps);
#endif

            console.Update(gameTime);
            tiles.Update(gameTime);

            uiLayer.Clear();
            float yPos = 0;
            foreach (var mess in messages)
            {
                yPos += textFont.LineHeight + 10;
                textFont.DrawString(uiLayer, mess, 15, yPos, Color.White);
            }
            messages.Clear();

        }


        SpriteFont textFont;
        List<string> messages = new List<string>();
        public void PrintMessage(string message)
        {
            messages.Add(message);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverInfo"></param>
        public override void DiscoveryResponse(System.Net.IPEndPoint endPoint, string serverInfo)
        {
            Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo);
        }
    }
}
