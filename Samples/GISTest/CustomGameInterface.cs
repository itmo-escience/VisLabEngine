using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
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
using FusionUI;
using FusionUI.UI;
using FusionUI.UI.Factories;
using FusionUI.UI.Elements;
using Fusion.Core.Utils;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using Label = Fusion.Engine.Graphics.SpritesD2D.Label;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;

namespace GISTest
{


	[Command("refreshServers", CommandAffinity.Default)]
	public class RefreshServerList : NoRollbackCommand {

		public RefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StartDiscovery(4, new TimeSpan(0,0,10));
		}

	}

	[Command("stopRefresh", CommandAffinity.Default)]
	public class StopRefreshServerList : NoRollbackCommand {

		public StopRefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StopDiscovery();
		}

	}




	public class CustomGameInterface : ApplicationInterface
	{

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


        SpriteLayer     testSpritelayerLayer;
        SpriteLayer		uiLayer;
		RenderWorld		masterView;
		RenderLayer		viewLayer;
		DiscTexture		debugFont;

		TilesGisLayer	tiles;

		Vector2 prevMousePos;


		ScalableFrame Scene;
		ScalableFrame DragFieldFrame;
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( Game game ) : base(game)
		{
			console			=	new GameConsole( game, "conchars");
			userInterface	=	new FrameProcessor( game, @"Fonts\textFont" );
		}


		/// <summary>
		///
		/// </summary>
		public override void Initialize ()
		{
			debugFont		=	Game.Content.Load<DiscTexture>( "conchars" );
			masterView		=	Game.RenderSystem.RenderWorld;

            Game.RenderSystem.RemoveLayer(masterView);
			masterView.Dispose();

			viewLayer = new RenderLayer(Game);
			Game.RenderSystem.AddLayer(viewLayer);

            testSpritelayerLayer = new SpriteLayer(Game.Instance.RenderSystem, 1024);
            viewLayer.SpriteLayers.Add(testSpritelayerLayer);

            uiLayer		=	new SpriteLayer( Game.RenderSystem, 1024 );
			textFont = Game.Content.Load<SpriteFont>(@"fonts\textFont");

			Gis.Debug = new DebugGisLayer(Game);
			Gis.Debug.ZOrder = -1;
			viewLayer.GisLayers.Add(Gis.Debug);


			// Setup tiles
			tiles = new TilesGisLayer(Game, viewLayer.GlobeCamera);
			tiles.SetMapSource(TilesGisLayer.MapSource.BingMapSatellite);
			tiles.ZOrder = 1;
			viewLayer.GisLayers.Add(tiles);


			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();

			Game.Touch.Tap			+= args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
			Game.Touch.DoubleTap	+= args => System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
			Game.Touch.SecondaryTap += args => System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
			Game.Touch.Manipulate	+= args => System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta + "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);

			Game.Mouse.Move += (sender, args) => {
				if (Game.Keyboard.IsKeyDown(Keys.LeftButton)) {
					viewLayer.GlobeCamera.MoveCamera(prevMousePos, args.Position);
				}
				if (Game.Keyboard.IsKeyDown(Keys.MiddleButton)) {
					viewLayer.GlobeCamera.RotateViewToPointCamera(args.Offset);
				}

				prevMousePos	= args.Position;
			};

			Game.Mouse.Scroll += (sender, args) => {
				viewLayer.GlobeCamera.CameraZoom(args.WheelDelta > 0 ? -0.1f : 0.1f);
			};


			viewLayer.SpriteLayers.Add(console.ConsoleSpriteLayer);
			viewLayer.SpriteLayers.Add(uiLayer);

		    _random = new Random();
            _spriteLayer = new SpriteLayerD2D(Game.RenderSystem);

		    _textFormat = new TextFormatD2D("Calibri", 10);
		    _brush = new SolidBrushD2D(Color4.White);

            viewLayer.SpriteLayersD2D.Add(_spriteLayer);

		    Game.Keyboard.KeyUp += (sender, args) =>
		    {
		        if (args.Key == Keys.LeftButton)
		        {
		            _spriteLayer.DrawEllipse(50 + _random.Next(400), 450 + _random.Next(400), 5 + _random.Next(50), 5 + _random.Next(50), Color4.White);
                }

		        if (args.Key == Keys.RightButton)
		        {
                    _spriteLayer.Clear();
		        }
            };

		    _userInterface2 = new UIManager(Game.RenderSystem);
		    _userInterface2.DebugEnabled = true;

            txt = new Fusion.Engine.Frames2.Components.Label("z", new TextFormatD2D("Calibri", 20), 150, 450, 100, 100);
            string fileName = @"E:\GitHub\image.png";
            img1 = new Image(50, 50, 100, 100, fileName, 1);
            img2 = new Image(50, 50, 100, 100, fileName, 0.66f);
            img3 = new Image(50, 50, 100, 100, fileName, 0.33f);
            img4 = new Image(50, 50, 100, 100, fileName, 0);

            verticalAlignment alignment = verticalAlignment.CENTER;

            verticalBox1 = new VerticalBox(0, 0, 0, 0, alignment, true);
            verticalBox1.Add(txt);
            verticalBox1.Add(img1);
            verticalBox1.Add(img2);

            verticalBox2 = new VerticalBox(0, 0, 0, 0, alignment);
            verticalBox2.Add(img3);
            verticalBox2.Add(img4);

            verticalBox3 = new VerticalBox(200, 200, 0, 0, alignment);
            verticalBox3.Add(verticalBox1);
            verticalBox3.Add(verticalBox2);

            _userInterface2.Root.Add(verticalBox3);

            userInterface.RootFrame = this.rootFrame = new MainFrame(FrameProcessor);
			viewLayer.SpriteLayers.Add(userInterface.FramesSpriteLayer);

			Scene = new ScalableFrame(0, 0, this.rootFrame.UnitWidth, this.rootFrame.UnitHeight, "Scene", Color.Zero) { Anchor= FrameAnchor.All };
			Scene.Visible = true;
			Scene.Ghost = false;

			rootFrame.Add(Scene);
		}

		void LoadContent ()
		{

		}


		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild();
				Game.Reload();
			}

			if (e.Key == Keys.LeftShift) {
				viewLayer.GlobeCamera.ToggleViewToPointCamera();
			}

			if (e.Key == Keys.Escape) {
				Game.Exit();
			}
		}


		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref uiLayer );
				tiles.Dispose();
				viewLayer.Dispose();
			}
			base.Dispose( disposing );
		}


		public override void RequestToExit ()
		{
			Game.Exit();
		}


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
#if DEBUG
		    PrintMessage("Tiles to render count: " + tiles.GetTilesToRenderCount());
