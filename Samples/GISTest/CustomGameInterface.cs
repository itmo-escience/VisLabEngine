using System;
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
using FusionUI;
using FusionUI.UI;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;
//using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;
using Fusion.Engine.Frames2;
//using Label = Fusion.Engine.Frames2.Components.Label;
using Fusion.Core.Utils;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;
using Fusion.Engine.Frames2.Controllers;
using Label = Fusion.Engine.Frames2.Components.Label;

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

            #region UITesting
			//----------
            UIManager = new UIManager(Game.RenderSystem);
		    UIManager.DebugEnabled = true;

            var image = Game.Content.Load<Fusion.Drivers.Graphics.Texture2D>(@"UI-new\fv_palette_bg|nomips");
            img1 = new Image(image);
            img2 = new Image(image);

		    //var imgVerticalBox = new VerticalBox()
		    //{
		    //    Name = "ImageFreePlacement",
		    //    DesiredWidth = 500,
		    //    DesiredHeight = 800,
      //          Alignment = HorizontalAlignment.Center
		    //};

		    //var imgFpSlot = UIManager.Root.Insert(imgVerticalBox, 0);
		    //imgFpSlot.X = 50;
		    //imgFpSlot.Y = 50;

            var slot = imgVerticalBox.Insert(img1, 0);
		    img1.DesiredWidth = 250;
		    img1.DesiredHeight = 50;


            var slot2 = imgVerticalBox.Insert(img2, 0);
		    img2.DesiredWidth = 450;
		    img2.DesiredHeight = 250;

		    var labelFreePlacement = new FreePlacement();
		    labelFreePlacement.Name = "LabelFreePlacement";
		    labelFreePlacement.DesiredWidth  = 400;
		    labelFreePlacement.DesiredHeight = 600;

		    var labelFpSlot = UIManager.Root.Insert(labelFreePlacement, 0);
		    labelFpSlot.X = 600;
		    labelFpSlot.Y = 50;
		    labelFpSlot.Angle = 0.3f;

            var text = new Label("Hello, world!", "Calibri", 50);
		    var textSlot = labelFreePlacement.Insert(text, 0);
		    textSlot.X = 10;
		    textSlot.Y = 200;
		    textSlot.Angle = -0.3f;

			var labelAnchorBox = new AnchorBox();
			labelAnchorBox.Name = "LabelAnchorBox";
			labelAnchorBox.DesiredWidth = 300;
			labelAnchorBox.DesiredHeight = 300;

			var labelAbSlot = UIManager.Root.Insert(labelAnchorBox, 0);
			labelAbSlot.X = 600;
			labelAbSlot.Y = 650;
			labelAbSlot.Angle = 0.0f;

			var text2 = new Label("Hello, anchor!", "Calibri", 50) {};
			var text2Slot = labelAnchorBox.Insert(text2, 0);
			text2Slot.Fixators[AnchorBoxSlot.Fixator.Right] = 50;
			text2Slot.Fixators[AnchorBoxSlot.Fixator.Left] = 50;
			text2Slot.Fixators[AnchorBoxSlot.Fixator.Bottom] = 10;
			text2Slot.Fixators[AnchorBoxSlot.Fixator.Top] = 10;

			var border = new Border();
		    border.DesiredWidth = 300;
		    border.DesiredHeight = 400;
            border.BackgroundColor = new Color4(1, 1, 1, 1);

            var borderSlot = UIManager.Root.Insert(border, 0);
		    borderSlot.X = 800;
		    borderSlot.Y = 300;

		    text.Events.Click += (sender, args) => Log.Message("Label click");
		    img2.Events.Click += (sender, args) => Log.Message("Image 2 click");
		    img1.Events.Click += (sender, args) => Log.Message("Image 1 click");
		    border.Events.Click += (sender, args) => Log.Message("Border click");
            //imgVerticalBox.Events.Click += (sender, args) => Log.Message("Image Container click");
		    labelFreePlacement.Events.Click += (sender, args) => Log.Message("Label Container click");
			//----------
			#endregion


			#region button
			var btn = new ButtonController();
		    btn.DesiredWidth = 100;
		    btn.DesiredHeight = 100;
			btn.Background.Attach(new Border() {DesiredWidth = 100, DesiredHeight = 100});
            btn.Foreground.Attach(new Label("Button", new TextFormatD2D("Calibry", 15)));

		    var bgColor = new PropertyValueStates("BackgroundColor", Color4.White);
		    bgColor[State.Hovered] = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
		    bgColor[ButtonController.Pressed] = new Color4(0.0f, 1.0f, 1.0f, 1.0f);

		    var color = new PropertyValueStates("BackgroundColor", new Color4(1.0f, 1.0f, 0.0f, 1.0f));
		    color[State.Hovered] = new Color4(1.0f, 0.0f, 1.0f, 1.0f);
		    color[ButtonController.Pressed] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            btn.Background.Properties.Add(bgColor);
            btn.Background.Properties.Add(color);

            var btnSlot = UIManager.Root.Insert(btn, 0);
		    btnSlot.X = 100;
		    btnSlot.Y = 500;

            #endregion

            #region radioButtons

            var rbg = new RadioButtonGroupController
            {
                DesiredWidth = 100,
                DesiredHeight = 100
            };
            rbg.Background.Attach(new Border());

            RadioButtonController CreateSimpleRadioButton()
            {
                var rb = new RadioButtonController
                {
                    DesiredWidth = 100,
                    DesiredHeight = 25
                };

                rb.Background.Attach(new Border(Color.Gray, Color.White) { DesiredWidth = 100, DesiredHeight = 25});
                rb.Body.Attach(new Label("RadioButton", "Calibri", 14));
                rb.RadioButton.Attach(new Border(Color.Blue, Color.White) { DesiredWidth = 25, DesiredHeight = 25});

                var radioButtoncolor = new PropertyValueStates("BackgroundColor", new Color4(1.0f, 0.0f, 0.0f, 1.0f));
                radioButtoncolor[State.Hovered] = new Color4(0.5f, 0.0f, 0.0f, 1.0f);
                radioButtoncolor[State.Disabled] = new Color4(1.0f, 0.5f, 0.5f, 1.0f);
                radioButtoncolor[RadioButtonController.Pressed] = new Color4(0.5f, 0.5f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.Checked] = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.CheckedHovered] = new Color4(0.0f, 0.5f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.CheckedDisabled] = new Color4(0.5f, 1.0f, 0.5f, 1.0f);

                rb.RadioButton.Properties.Add(radioButtoncolor);

                return rb;
            }

            rbg.Insert(CreateSimpleRadioButton(), 0);
            rbg.Insert(CreateSimpleRadioButton(), 0);

            var rbgSlot = UIManager.Root.Insert(rbg, 0);
            rbgSlot.X = 300;
            rbgSlot.Y = 500;

            #endregion

            /*
            #region textbox

		    var tb = new TextBoxController(300, 100);
            tb.Background.Attach(new Border(0, 0, 100, 100));

            UIManager.Root.Add(tb);
            #endregion
            */


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
		    PrintMessage("FPS: {0:0.00}", gameTime.Fps);
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
