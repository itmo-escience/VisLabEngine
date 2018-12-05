using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI
{
	public class MainFrame : ControllableFrame
	{
		public void Add(Frame frame)
        {
            frame.ZOrder = 1000;
            base.Add(frame);
        }

        public override bool Selected {
            get { return true; }
            set { }
        }

        public float UnitWidth
        {
            get { return Width / ApplicationInterface.ScaleMod; }
            set { Width = (int)(value * ApplicationInterface.ScaleMod); }
        }

        public float UnitHeight
        {
            get { return Height/ ApplicationInterface.ScaleMod; }
            set { Height = (int)(value * ApplicationInterface.ScaleMod); }
        }

        [Obsolete("Please use constructor without FrameProcessor")]
	    public MainFrame(FrameProcessor ui) : this () { }

        public MainFrame()
        {
            ApplicationInterface.Instance.rootFrame = this;
            ApplicationInterface.Instance.FrameProcessor.RootFrame = this;
            X = 0;
            Y = 0;
            Width = Game.Instance.RenderSystem.DisplayBounds.Width;
            Height = Game.Instance.RenderSystem.DisplayBounds.Height;
            BackColor = Color.Zero;

            Game.Instance.RenderSystem.DisplayBoundsChanged += (sender, args) => {
                Width = Game.Instance.RenderSystem.DisplayBounds.Width;
                Height = Game.Instance.RenderSystem.DisplayBounds.Height;
            };
            init ();

            AddMouseActions ();
        }

        protected virtual void init()
        {
        }

		[XmlIgnore]
		public Action ActionResize;

        public override void UpdateResize(bool updateChildren = true)
        {
            base.UpdateResize();
            ActionResize?.Invoke();
        }

        public static Frame GetHoveredFrame (Frame root, Point position) {
            Frame hoverFrame = null;
            UpdateHoverRecursive (root, position, ref hoverFrame);
            return hoverFrame;
        }

        public static bool IsChildOf (Frame root, Frame frame, Frame current) {
            while (frame != current && frame != root && frame != null) {
                frame = frame.Parent;
            }
            return (frame == current);
        }


        static void UpdateHoverRecursive (Frame frame, Point p, ref Frame mouseHoverFrame) {
            if (frame == null) {
                return;
            }

            if (!frame.Ghost && frame.Visible && frame.GlobalRectangle.Contains (p)) {
                frame.ReorderChildren ();
                mouseHoverFrame = frame;
                foreach (var child in frame.Children) {
                    UpdateHoverRecursive (child, p, ref mouseHoverFrame);
                }
            }
        }
    }
}
