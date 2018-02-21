using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using FusionUI.UI.Elements.DropDown;

namespace FusionUI.UI
{
    public class ScalableFrame : ControllableFrame
    {
        public int order = 0;

        public static float ScaleMultiplier
        {
            get { return (ApplicationInterface.ScaleMod); }
        }

        public UIConfig.FontHolder FontHolder = new UIConfig.FontHolder(@"fonts\new\Base");

        public override SpriteFont Font => FontHolder[ApplicationInterface.uiScale];

        private float unitX, unitY, unitWidth, unitHeight;

        public virtual float UnitX
        {
            get { return unitX; }
            set
            {
                if (unitX != value)
                    OnCoordUpdate?.Invoke(value);
                unitX = value;                
            }
        }

        public virtual float UnitY
        {
            get { return unitY; }
            set
            {
                if (unitY != value)
                    OnCoordUpdate?.Invoke(value);
                unitY = value;                
            }
        }

        public Action<float> OnCoordUpdate;

        public virtual float UnitWidth
        {
            get { return unitWidth; }
            set { unitWidth = value; }
        }

        public virtual float UnitHeight
        {
            get { return unitHeight; }
            set { unitHeight = value; }
        }

        public override int X
        {
            get { return (int) Math.Round(unitX * ScaleMultiplier); }
            set
            {
                unitX = value / ScaleMultiplier;                
            }
        }

        public override int Y
        {
            get { return (int) Math.Round(unitY * ScaleMultiplier); }
            set
            {
                unitY = value / ScaleMultiplier;
            }
        }

        public override int Width
        {
            get { return (int) Math.Round(unitWidth * ScaleMultiplier); }
            set { unitWidth = value / ScaleMultiplier; }
        }

        public override int Height
        {
            get { return (int) Math.Round(unitHeight * ScaleMultiplier); }
            set { unitHeight = value / ScaleMultiplier; }
        }

        public override string Text
        {
            get { return text; }
            set
            {
                text = value;                
                DefaultText = value.Trim();
                UpdateLanguage();
            }
        }

        private string text;
        public bool TryLoadResource = true;

        public static string TryGetText(string DefaultText)
        {
            try
            {
                return ApplicationInterface.Instance.LangManager.GetString(DefaultText,
                    ApplicationInterface.Instance.CurrentCulture).Replace("\\n", "\n").Replace("\\t", "\t");
            }
            catch (MissingManifestResourceException e)
            {
                return DefaultText;
            }
        }

        public void UpdateLanguage(bool alwaysTry = false)
        {
            TryLoadResource = TryLoadResource | alwaysTry;
            try
            {
                text = TryLoadResource && ApplicationInterface.Instance.LangManager != null ? ApplicationInterface.Instance.LangManager.GetString(DefaultText,
                    ApplicationInterface.Instance.CurrentCulture).Replace("\\n", "\n").Replace("\\t", "\t") : DefaultText;
            }
            catch (MissingManifestResourceException e)
            {
                if (!string.IsNullOrWhiteSpace(DefaultText) && !double.TryParse(DefaultText, out var n)) Log.Warning("Missing translation for string \"{0}\"", DefaultText);
                text = DefaultText;
                TryLoadResource = false;
            }

            
        }



        public string DefaultText = "_";
       

        #region Padding

        private float unitPaddingTop, unitPaddingBottom, unitPaddingLeft, unitPaddingRight;

        public override int PaddingBottom
        {
            get { return (int) (unitPaddingBottom * ScaleMultiplier); }
            set { unitPaddingBottom = value / ScaleMultiplier; }
        }

        public override int PaddingTop
        {
            get { return (int) (unitPaddingTop * ScaleMultiplier); }
            set { unitPaddingTop = value / ScaleMultiplier; }
        }

        public override int PaddingLeft
        {
            get { return (int) (unitPaddingLeft * ScaleMultiplier); }
            set { unitPaddingLeft = value / ScaleMultiplier; }
        }

        public override int PaddingRight
        {
            get { return (int) (unitPaddingRight * ScaleMultiplier); }
            set { unitPaddingRight = value / ScaleMultiplier; }
        }

