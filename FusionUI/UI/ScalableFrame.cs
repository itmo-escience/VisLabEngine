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
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.ComponentModel;

namespace FusionUI.UI
{
	public class ScalableFrame : ControllableFrame
    {
        public int order = 0;

        public static float ScaleMultiplier
        {
            get { return (ApplicationInterface.ScaleMod); }
        }
		[XmlIgnore]
		public UIConfig.FontHolder FontHolder = new UIConfig.FontHolder(@"fonts\new\Base");
		[XmlIgnore]
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
				OnPropertyChanged();
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
				OnPropertyChanged();
			}
        }
		[XmlIgnore]
		public Action<float> OnCoordUpdate;

        public virtual float UnitWidth
        {
            get { return unitWidth; }
            set { unitWidth = value;
				OnPropertyChanged();
			}
        }

        public virtual float UnitHeight
        {
            get { return unitHeight; }
            set { unitHeight = value;
				OnPropertyChanged();
			}
        }

        public override int X
        {
            get { return (int) Math.Round(UnitX * ScaleMultiplier); }
            set
            {
                UnitX = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int Y
        {
            get { return (int) Math.Round(UnitY * ScaleMultiplier); }
            set
            {
                UnitY = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int Width
        {
            get { return (int) Math.Round(UnitWidth * ScaleMultiplier); }
            set { UnitWidth = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int Height
        {
            get { return (int) Math.Round(UnitHeight * ScaleMultiplier); }
            set { UnitHeight = value / ScaleMultiplier;
				OnPropertyChanged(); }
        }

        public override string Text
        {
            get { return text; }
            set
            {
                text = value;                
                DefaultText = value.Trim();
                UpdateLanguage();
				OnPropertyChanged();
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

            catch (Exception e)

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
            catch (Exception e)
            {
                //if (!string.IsNullOrWhiteSpace(DefaultText) && !double.TryParse(DefaultText, out var n)) Log.Warning("Missing translation for string \"{0}\"", DefaultText);
                text = DefaultText;
                TryLoadResource = false;
            }

            
        }



        public string DefaultText = "_";
       

        #region Padding

        private float unitPaddingTop, unitPaddingBottom, unitPaddingLeft, unitPaddingRight;

        public override int PaddingBottom
        {
            get { return (int) (UnitPaddingBottom * ScaleMultiplier); }
            set { UnitPaddingBottom = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int PaddingTop
        {
            get { return (int) (UnitPaddingTop * ScaleMultiplier); }
            set { UnitPaddingTop = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int PaddingLeft
        {
            get { return (int) (unitPaddingLeft * ScaleMultiplier); }
            set { UnitPaddingLeft = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int PaddingRight
        {
            get { return (int) (UnitPaddingRight * ScaleMultiplier); }
            set { UnitPaddingRight = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public virtual float UnitPaddingLeft
        {
            get { return unitPaddingLeft; }
            set { unitPaddingLeft = value;
				OnPropertyChanged();
			}
        }

        public virtual float UnitPaddingRight
        {
            get { return unitPaddingRight; }
            set { unitPaddingRight = value;
				OnPropertyChanged();
			}
        }

        public virtual float UnitPaddingTop
        {
            get { return unitPaddingTop; }
            set { unitPaddingTop = value;
				OnPropertyChanged();
			}
        }

        public virtual float UnitPaddingBottom
        {
            get { return unitPaddingBottom; }
            set { unitPaddingBottom = value;
				OnPropertyChanged();
			}
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
            set { unitTextOffsetX = value;
				OnPropertyChanged();
			}
        }

        public float UnitTextOffsetY
        {
            get { return unitTextOffsetY; }
            set { unitTextOffsetY = value;
				OnPropertyChanged();
			}
        }

        public override int TextOffsetX
        {
            get { return (int) (UnitTextOffsetX * ScaleMultiplier); }
            set { UnitTextOffsetX = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int TextOffsetY
        {
            get { return (int) (UnitTextOffsetY * ScaleMultiplier); }
            set { UnitTextOffsetY = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        #endregion

        #region Image 

        private float unitImageOffsetX, unitImageOffsetY;

        public float UnitImageOffsetX
        {
            get { return unitImageOffsetX; }
            set { unitImageOffsetX = value;
				OnPropertyChanged();
			}
        }

        public float UnitImageOffsetY
        {
            get { return unitImageOffsetY; }
            set { unitImageOffsetY = value;
				OnPropertyChanged();
			}
        }

        public override int ImageOffsetX
        {
            get { return (int) (UnitImageOffsetX * ScaleMultiplier); }
            set { UnitImageOffsetX = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }

        public override int ImageOffsetY
        {
            get { return (int) (UnitImageOffsetY * ScaleMultiplier); }
            set { UnitImageOffsetY = value / ScaleMultiplier;
				OnPropertyChanged();
			}
        }


		#endregion

		protected ScalableFrame()
		{
		}

		public ScalableFrame(FrameProcessor ui) : base(ui)
        {
            ForeColor = UIConfig.ActiveTextColor;
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
            ForeColor = UIConfig.ActiveTextColor;
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

		#region Serialization
		/*-----------------------------------------------------------------------------------------
         * 
         *	Serialization :
         * 
        -----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Serializes the frame.
		/// </summary>
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(this.Height);
			writer.Write(this.ImageOffsetX);
			writer.Write(this.ImageOffsetY);
			writer.Write(this.PaddingBottom);
			writer.Write(this.PaddingLeft);
			writer.Write(this.PaddingRight);
			writer.Write(this.PaddingTop);
			writer.Write(this.Text??"");
			writer.Write(this.TextOffsetX);
			writer.Write(this.TextOffsetY);

			writer.Write(this.UnitHeight);
			writer.Write(this.UnitImageOffsetX);
			writer.Write(this.UnitImageOffsetY);
			writer.Write(this.UnitPaddingBottom);
			writer.Write(this.UnitPaddingLeft);
			writer.Write(this.UnitPaddingRight);
			writer.Write(this.UnitPaddingTop);
			writer.Write(this.UnitTextOffsetX);
			writer.Write(this.UnitTextOffsetY);
			writer.Write(this.UnitImageOffsetX);
			writer.Write(this.UnitWidth);
			writer.Write(this.UnitX);
			writer.Write(this.UnitY);

			writer.Write(this.Width);
			writer.Write(this.X);
			writer.Write(this.Y);

		}

		/// <summary>
		/// Deerializes the frame.
		/// </summary>
		public override void Deserialize(BinaryReader reader)
		{
			//this.FontHolder = new UIConfig.FontHolder(@"fonts\new\Base");

			base.Deserialize(reader);
			this.Height = reader.ReadInt32();
			this.ImageOffsetX = reader.ReadInt32();
			this.ImageOffsetY = reader.ReadInt32();
			this.PaddingBottom = reader.ReadInt32();
			this.PaddingLeft = reader.ReadInt32();
			this.PaddingRight = reader.ReadInt32();
			this.PaddingTop = reader.ReadInt32();
			this.Text = reader.ReadString();
			this.TextOffsetX = reader.ReadInt32();
			this.TextOffsetY = reader.ReadInt32();

			this.UnitHeight = reader.ReadSingle();
			this.UnitImageOffsetX = reader.ReadSingle();
			this.UnitImageOffsetY = reader.ReadSingle();
			this.UnitPaddingBottom = reader.ReadSingle();
			this.UnitPaddingLeft = reader.ReadSingle();
			this.UnitPaddingRight = reader.ReadSingle();
			this.UnitPaddingTop = reader.ReadSingle();
			this.UnitTextOffsetX = reader.ReadSingle();
			this.UnitTextOffsetY = reader.ReadSingle();
			this.UnitImageOffsetX = reader.ReadSingle();
			this.UnitWidth = reader.ReadSingle();
			this.UnitX = reader.ReadSingle();
			this.UnitY = reader.ReadSingle();

			this.Width = reader.ReadInt32();
			this.X = reader.ReadInt32();
			this.Y = reader.ReadInt32();

		}
		#endregion
	}

	public class FullScreenFrame<T> : FullScreenFrame where T : ScalableFrame
    {
		public FullScreenFrame(FrameProcessor ui) : base(ui)
        {
        }

        public T Item;

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml( XmlReader reader )
		{
			XmlSerializer itemSerializer = new XmlSerializer(typeof(T));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				T item = (T)itemSerializer.Deserialize(reader);

				this.Item = item;

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml( XmlWriter writer )
		{
			XmlSerializer itemSerializer = new XmlSerializer(typeof(T));



			writer.WriteStartElement("item");
			itemSerializer.Serialize(writer, this.Item);

			writer.WriteEndElement();
		}
	}


    public class FullScreenFrame : ScalableFrame
    {
		protected FullScreenFrame()
		{
		}

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
