using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Plots
{
    class PlotLegend : ScalableFrame
    {
		protected PlotLegend()
		{
		}
		public float UnitLineLength = 8;
        public float ElementWidth = 42;
        public float ElementHeight = 6;
        public float UnitLineWidth = 0.5f;
        public float MinXOffset = 3;
        public float YOffset = 6;
        public List<PlotMapPoint> PlotData = new List<PlotMapPoint>();

        private static long  t;

        [Flags]
        public enum LegendSettings
        {
            DrawStaticLines = 1,
            DrawAnimLines = 2,
        }

        public LegendSettings Settings;

        public bool IsNewFrame(GameTime time)
        {
            var t1 = time.Total.Milliseconds;
            if (t1 != t)
            {
                t = t1;
                return true;
            }
            return false;
        }

        private static int ClipRectId;

        public PlotLegend(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {

        }

        private List<Tuple<Color, String>> currentPlots;
        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);

            currentPlots = new List<Tuple<Color, string>>();
            foreach (var point in PlotData)
            {
                if (!point.Active) continue;
                foreach (var plot in point.Plots.Values)
                {
                    if (!plot.Ready) continue;
                    if (plot.AnimActive && (Settings & LegendSettings.DrawAnimLines) != 0)
                        currentPlots.Add(new Tuple<Color, string>(plot.AnimColor, $"({point.Index}){plot.Variable.NiceName}(Animated)"));
                    if (plot.Active && (Settings & LegendSettings.DrawStaticLines) != 0)
                    {
                        foreach (var d in plot.ActiveDepths)
                        {
                            currentPlots.Add(new Tuple<Color, string>(plot.Colors[d],
                                $"({point.Index}){plot.Variable.NiceName} at {d:0.##} {plot.Variable.DepthUnits}"));
                        }
                    }
                }
            }

            var whiteTex = Game.Instance.RenderSystem.WhiteTexture;
            var rect = GetBorderedRectangle();
            int hCount = Math.Max(1, Math.Min((int) Math.Floor((UnitWidth + MinXOffset )/ (ElementWidth + MinXOffset)), currentPlots.Count));
            int i = 0;
            if (IsNewFrame(gameTime)) ClipRectId = 800;
            int selected = -1;
            string selectedString = "";
            foreach (var colorData in currentPlots)
            {
                ClipRectId++;
                if (ClipRectId > 928) break; // too much elements
                string s = colorData.Item2;

                var sRect = Font.MeasureStringF(s);
                float left = rect.Left + (i % hCount) * (UnitWidth / hCount) * ScaleMultiplier;
                float top = rect.Top + ElementHeight * (i / hCount) * ScaleMultiplier;
                float width = (Width / hCount);
                float height = ElementHeight * ScaleMultiplier;
                var clipRect = new RectangleF(left, top, width, height);
                spriteLayer.SetClipRectangle(ClipRectId, (Rectangle) clipRect, Color.White);
                float lineX = left;
                float lineY = top + (ElementHeight / 2 - UnitLineWidth / 2) * ScaleMultiplier;
                spriteLayer.Draw(whiteTex, lineX, lineY, UnitLineLength * ScaleMultiplier,
                    UnitLineWidth * ScaleMultiplier, colorData.Item1, ClipRectId);

                float textX = left + (UnitLineLength + MinXOffset) * ScaleMultiplier;
                float textY = (float) Math.Floor(top + height / 2 + sRect.Height / 2);
                //if (clipRect.Contains(Game.Mouse.Position))
                //{
                //    selected = i;
                //    selectedString = s;
                //}
                //else
                //{
                    Font.DrawString(spriteLayer, s, textX, textY, Color.White, ClipRectId);
                //}

                i++;
            }

            //if (selected > 0)
            //{
            //    var sRect = Font.MeasureStringF(selectedString);
            //    float left = rect.Left + (selected % hCount) * (UnitWidth / hCount) * ScaleMultiplier;
            //    float height = ElementHeight * ScaleMultiplier;
            //    float top = rect.Top + ElementHeight * (selected / hCount) * ScaleMultiplier;
            //    float textX = left + (UnitLineLength + MinXOffset) * ScaleMultiplier;
            //    float textY = (float)Math.Floor(top + height / 2 + sRect.Height / 2);
            //    spriteLayer.Draw(whiteTex, textX, textY, sRect.Width,
            //        sRect.Height, UIConfig.PopupColor, 0);
            //    Font.DrawString(spriteLayer, selectedString, textX, textY, Color.White, 0);
            //}
            UnitHeight = (float)Math.Ceiling((float) (i) / hCount) * ElementHeight;
        }

        public override string Tooltip
        {
            get
            {
                var pos = Game.Instance.Mouse.Position - (Vector2)this.GlobalRectangle.TopLeft;
                int hCount = Math.Max(1, (int)Math.Floor((UnitWidth + MinXOffset) / (ElementWidth + MinXOffset)));
                float ew = Width / hCount;
                float eh = ElementHeight * ScaleMultiplier;
                int i = (int) (Math.Floor(pos.Y / eh) * hCount + Math.Floor(pos.X / ew));
                return i >= 0 && i < currentPlots.Count ? currentPlots[i].Item2 : "";
            }
            set { }
        }
    }
}