#endif
			console.Update( gameTime );
			tiles.Update(gameTime);
			userInterface.Update(gameTime);

            uiLayer.Clear();
			float yPos = 0;
			foreach (var mess in messages) {
				yPos += textFont.LineHeight + 10;
				textFont.DrawString(uiLayer, mess, 15, yPos, Color.White);
			}
			messages.Clear();

            txt.Text = Game.Mouse.Position.ToString();
		    txt.Angle = MathUtil.TwoPi - angle;
            img1.Angle = angle;
            img3.Angle = angle;
            verticalBox1.Angle = -angle;

            _userInterface2.Update(gameTime);
		    _userInterface2.Draw(_spriteLayer);

		    angle += 0.01f;
		    if (angle > MathUtil.TwoPi)
		        angle -= MathUtil.TwoPi;
		}

		SpriteFont textFont;
		List<string> messages = new List<string>();
	    private SpriteLayerD2D _spriteLayer;
	    private Random _random;
	    private TextFormatD2D _textFormat;
	    private SolidBrushD2D _brush;
	    private UIManager _userInterface2;

	    private Fusion.Engine.Frames2.Components.Label txt;
	    private float angle = 0;
	    private Image img1;
        private Image img2;
        private Image img3;
        private Image img4;
        private VerticalBox verticalBox1;
        private VerticalBox verticalBox2;
        private VerticalBox verticalBox3;

        public void PrintMessage(string message)
		{
			messages.Add(message);
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}
	}
}
