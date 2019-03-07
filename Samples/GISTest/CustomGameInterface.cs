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
using Fusion.Engine.Frames2.Controllers;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;
using Fusion.Engine.Frames2;
using Label = Fusion.Engine.Frames2.Components.Label;
using Fusion.Core.Utils;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;

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

            #region UITesting

            UIManager = new UIManager(Game.RenderSystem);
		    UIManager.DebugEnabled = false;

            /*
            txt = new Fusion.Engine.Frames2.Components.Label("z", new TextFormatD2D("Calibri", 20), 150, 450, 100, 100);
            var image = Game.Content.Load<Fusion.Drivers.Graphics.Texture2D>(@"UI-new\fv_palette_bg|nomips");
            img1 = new Image(0, 0, 200, 75, image, 1);
            img2 = new Image(125, 0, 75, 200, image, 1);
            img3 = new Image(0, 125, 200, 75, image, 1);
            img4 = new Image(0, 0, 75, 200, image, 1);

            FreePlacement freePlacement = new FreePlacement(300, 300, 200, 200, true);
            freePlacement.Name = "ImageFreePlacement";
            freePlacement.Add(img1);
            freePlacement.Add(img2);
            //freePlacement.Add(img3);
            //freePlacement.Add(img4);

            //

            /*SerializableList<UIComponent> list = new SerializableList<UIComponent>();
            list.Add(img3);
            list.Add(img4);

            XmlSerializer formatter = new XmlSerializer(typeof(SerializableList<UIComponent>));
            string xml;
            using (StringWriter sw = new StringWriter())
            {
                formatter.Serialize(sw, list);
                xml = sw.ToString();
            }

            System.Console.WriteLine(xml);

            list = new SerializableList<UIComponent>();
            using (StringReader sr = new StringReader(xml))
            {
                list = (SerializableList<UIComponent>)formatter.Deserialize(sr);
            }

            freePlacement.Add(list[0]);
            freePlacement.Add(list[1]);*/

            //

            //UIManager.Root.Add(freePlacement);
            
            #endregion

            /*
            #region button
            var btn = new ButtonController(100, 100);

			btn.Background.Attach(new Border(0, 0, 100, 100));
            btn.Foreground.Attach(new Label("Button", new TextFormatD2D("Calibry", 15), 0, 0, 100, 100));


		    var bgColor = new UIController.PropertyValue("BackgroundColor", Color4.White);
		    bgColor[UIController.State.Hovered] = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
		    bgColor[ButtonController.Pressed] = new Color4(0.0f, 1.0f, 1.0f, 1.0f);

		    var color = new UIController.PropertyValue("BackgroundColor", new Color4(1.0f, 1.0f, 0.0f, 1.0f));
		    color[UIController.State.Hovered] = new Color4(1.0f, 0.0f, 1.0f, 1.0f);
		    color[ButtonController.Pressed] = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            btn.Background.Properties.Add(bgColor);
            btn.Background.Properties.Add(color);

            UIManager.Root.Add(btn);

            #endregion

            #region textbox

		    var tb = new TextBoxController(300, 100);
            tb.Background.Attach(new Border(0, 0, 100, 100));

            UIManager.Root.Add(tb);
            #endregion
    */
            #region radioButtons

            RadioButtonController CreateSimpleRadioButton(float x, float y)
            {
                RadioButtonController rbtn = new RadioButtonController(x, y);
                float height = 25;
                rbtn.Background.Attach(new Border(0, 0, 100, height));
                rbtn.RadioButton.Attach(new Border(0, 0, height, height));
                rbtn.Text.Attach(new Label("RadioButton", new TextFormatD2D("Calibry", 12), height, 0, 100 - height, height));

                var radioButtoncolor = new UIController.PropertyValue("BackgroundColor", new Color4(1.0f, 0.0f, 0.0f, 1.0f));
                radioButtoncolor[UIController.State.Hovered] = new Color4(0.5f, 0.0f, 0.0f, 1.0f);
                radioButtoncolor[UIController.State.Disabled] = new Color4(1.0f, 0.5f, 0.5f, 1.0f);
                radioButtoncolor[RadioButtonController.Pressed] = new Color4(0.5f, 0.5f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.Checked] = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.CheckedHovered] = new Color4(0.0f, 0.5f, 0.0f, 1.0f);
                radioButtoncolor[RadioButtonController.CheckedDisabled] = new Color4(0.5f, 1.0f, 0.5f, 1.0f);

                rbtn.RadioButton.Properties.Add(radioButtoncolor);
                return rbtn;
            }

            RadioButtonGroupController rbg = new RadioButtonGroupController(500, 100, 100, 100);
            rbg.Background.Attach(new Border(0, 0, 100, 100));
            rbg.ButtonsContainer.Attach(new FreePlacement(0, 0, 100, 100));

            UIContainer radioButtons = rbg.ButtonsContainer.Component as UIContainer;
            for (int i = 0; i < 4; i++)
            {
                radioButtons.Add(CreateSimpleRadioButton(0, 25 * i));
            }

            UIManager.Root.Add(rbg);

            #endregion

            #region dialogBox

            DialogBoxController dlg = new DialogBoxController(300, 300, 200, 150);

            var background = new Border();
            background.BackgroundColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            dlg.ContentBackground.Attach(background);

            var titleBackground = new Border();
            titleBackground.BackgroundColor = new Color4(0.25f, 0.25f, 0.25f, 1.0f);
            dlg.TitleBackground.Attach(titleBackground);

            dlg.Title.Attach(new Label("Dialog", new TextFormatD2D("Calibry", 12), 0, 0, 0, 0));
            var content = new FreePlacement();
            content.Add(new Label("Content", new TextFormatD2D("Calibry", 20), 0, 0, 100, 100));
            dlg.Content.Attach(content);

            var btn = new ButtonController(0, 0, 0, 0);

            var buttonBackground = new Border(0, 0, 25, 25);
            buttonBackground.BackgroundColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            btn.Background.Attach(buttonBackground);

            //btn.Foreground.Attach(new Label("  X", new TextFormatD2D("Calibry", 15), 0, 0, 25, 25));
            btn.Foreground.Attach(new Image(0, 0,@"UI-new\fv-icons_clear-text-box|nomips", 1));

            dlg.ExitButton.Attach(btn);

            UIManager.Root.Add(dlg);

            #endregion

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

        private Fusion.Engine.Frames2.Components.Label txt;
	    private float angle = 0;
	    private Image img1;
        private Image img2;
        private Image img3;
        private Image img4;
        private VerticalBox verticalBox1;
        private VerticalBox verticalBox2;
        private VerticalBox verticalBox3;
        private Flexbox flexbox1;

        public void PrintMessage(string message)
		{
			messages.Add(message);
		}

	    public void PrintMessage(string messageFormat, params object[] args)
	    {
	        messages.Add(string.Format(messageFormat, args));
	    }

        public UIContainer GetUIRoot()
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
