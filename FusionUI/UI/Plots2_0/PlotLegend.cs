using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Plots2_0
{
    public class PlotLegend: ScalableFrame 
    {            
        public float UnitLineLength = 8;
        public float ElementWidth = 42;
        public float ElementHeight = 6;
        public float UnitLineWidth = 0.5f;
        public float MinXOffset = 3;
        public float YOffset = 6;
        public PlotContainer PlotData;

        private static long t;

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
            ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {                
                var oldDelta = delta;
                delta = oldDelta + args.DY;
                flag = true;
                if (delta > 0)
                {
                    delta = 0;
                    flag = false;
                }
                if (delta < ElementHeight * ScaleMultiplier * (MaxRowCount - rowCount - 1))
                {
                    delta = ElementHeight * ScaleMultiplier * (MaxRowCount - rowCount - 1);
                    flag = false;
                }                
                args.MoveDelta.Y = args.MoveDelta.Y - (delta - oldDelta);
            };

            this.MouseWheel += (sender, args) =>
            {
                delta = delta + args.Wheel * 0.05f;                
                if (delta > 0)
                {
                    delta = 0;                    
                }
                if (delta < ElementHeight * ScaleMultiplier * (MaxRowCount - rowCount - 1))
                {
                    delta = ElementHeight * ScaleMultiplier * (MaxRowCount - rowCount - 1);
                }
            };
        }

        protected List<Tuple<Color, String, bool>> currentPlots;

        public Func<string, string> textFunc;

        protected virtual void DeterminePlots()
        {
            currentPlots = new List<Tuple<Color, string, bool>>();
            foreach (var variable in PlotData.Data)
            {
                if (!variable.Value.IsPresent) continue;                
                foreach (var data in variable.Value.Data)
                {
                    if (!data.Value.Any(a => a.Value.IsPresent)) continue;
                    foreach (var kv in data.Value)
                    {                        
                        foreach (var depth in kv.Value.ActiveDepths)
                        {
                            if (kv.Value.ColorsByDepth.ContainsKey(depth))
                                currentPlots.Add(new Tuple<Color, string, bool>(kv.Value.ColorsByDepth[depth],
                                    $"({data.Key.Index}){kv.Key}{(kv.Value.ActiveDepths.Count > 1 ? " at {depth:0.##}" : "")} {variable.Value.CatUnits}", kv.Value.IsBarChart));
                        }
                    }
                }
            }            
        }

        private float delta = 0;
        private int elementCount = 0, rowCount = 0;
        public int MaxRowCount = 3;
        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            delta = MathUtil.Clamp(delta, ElementHeight * ScaleMultiplier * (MaxRowCount - rowCount - 1), 0);
            base.DrawFrame(gameTime, spriteLayer, clipRectIndex);            
            var delta1 = delta + ElementHeight / 2 * ScaleMultiplier;
            if (rowCount <= MaxRowCount) delta1 = 0;
            DeterminePlots();

            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            var rect = GetBorderedRectangle();
            int hCount = Math.Max(1, Math.Min((int)Math.Floor((UnitWidth + MinXOffset) / (ElementWidth + MinXOffset)), currentPlots.Count));
            int i = 0;
            if (IsNewFrame(gameTime)) ClipRectId = 800;
            int selected = -1;
            string selectedString = "";
            foreach (var colorData in currentPlots)
            {
                ClipRectId++;
                if (ClipRectId > 1256) break; // too much elements
                string s = colorData.Item2;

                var sRect = Font.MeasureStringF(s);
                float left = rect.Left + (i % hCount) * (UnitWidth / hCount) * ScaleMultiplier;
                float top = rect.Top + ElementHeight * (i / hCount) * ScaleMultiplier;
                float width = (Width / hCount);
                float height = ElementHeight * ScaleMultiplier;
                var clipRect = RectangleF.Intersect(new RectangleF(left, top + delta1, width, height), this.GlobalRectangle);
                spriteLayer.SetClipRectangle(ClipRectId, (Rectangle)clipRect, Color.White);
                float lineX = left;

                var w = !colorData.Item3 ? UnitLineWidth : ElementHeight - UnitLineWidth;
                float lineY = top + (ElementHeight / 2 - w / 2) * ScaleMultiplier + delta1;
                spriteLayer.Draw(whiteTex, lineX, lineY, UnitLineLength * ScaleMultiplier,
                    w * ScaleMultiplier, colorData.Item1, ClipRectId);

                float textX = left + (UnitLineLength + MinXOffset) * ScaleMultiplier;
                float textY = (float)Math.Floor(top + height / 2 + sRect.Height / 2 + delta1);
                //if (clipRect.Contains(Game.Mouse.Position))
                //{
                //    selected = i;
                //    selectedString = s;
                //}
                //else
                //{
                Font.DrawString(spriteLayer, s, textX, textY, UIConfig.ActiveTextColor, ClipRectId);
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
            elementCount = i;
            rowCount = (int) Math.Ceiling((float) (elementCount) / hCount);
            UnitHeight = ElementHeight * Math.Min(MaxRowCount, rowCount);

            if (rowCount > MaxRowCount)
            {
                //Draw scroll line
                float d = (float)MaxRowCount / rowCount;

                float deltaMul = rowCount > MaxRowCount ? delta1 / (ElementHeight * (MaxRowCount - rowCount - 1)) : 1;
                spriteLayer.Draw(whiteTex, GlobalRectangle.Right - UIConfig.ScrollBarWidth * ScaleMultiplier,
                    GlobalRectangle.Top, UIConfig.ScrollBarWidth * ScaleMultiplier, Height, UIConfig.ButtonColor,
                    clipRectIndex);
                spriteLayer.Draw(whiteTex, GlobalRectangle.Right - UIConfig.ScrollBarWidth * ScaleMultiplier,
                    GlobalRectangle.Top + Height * (1 - d) * deltaMul, UIConfig.ScrollBarWidth * ScaleMultiplier,
                    Height * d, UIConfig.ActiveColor, clipRectIndex);
            }
        }

        public override string Tooltip
        {
            get
            {
                var pos = Game.Mouse.Position - (Vector2)this.GlobalRectangle.TopLeft;
                int hCount = Math.Max(1, (int)Math.Floor((UnitWidth + MinXOffset) / (ElementWidth + MinXOffset)));
                float ew = Width / hCount;
                float eh = ElementHeight * ScaleMultiplier;
                int i = (int)(Math.Floor(pos.Y / eh) * hCount + Math.Floor(pos.X / ew));
                return i >= 0 && i < currentPlots.Count ? currentPlots[i].Item2 : "";
            }
            set { }
        }
    }
}
