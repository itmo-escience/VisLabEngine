using System;
using System.CodeDom;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Framework;
using Fusion.Build;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Frames;
using Fusion;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using FusionUI;
using FusionUI.UI;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;
using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Controllers;
using Label = Fusion.Engine.Frames2.Components.Label;
using System.Reflection;

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


	public class CustomGameInterface : ApplicationInterface, ICustomizableUI
	{

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


		SpriteLayer testSpritelayerLayer;
		SpriteLayer uiLayer;
		RenderWorld masterView;
		RenderLayer viewLayer;
		DiscTexture debugFont;

		TilesGisLayer tiles;

		Vector2 prevMousePos;


		ScalableFrame Scene;
		ScalableFrame DragFieldFrame;
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface( Game game ) : base(game)
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
			Gis.Debug.ZOrder = -1;
			viewLayer.GisLayers.Add(Gis.Debug);


			// Setup tiles
			tiles = new TilesGisLayer(Game, viewLayer.GlobeCamera);
			tiles.SetMapSource(TilesGisLayer.MapSource.BingMapSatellite);
			tiles.ZOrder = 1;
			viewLayer.GisLayers.Add(tiles);


			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += ( s, e ) => LoadContent();

			Game.Touch.Tap += args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
			Game.Touch.DoubleTap += args => System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
			Game.Touch.SecondaryTap += args => System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
			Game.Touch.Manipulate += args => System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta + "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);

			Game.Mouse.Move += ( sender, args ) => {
				if (Game.Keyboard.IsKeyDown(Keys.LeftButton)) {
					viewLayer.GlobeCamera.MoveCamera(prevMousePos, args.Position);
				}
				if (Game.Keyboard.IsKeyDown(Keys.MiddleButton)) {
					viewLayer.GlobeCamera.RotateViewToPointCamera(args.Offset);
				}

				prevMousePos = args.Position;
			};

			Game.Mouse.Scroll += ( sender, args ) => {
				viewLayer.GlobeCamera.CameraZoom(args.WheelDelta > 0 ? -0.1f : 0.1f);
			};


			viewLayer.SpriteLayers.Add(console.ConsoleSpriteLayer);
			viewLayer.SpriteLayers.Add(uiLayer);

			_random = new Random();
			_spriteLayer = new SpriteLayerD2D(Game.RenderSystem);

			_textFormat = new TextFormatD2D("Calibri", 10);
			_brush = new SolidBrushD2D(Color4.White);

			viewLayer.SpriteLayersD2D.Add(_spriteLayer);

			#region UITesting
			//----------
			UIManager = new UIManager(Game.RenderSystem);
			UIManager.DebugEnabled = true;

			
			//----------

            #region UISerializationTesting

            var testcontainer = new FreePlacement()
            {
                Name = "TestContainer",
                DesiredWidth = 500,
                DesiredHeight = 500,
            };

            //var testImage = new Image(image);
            var testText = new Label("TestText", "Calibri", 14);
            var testBorder = new Border();
            var testFreePlacement = new FreePlacement()
            {
                Name = "TestFreePlacement",
                DesiredWidth = 100,
                DesiredHeight = 100,
            };
            var testFreePlacement2 = new FreePlacement()
            {
                Name = "TestFreePlacement2",
                DesiredWidth = 10,
                DesiredHeight = 10,
            };

            //var testImageSlot = testFreePlasement.Insert(testImage, 0);
            testcontainer.Insert(testFreePlacement, 0);
            testFreePlacement.Insert(testFreePlacement2, 0);
            var testTextSlot = testcontainer.Insert(testText, 1);
            var testBorderSlot = testcontainer.Insert(testBorder, 2);
            //testcontainer.Alignment = VerticalBox.HorizontalAlignment.Right;
            //testImageSlot.X = 10;
            //testImageSlot.Y = 10;
            /*testTextSlot.X = 20;
            testTextSlot.Y = 20;
            testBorderSlot.X = 30;
            testBorderSlot.Y = 30;*/

            var button = new RadioButtonController(new Border(), RadioButtonManager.CreateNewGroup("TestGroup"));

            string str = UIComponentSerializer.WriteToString(testcontainer);
            var comp = UIComponentSerializer.ReadFromString(str);

            #endregion

			#endregion


			userInterface.RootFrame = this.rootFrame = new MainFrame(FrameProcessor);
			viewLayer.SpriteLayers.Add(userInterface.FramesSpriteLayer);

			Scene = new ScalableFrame(0, 0, this.rootFrame.UnitWidth, this.rootFrame.UnitHeight, "Scene", Color.Zero) { Anchor = FrameAnchor.All };
			Scene.Visible = true;
			Scene.Ghost = false;

			rootFrame.Add(Scene);
		}

		void LoadContent()
		{

		}


		void Keyboard_KeyDown( object sender, KeyEventArgs e )
		{
			if (e.Key == Keys.F5) {

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


		protected override void Dispose( bool disposing )
		{
			if (disposing) {
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

		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update( GameTime gameTime )
		{
#if DEBUG
			PrintMessage("Tiles to render count: " + tiles.GetTilesToRenderCount());
			PrintMessage("FPS: {0:0.00}", gameTime.Fps);
#endif
			console.Update(gameTime);
			tiles.Update(gameTime);
			userInterface.Update(gameTime);

			uiLayer.Clear();
			float yPos = 0;
			foreach (var mess in messages) {
				yPos += textFont.LineHeight + 10;
				textFont.DrawString(uiLayer, mess, 15, yPos, Color.White);
			}
			messages.Clear();

			// txt.Text = Game.Mouse.Position.ToString();
			// txt.Angle = MathUtil.TwoPi - angle;
			// img1.Angle = angle;
			// img3.Angle = angle;
			// verticalBox1.Angle = -angle;
			// verticalBox2.Angle = angle / 2;

			UIManager.Update(gameTime);
			UIManager.Draw(_spriteLayer);

			// angle += 0.01f;
			// if (angle > 2 * MathUtil.TwoPi)
			//     angle -= 2 * MathUtil.TwoPi;
		}

		SpriteFont textFont;
		List<string> messages = new List<string>();
		private SpriteLayerD2D _spriteLayer;
		private Random _random;
		private TextFormatD2D _textFormat;
		private SolidBrushD2D _brush;
		public UIManager UIManager { get; private set; }

		public Assembly ProjectAssembly => this.GetType().Assembly;

		public List<Type> CustomUIComponentTypes
		{
			get
			{
				var componentTypes = new List<Type>();
				foreach (var item in ProjectAssembly.GetTypes())
				{
					if (item is UIComponent)
					{
						componentTypes.Add(item);
					}
				}
				//var components = new List<UIComponent>();
				//foreach (var item in componentTypes)
				//{
				//	components.Add(Activator.CreateInstance(item) as UIComponent);
				//}
				return componentTypes;
			}
		}

		//private Fusion.Engine.Frames2.Components.Label txt;
		private float angle = 0;
	    private Image img1;
        private Image img2;
        private Image img3;
        private Image img4;

	    /*private VerticalBox verticalBox1;
        private VerticalBox verticalBox2;
        private VerticalBox verticalBox3;
        private Flexbox flexbox1;
        */

        public void PrintMessage(string message)
		{
			messages.Add(message);
		}

	    public void PrintMessage(string messageFormat, params object[] args)
	    {
	        messages.Add(string.Format(messageFormat, args));
	    }

        public IUIModifiableContainer<ISlot> GetUIRoot()
        {
            return UIManager.Root;
        }

        public UIManager GetUIManager()
        {
            return UIManager;
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
