using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using FusionUI;
using FusionUI.UI;

namespace FusionUI.Legenda
{
	public class PaletteLegend : ILegend
	{
		LayoutFrame legendFrame = null;


		public virtual void Init()
		{
			
		}


	    public string Name { get; set; } = "";

	    public float Width { get; set; } = UIConfig.UnitLegendWidth;

        public string PaletteName { get; set; }
	    private string minValue, maxValue;	    

	    public string MinValue
	    {
	        get { return minValue; }
            set
	        {
	            minValue = value;
	            if (minLabel != null) minLabel.Text = minValue;
	        }
	    }

	    public string MaxValue
	    {
	        get { return maxValue; }
	        set
	        {
	            maxValue = value;
                if (maxLabel != null) maxLabel.Text = maxValue;
	        }
	    }

	    public void UpdateData(string name = null, string min = null, string max = null)
	    {
	        if (name != null)
	        {
	            caption.Text = name;
	        }

	        if (min != null)
	        {
	            MinValue = min;
	        }

	        if (max != null)
	        {
	            MaxValue = max;
	        }
	    }

        protected ScalableFrame minLabel, maxLabel;
	    protected ScalableFrame palette;
	    protected ScalableFrame caption;
		public ScalableFrame LegendFrame
		{
			get
			{
				if (legendFrame == null)
				{
					var ui = ApplicationInterface.Instance.FrameProcessor;
					legendFrame = new LayoutFrame(ui, 0, 0, Width, 0, Color.Zero, LayoutType.Vertical)
					{
					};
					caption = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight * (Name.Count(a => a == '\n') + 1), Name, Color.Zero) {
						FontHolder	= UIConfig.FontBody,
						UnitPadding = 1,
					};
					legendFrame.Add(caption);

				    palette = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight, "", Color.Zero) {
						UnitTextOffsetX = UIConfig.UnitLegendElementHeight,
						TextAlignment = Alignment.MiddleLeft,
						ImageColor = Color.White,
						Image		= string.IsNullOrEmpty(PaletteName) ? null : new DiscTexture(ui.Game.RenderSystem, ui.Game.Content.Load<Texture2D>(PaletteName)),
						ImageMode	= FrameImageMode.Stretched,
						ClippingMode = ClippingMode.ClipByPadding,
						UnitVPadding = 2,
						UnitImageOffsetX = -(Width - UIConfig.UnitLegendElementHeight) / 2,
					};
					legendFrame.Add(palette);

					var horLay = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight, "", Color.Zero)
					{
					};

					horLay.Add(minLabel = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight, MinValue, Color.Zero)
					{
					});
					horLay.Add(maxLabel = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight, MaxValue, Color.Zero) {
						TextAlignment = Alignment.MiddleRight,                        
					});

					legendFrame.Add(horLay);
					
				}
				return legendFrame;
			}
		}
	}
}
