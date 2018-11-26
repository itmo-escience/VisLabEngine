using ForumProject.ScenarioManager.Legenda;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;

namespace FusionUI.UI.Elements.Legenda
{
    public class PaletteLegendExplicitColor : PaletteLegend
    {
        public Color ColorLeft, ColorRight;

        public override void Init()
        {
            base.Init();
            LegendFrame.X = LegendFrame.X;
            palette.ActionDraw += (time, rs, cri) =>
            {
                var whiteTex = Game.Instance.RenderSystem.WhiteTexture;
                rs.DrawBeam(whiteTex, new Vector2(palette.GlobalRectangle.Left, palette.GlobalRectangle.Center.Y), new Vector2(palette.GlobalRectangle.Right, palette.GlobalRectangle.Center.Y), ColorLeft, ColorRight, palette.GlobalRectangle.Height, clipRectIndex:cri);
            };
        }
    }
}
