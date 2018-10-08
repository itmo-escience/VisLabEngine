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




	public class CustomGameInterface : Fusion.Engine.Common.UserInterface {

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


		SpriteLayer     testSpritelayerLayer;
        SpriteLayer		uiLayer;
		RenderWorld		masterView;
		RenderLayer		viewLayer;
		DiscTexture		debugFont;

		TilesGisLayer	tiles;

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
			Gis.Debug.ZOrder = 0;
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

			if(e.Key == Keys.A)
			{
				viewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 500;
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


		int framesCount = 0;
		float time;
		float fps = 0;

		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
#if DEBUG
		    PrintMessage("Tiles to render count: " + tiles.GetTilesToRenderCount());

			framesCount++;
			time += gameTime.ElapsedSec;

			if(time > 1.0f) {
				fps = framesCount / time;

				framesCount = 0;
				time = time - (int)time;
			}
			PrintMessage("FPS: " + fps);
#endif

			console.Update( gameTime );
			tiles.Update(gameTime);

            uiLayer.Clear();
			float yPos = 0;
			foreach (var mess in messages) {
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
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}
	}
}