        public virtual float UnitPaddingLeft
        {
            get { return unitPaddingLeft; }
            set { unitPaddingLeft = value; }
        }

        public virtual float UnitPaddingRight
        {
            get { return unitPaddingRight; }
            set { unitPaddingRight = value; }
        }

        public virtual float UnitPaddingTop
        {
            get { return unitPaddingTop; }
            set { unitPaddingTop = value; }
        }

        public virtual float UnitPaddingBottom
        {
            get { return unitPaddingBottom; }
            set { unitPaddingBottom = value; }
        }

        public virtual float UnitHPadding
        {
            set { UnitPaddingLeft = UnitPaddingRight = value; }
        }

        public virtual float UnitVPadding
        {
            set { UnitPaddingTop = UnitPaddingBottom = value; }
        }

        public virtual float UnitPadding
        {
            set { UnitHPadding = UnitVPadding = value; }
        }

        #endregion

        #region Text

        private float unitTextOffsetX, unitTextOffsetY;

        public float UnitTextOffsetX
        {
            get { return unitTextOffsetX; }
            set { unitTextOffsetX = value; }
        }

        public float UnitTextOffsetY
        {
            get { return unitTextOffsetY; }
            set { unitTextOffsetY = value; }
        }

        public override int TextOffsetX
        {
            get { return (int) (unitTextOffsetX * ScaleMultiplier); }
            set { unitTextOffsetX = value / ScaleMultiplier; }
        }

        public override int TextOffsetY
        {
            get { return (int) (unitTextOffsetY * ScaleMultiplier); }
            set { unitTextOffsetY = value / ScaleMultiplier; }
        }

        #endregion

        #region Image 

        private float unitImageOffsetX, unitImageOffsetY;

        public float UnitImageOffsetX
        {
            get { return unitImageOffsetX; }
            set { unitImageOffsetX = value; }
        }

        public float UnitImageOffsetY
        {
            get { return unitImageOffsetY; }
            set { unitImageOffsetY = value; }
        }

        public override int ImageOffsetX
        {
            get { return (int) (unitImageOffsetX * ScaleMultiplier); }
            set { unitImageOffsetX = value / ScaleMultiplier; }
        }

        public override int ImageOffsetY
        {
            get { return (int) (unitImageOffsetY * ScaleMultiplier); }
            set { unitImageOffsetY = value / ScaleMultiplier; }
        }


        #endregion

        public ScalableFrame(FrameProcessor ui) : base(ui)
        {
        }

        public ScalableFrame(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor)
            : base(
                ui, (int) (x * ScaleMultiplier), (int) (y * ScaleMultiplier), (int) (w * ScaleMultiplier),
                (int) (h * ScaleMultiplier), text, backColor)
        {
            //base.Ghost = true;
            UnitX = x;
            UnitY = y;
            UnitWidth = w;
            UnitHeight = h;

            ApplicationInterface.Instance.onScaleUpdate += UpdateScale;
        }

        public void UpdateScale()
        {
            UpdateScale(ScaleMultiplier);
        }

        public virtual void UpdateScale(float scale)
        {
            foreach (var frame in Children)
            {
                if (frame is ScalableFrame)
                    ((ScalableFrame) frame).UpdateScale();
            }
            UpdateMove();
            UpdateResize(false);

        }        
    }

    public class FullScreenFrame<T> : FullScreenFrame where T : ScalableFrame
    {
        public FullScreenFrame(FrameProcessor ui) : base(ui)
        {
        }

        public T Item;
    }

    public class FullScreenFrame : ScalableFrame
    {
        public FullScreenFrame(FrameProcessor ui) : base(ui)
        {
            X = 0;
            Y = 0;
            Width = ui.Game.RenderSystem.DisplayBounds.Width;
            Height = ui.Game.RenderSystem.DisplayBounds.Height;
        }

        protected override void Update(GameTime time)
        {
            base.Update(time);
            X = 0;
            Y = 0;
            Width = ui.Game.RenderSystem.DisplayBounds.Width;
            Height = ui.Game.RenderSystem.DisplayBounds.Height;
        }        
    }
}
