using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using FusionData;
using FusionData.Data;
using FusionData.Utility.DataReaders;
using FusionVis;
using KeyEventArgs = Fusion.Engine.Input.KeyEventArgs;
using Keys = Fusion.Engine.Input.Keys;


namespace VisTest
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

	    GraphVisualiser vis;

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


	        Game.Keyboard.KeyDown += Keyboard_KeyDown;

	        LoadContent();

	        Game.Reloading += (s, e) => LoadContent();

	        Game.Touch.Tap += args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
	        Game.Touch.DoubleTap += args =>
	            System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
	        Game.Touch.SecondaryTap += args =>
	            System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
	        Game.Touch.Manipulate += args =>
	            System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta +
	                                     "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);

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

            //LoadBSPBTest();
	    }

	    void LoadBSPBTest()
	    {
            LocalRAMDatasheet users = LocalRAMDatasheet.CreateWithLoader(new CSVLoader()
            {
                delimeter = ',',
                escape = '\\',
                quote = '"',
                hasHeader = true,
            }, @"Data\followers_short_profile_29425083.csv");


            LocalRAMDatasheet edges = LocalRAMDatasheet.CreateWithLoader(new BSPBGraphListJSONLoader(),
                @"Data\followers_communication_network_29425083.json");


            Recalculator edgeListRec = OnlineRecalculatorFactory.CreateRecalculatorForSingleChannel(edges.Indexer,
                edges.outputs.First(a => a.Name == "ListId"), Recalculator.PresetRecFuncsFactory.UnrollList("Index", "ListId",
                    "EdgeId", "Edge",
                    DataType.BasicTypes.Integer));


            var edgesIndex = edgeListRec.IndexOutput["EdgeId"];
            var edgesPair = edgeListRec.ChannelOutput["Edge"];

            Func<DataElement, DataElement> userColorFunc = d =>
            {
                Color.Cyan.ToHSB(out float hCyan, out float s, out float b);
                Color.Magenta.ToHSB(out float hMagenta, out float s1, out float b1);
                return new DataElement(Convert.ToInt32(d.Item) == 1 ? hMagenta : hCyan, DataType.BasicTypes.Float);
            };

            Recalculator userColorRec = OnlineRecalculatorFactory.CreateTransformRecalculatorForSingleChannel(users.Indexer, users.Outputs["sex"], userColorFunc, "ColorHue", DataType.BasicTypes.Float);


            Recalculator nodeIdToIntRec = OnlineRecalculatorFactory.CreateTransformRecalculatorForSingleChannel(
                users.Indexer, users.Outputs["id"],
                d => new DataElement(Convert.ToInt32(d.Item), DataType.BasicTypes.Integer), "id",
                DataType.BasicTypes.Integer);


            Recalculator sizeRec = OnlineRecalculatorFactory.CreateTransformRecalculatorForSingleChannel(users.Indexer,
                users.Outputs["count_friends"],
                d => new DataElement((float)Math.Pow(1 + float.Parse((string)d.Item), 0.5f) / 10,
                    DataType.BasicTypes.Float), "Size", DataType.BasicTypes.Float);


            Recalculator massRec = OnlineRecalculatorFactory.CreateTransformRecalculatorForSingleChannel(users.Indexer, users.Outputs["count_friends"],
                d => new DataElement((float)Math.Log(float.Parse((string)d.Item) / 20), DataType.BasicTypes.Float),
                "Size", DataType.BasicTypes.Float);

            ConstChannel nodeSaturation = new ConstChannel("NodeColorSaturation", users.Indexer,
                new DataElement(1, DataType.BasicTypes.Float));

            ConstChannel linkWidth =
                new ConstChannel("LinkWidth", edges.Indexer, new DataElement(0.05f, DataType.BasicTypes.Float));

            ConstChannel linkColorAlpha =
                new ConstChannel("LinkAlpha", edges.Indexer, new DataElement(0.75f, DataType.BasicTypes.Float));


            vis = new GraphVisualiser();
            vis.IndexInputs["Vertices"].Assign(users.Indexer);
            vis.IndexInputs["Edges"].Assign(edgesIndex);

            Log.Message("Assign NodeId: " + vis.Inputs["NodeId"].Assign(nodeIdToIntRec.ChannelOutput["id"]));
            Log.Message("Assign LinksInd1: " + vis.Inputs["LinksInd1"].Assign(edges.Outputs["Id"]));
            Log.Message("Assign LinksInd2: " + vis.Inputs["LinksInd2"].Assign(edgesPair));
            Log.Message("Assign NodeSize: " + vis.Inputs["NodeSize"].Assign(sizeRec.ChannelOutput["Size"]));
            Log.Message("Assign NodeMass: " + vis.Inputs["NodeMass"].Assign(massRec.ChannelOutput["Size"]));
            Log.Message("Assign NodeColorHUE: " + vis.Inputs["NodeColorHUE"].Assign(userColorRec.ChannelOutput["ColorHue"]));
            Log.Message("Assign NodeColorSat: " + vis.Inputs["NodeColorSaturation"].Assign(nodeSaturation));
            Log.Message("Assign LinkWidth: " + vis.Inputs["LinksWidth"].Assign(linkWidth));
            Log.Message("Assign LinkAlpha: " + vis.Inputs["LinkColorAlpha"].Assign(linkColorAlpha));
            //vis.LoadData();
            vis.Prepare();

            Game.RenderSystem.AddLayer(vis.VisLayer);
        }

	    void LoadGridTest()
	    {
	        LocalRAMDatasheet users = LocalRAMDatasheet.CreateWithLoader(new VisgridLoader(), @"Data\GridTest\1514764800.json");


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
            vis.UpdateFrame(gameTime);
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
