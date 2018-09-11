using System;
using System.Collections.Generic;
using System.Net;
using System.Resources;
using System.Globalization;
using System.Linq;
using Fusion.Core.Configuration;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI.UI;

namespace FusionUI
{
    public abstract class ApplicationInterface : Fusion.Engine.Common.UserInterface {
        public ApplicationInterface(Game Game) : base(Game)
        {
            Instance = this;
        }

        public static ApplicationInterface Instance;

        public ResourceManager LangManager;
        public CultureInfo CurrentCulture;
        public void UpdateLanguage()
        {            
            MainFrame.BFSList(rootFrame).ForEach(a =>
            {
                if (a is ScalableFrame) ((ScalableFrame)a).UpdateLanguage();
            });

        }


        public MainFrame rootFrame;        

        public Action onScaleUpdate = null;

        [GameModule ("GUI", "gui", InitOrder.Before)]
        public FrameProcessor FrameProcessor { get { return userInterface; } }
        protected FrameProcessor userInterface;

        protected SpriteLayer     uiLayer;
        protected RenderWorld     masterView;
        protected DiscTexture     debugFont;
        protected SpriteFont      textFont;

		public RenderLayer ViewLayer;


		[Config]
        public static int gridUnitDefault = 4;

        [Config]
        public static float uiScale = 1.0f;

        public static float ScaleMod {
            get { return gridUnitDefault * uiScale; }
        }

        protected List<string> messages = new List<string>();

        public string[] InitialArguments;

        protected  Vector2? prevMousePos;
        protected  Vector2 mouseDelta;

        public virtual void UpdateScale()
        {
            int deltaMin = 64;
            HashSet<Frame> intersect = new HashSet<Frame>();

            foreach (var frame in rootFrame.Children)
            {
                if (!rootFrame.GlobalRectangle.Contains(frame.GlobalRectangle))
                {
                    intersect.Add(frame);
                }
            }

            onScaleUpdate();

            foreach (var frame in rootFrame.Children) {
                frame.UpdateGlobalRect(0, 0);
                if (!(intersect.Contains(frame) && !rootFrame.GlobalRectangle.Contains(frame.GlobalRectangle)))
                {
                    frame.X = MathUtil.Clamp(frame.X, 0, rootFrame.Width - frame.Width);
                    frame.Y = MathUtil.Clamp(frame.Y, rootFrame.Y, rootFrame.Height - frame.Height);
                }
                else
                {
                    frame.X = MathUtil.Clamp(frame.X, 0, rootFrame.Width - deltaMin);
                    frame.Y = MathUtil.Clamp(frame.Y, rootFrame.Y, rootFrame.Height - deltaMin);
                }
            }
        }

        public override void Initialize()
        {
            ViewLayer = new RenderLayer(Game);
            Game.RenderSystem.AddLayer(ViewLayer);

            uiLayer = new SpriteLayer(Game.RenderSystem, 1024);

            ViewLayer.SpriteLayers.Add(uiLayer);

        }

        public override void Update(GameTime gameTime)
        {
        }

        public abstract override void RequestToExit();

        public abstract override void DiscoveryResponse(IPEndPoint endPoint, string serverInfo);
    }
}
