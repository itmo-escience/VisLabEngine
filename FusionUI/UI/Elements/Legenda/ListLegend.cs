using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using FusionUI;
using FusionUI.UI;
using FusionUI.UI.Elements.TextFormatting;

namespace FusionUI.Legenda
{
    public class ListLegend : ILegend
    {
        public virtual void Init()
        {
        }

        
                
        public string Name { get; set; }
        public float Width { get; set; } = UIConfig.UnitLegendWidth;

        LayoutFrame legendFrame = null;

        public List<Tuple<string, Color>> CatList;


        public ScalableFrame LegendFrame
        {
            get
            {
                if (legendFrame == null)
                {
                    var ui = ApplicationInterface.Instance.FrameProcessor;
                    legendFrame = new LayoutFrame(ApplicationInterface.Instance.FrameProcessor, 0, 0, Width, 0, Color.Zero, LayoutType.Vertical);
                    if (!String.IsNullOrEmpty(Name))
                    {
                        var Caption = new FormatTextBlock(ui, 0, 0, Width,
                            UIConfig.UnitLegendElementHeight, Name, Color.Zero, UIConfig.FontSubtitle)
                        {                            
                            UnitPadding = 3,
                        };
                        legendFrame.Add(Caption);
                    }
                    foreach (var pair in CatList)
                    {
                        var entry = new ScalableFrame(ui, 0, 0, Width, UIConfig.UnitLegendElementHeight, pair.Item1, Color.Zero)
                        {
                            Image = ui.Game.Content.Load<DiscTexture>(@"circle"),
                            UnitTextOffsetX = UIConfig.UnitLegendElementHeight + 2,
                            TextAlignment = Alignment.MiddleLeft,
                            ImageColor = pair.Item2,
                            ImageMode = FrameImageMode.Fitted,
                            ClippingMode = ClippingMode.ClipByPadding,
                            UnitVPadding = 2,
                            UnitImageOffsetX = -(Width)/2 + (UIConfig.UnitLegendElementHeight - 2),                            
                        };
                        legendFrame.Add(entry);
                    }
                    //(ApplicationInterface.Instance as ApplicationInterface).ScreenComposer.LegendHolder.Add(legendFrame);
                }
                return legendFrame;
            }
        }
    }
}
