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
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using Label = Fusion.Engine.Graphics.SpritesD2D.Label;

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

		    var img = new Image(10, 50, 100, 200);
		    var txt = new Fusion.Engine.Frames2.Components.Label("I'm Label!", 100, 50, 100, 80);
		    var border = new Border(200, 50, 150, 260);

            _userInterface2.Root.Add(img);
            _userInterface2.Root.Add(txt);
            _userInterface2.Root.Add(border);

            userInterface.RootFrame = this.rootFrame = new MainFrame(FrameProcessor);
			viewLayer.SpriteLayers.Add(userInterface.FramesSpriteLayer);

			Scene = new ScalableFrame(0, 0, this.rootFrame.UnitWidth, this.rootFrame.UnitHeight, "Scene", Color.Zero) { Anchor= FrameAnchor.All };
			Scene.Visible = true;
			Scene.Ghost = false;

			rootFrame.Add(Scene);
		}

		public Window CreateStartFrame()
		{
			var rootFrame = this.rootFrame;


			Window mainFrame = new Window(50, 5, 200, 250, "TestWindow", Color.Zero)
			{
				ImageColor = Color.White,
				ImageMode = FrameImageMode.Fitted,
				//Anchor = FrameAnchor.All,
				HatColor = Color.Coral,
				BasementColor = Color.Crimson,
				Border = 5,
				BorderColor = Color.White
			};

			ScalableFrame mainLayout = new ScalableFrame(200, 20, 20, 80, "TestLayout", Color.Zero)
			{

			};

			mainFrame.Add(mainLayout);

			Color buf = UIConfig.ActiveColor;

			UIConfig.ActiveColor = Color.Red;
			Window smallWindow = new Window(0, 10, 256 / ApplicationInterface.gridUnitDefault, 256 / ApplicationInterface.gridUnitDefault,
				"SomethingNew", Color.SandyBrown, fixedSize: true)
			{ HatColor = Color.Blue };
			smallWindow.Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right;

			smallWindow.Click += ( s, e ) =>
			{
				//smallWindow.BackColor = new Color3();
			};

			UIConfig.ActiveColor = buf;

			buf = UIConfig.ButtonColor;
			UIConfig.ButtonColor = Color.Gray;

			Button next;
			//    = new Button(smallWindow.ui, 0, 10, 10, 10, "Next", UIConfig.ButtonColor, UIConfig.ActiveColor, 50,
			//    () =>
			//    {
			//        System.Console.WriteLine("You've just performed a button press action: " + next.ToString());
			//    }, Color.White, Color.White)
			//{
			//    Anchor = FrameAnchor.None,
			//    FontHolder = UIConfig.FontSubtitle,
			//    TextAlignment = Alignment.MiddleCenter,
			//};

			UIConfig.ButtonColor = buf;


			//next.Visible = true;
			var holder = ButtonFactory.CenterButtonHolder(0, 10, mainLayout, 30, "Next_2",
								() =>
								{
									System.Console.WriteLine("You've just performed a button press action: "/* + next.ToString()*/);
								}, out next);
			holder.Text = "ButtonHolder";
			holder.Height = 40;
			mainLayout.Add(next);
			smallWindow.Visible = true;
			mainLayout.Add(smallWindow);

			var t = "testSerialization.xml";

			Frame ser = new Frame(10, 20, 190, 220, "Fram", Color.Red)
			{
				Image = Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_close-window")
			};
			ControllableFrame ser2 = new ControllableFrame(11, 21, 60, 60, "ContrF", Color.Blue)
			{
				Image = Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_radio-on")
			};
			ScalableFrame ser3 = new ScalableFrame(20, 6, 25, 15, "Scales", Color.Green)
			{
				Image = Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_switcher-on")
			};
			FreeFrame ser4 = new FreeFrame(30, 20, 15, 15, "FreeFr", Color.Gray)
			{
				Image = Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_checkbox-big-on")
			};

			ScalableFrame sHolder = new ScalableFrame(50, 35, 55, 15, "Scales", Color.Green);

		    SliderFactory.SliderHorizontalHolderNew(5, 35, 5, 15, ser3, "mySlider", 10, null, 0, 1, 0.3f, out var slider);

			slider.Text = slider.Name = "Sliderser";

			//dynamic frame = new ScalableFrame(ui, 0, 0, 100, 100, "ScalableFrame", Color.Gray);
			//FrameSerializer.Write(frame, "Frames\\" + frame.GetType().Name + ".xml");
			//frame = new ControllableFrame(ui, 0, 0, 100, 100, "ControllableFrame", Color.Gray);
			//FrameSerializer.Write(frame, "Frames\\" + frame.GetType().Name + ".xml");
			//frame = new FreeFrame(ui, 0, 0, 100, 100, "FreeFrame", Color.Gray);
			//FrameSerializer.Write(frame, "Frames\\" + frame.GetType().Name + ".xml");



			ser.Add(ser3);
			ser.Add(ser4);
			ser.Add(ser2);
			ser.Add(slider);

			mainLayout.Add(ser);

			//FrameSerializer.Write(mainFrame, "Frames\\" + "TestWindow" + ".xml");

			//FrameSerializer.Write(ser, "Frames\\" + "Fram" + ".xml");

			return mainFrame;
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

			DVector2 pos;// = new DVector2();
            pos = GeoHelper.CartesianToSpherical(new DVector3(10, 10, 0));
		    //GlobeCamera.Instance.ScreenToSpherical(10, 10, out pos);
            var cartPos = GeoHelper.SphericalToCartesian(pos, GeoHelper.EarthRadius);
            var screenPos = GlobeCamera.Instance.CartesianToScreen(cartPos);

            uiLayer.Clear();
			float yPos = 0;
			foreach (var mess in messages) {
				yPos += textFont.LineHeight + 10;
				textFont.DrawString(uiLayer, mess, 15, yPos, Color.White);
			}
			messages.Clear();

            _userInterface2.Update(gameTime);
		    _userInterface2.Draw(_spriteLayer);

            /*_spriteLayer.Clear();
		    for (var i = 0; i < 10000; i++)
		    {
		        _spriteLayer.Draw(new Label("Hello, world!", new RectangleF(50 + i % 749, 50 + i % 1354, 150, 300), _textFormat, _brush));
		    }
            */

		}

		SpriteFont textFont;
		List<string> messages = new List<string>();
	    private SpriteLayerD2D _spriteLayer;
	    private Random _random;
	    private TextFormatD2D _textFormat;
	    private SolidBrushD2D _brush;
	    private UIManager _userInterface2;

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
