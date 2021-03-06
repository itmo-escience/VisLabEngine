﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace FusionUI.UI.Elements
{
    public class ArrowController
    {
        private static ArrowController instance = null;

        public static ArrowController Instance
        {
            get { return instance ?? (instance = new ArrowController()); }
        }        

        public Dictionary<ScalableFrame, List<ConnectorArrow>> ArrowsByFrame
        {
            get
            {
                return OutArrowsByFrame.Keys.Union(InArrowsByFrame.Keys)
                    .ToDictionary(a => a, a => (OutArrowsByFrame.ContainsKey(a) ? OutArrowsByFrame[a] : new List<ConnectorArrow>()).Union(InArrowsByFrame.ContainsKey(a) ? InArrowsByFrame[a].ToList() : new List<ConnectorArrow>()).ToList());
            }
        }

        public Dictionary<ScalableFrame, List<ConnectorArrow>> OutArrowsByFrame = new Dictionary<ScalableFrame, List<ConnectorArrow>>();
        public Dictionary<ScalableFrame, List<ConnectorArrow>> InArrowsByFrame = new Dictionary<ScalableFrame, List<ConnectorArrow>>();


        public void AddArrow(ConnectorArrow arrow)
        {            
            if (!OutArrowsByFrame.ContainsKey(arrow.FromFrame)) OutArrowsByFrame.Add(arrow.FromFrame, new List<ConnectorArrow>());
            if (!OutArrowsByFrame.ContainsKey(arrow.ToFrame)) OutArrowsByFrame.Add(arrow.ToFrame, new List<ConnectorArrow>());
            OutArrowsByFrame[arrow.FromFrame].Add(arrow);
            if (!InArrowsByFrame.ContainsKey(arrow.ToFrame)) InArrowsByFrame.Add(arrow.ToFrame, new List<ConnectorArrow>());
            if (!InArrowsByFrame.ContainsKey(arrow.FromFrame)) InArrowsByFrame.Add(arrow.FromFrame, new List<ConnectorArrow>());
            InArrowsByFrame[arrow.ToFrame].Add(arrow);            
        }
        
    }

    public class ConnectorArrow : ScalableFrame
    {
        public ScalableFrame FromFrame, ToFrame;
        public float ArrowWidth, ArrowPointerSize, DotMarkerSize;
        public int ForceInDirection = 0, ForceOutDirection = 0;
        
        public bool IsStraight = false;

        public bool DoStraighten
        {
            set
            {
                DoStraightenFrom = value;
                DoStraightenTo = value;
            }
        }
        public bool DoStraightenFrom = true;
        public bool DoStraightenTo = true;

        public bool IsAnimation = false;
        public float AnimVelocityMult = 2f;
        public float GlobalAnimvelocity = 50;
        private float animProgress = 0;


        public static float ArrowSpread = 20;
        private static int Index = 0;
        private int myIndex = Index++;

        public bool IsDottedLine = false;
        public float LineDotSizeGU = 1;
        public float LineSpaceSizeGU = 1;
        public ConnectorArrow(FrameProcessor ui, ScalableFrame from, ScalableFrame to, float width, float arrowSize, float markerSize) : base(ui)
        {
            FromFrame = from;
            ToFrame = to;
            ArrowWidth = width;
            ArrowPointerSize = arrowSize;
            DotMarkerSize = markerSize;
            ArrowController.Instance.AddArrow(this);
            X = 0;
            Y = (int)(4 * ScalableFrame.ScaleMultiplier);
            Width = 1;
            Height = 1;
        }
       
        protected override void Update(GameTime gameTime)
        {
            animProgress += gameTime.ElapsedSec * AnimVelocityMult;
            animProgress = animProgress - (float)Math.Floor(animProgress);
            base.Update(gameTime);
        }

        public int StartOffset = 0, EndOffset = 0;
        private int inDirection, outDirection;
        protected override void DrawFrame(GameTime gameTime, SpriteLayer spriteLayer, int clipRectIndex)
        {
            int xDir = FromFrame.GetBorderedRectangle().Center.X > ToFrame.GetBorderedRectangle().Center.X
                ? FromFrame.GetBorderedRectangle().Left > ToFrame.GetBorderedRectangle().Right ? -1 : 0
                : FromFrame.GetBorderedRectangle().Right < ToFrame.GetBorderedRectangle().Left
                    ? 1
                    : 0;
            int yDir = FromFrame.GetBorderedRectangle().Center.Y > ToFrame.GetBorderedRectangle().Center.Y
                ? FromFrame.GetBorderedRectangle().Top > ToFrame.GetBorderedRectangle().Bottom ? -1 : 0
                : FromFrame.GetBorderedRectangle().Bottom < ToFrame.GetBorderedRectangle().Top
                    ? 1
                    : 0;
            xDir++;
            yDir++;
            inDirection = 0; outDirection = 0;
            Vector2 start = new Vector2(), end = new Vector2();
            switch (xDir * 3 + yDir)
            {
                case 0:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 3;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 2;
                    break;
                case 1:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 3;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 1;
                    break;
                case 2:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 3;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 4;
                    break;
                case 3:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 4;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 2;
                    break;
                case 4: break;
                case 5:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 2;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 4;
                    break;
                case 6:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 1;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 2;
                    break;
                case 7:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 1;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 3;
                    break;
                case 8:
                    outDirection = ForceOutDirection > 0 ? ForceOutDirection : 1;
                    inDirection = ForceInDirection > 0 ? ForceInDirection : 4;
                    break;
            }

            switch (outDirection)
            {
                case 1: start = new Vector2(FromFrame.GlobalRectangle.Right, FromFrame.GlobalRectangle.Center.Y); break;
                case 2: start = new Vector2(FromFrame.GlobalRectangle.Center.X, FromFrame.GlobalRectangle.Bottom); break;
                case 3: start = new Vector2(FromFrame.GlobalRectangle.Left, FromFrame.GlobalRectangle.Center.Y); break;
                case 4: start = new Vector2(FromFrame.GlobalRectangle.Center.X, FromFrame.GlobalRectangle.Top); break;
            }

            switch (inDirection)
            {
                case 1: end = new Vector2(ToFrame.GlobalRectangle.Right, ToFrame.GlobalRectangle.Center.Y); break;
                case 2: end = new Vector2(ToFrame.GlobalRectangle.Center.X, ToFrame.GlobalRectangle.Bottom); break;
                case 3: end = new Vector2(ToFrame.GlobalRectangle.Left, ToFrame.GlobalRectangle.Center.Y); break;
                case 4: end = new Vector2(ToFrame.GlobalRectangle.Center.X, ToFrame.GlobalRectangle.Top); break;
            }


            #region multipleArrows

            var startArrows = ArrowController.Instance.OutArrowsByFrame[FromFrame]
                .Where(a => a.outDirection == outDirection)
                .Concat(ArrowController.Instance
                    .InArrowsByFrame[FromFrame]
                    .Where(a => a.inDirection == outDirection)
                    ).OrderBy(a => a.myIndex).Where(a => !(a.ToFrame == FromFrame && a.FromFrame == ToFrame)).ToList();

            var endarrows = ArrowController.Instance.InArrowsByFrame[ToFrame].Where(a => a.inDirection == inDirection).Concat(ArrowController.Instance
                .OutArrowsByFrame[ToFrame]
                .Where(a => a.outDirection == inDirection)
                ).OrderBy(a => a.myIndex).Where(a => !(a.ToFrame == FromFrame && a.FromFrame == ToFrame)).ToList();

            float startSpread = DoStraightenFrom ? ((outDirection % 2 == 1) ? FromFrame.Height : FromFrame.Width) - ArrowPointerSize : ArrowSpread;
            float endSpread = DoStraightenTo ? ((inDirection % 2 == 1) ? ToFrame.Height : ToFrame.Width) - ArrowPointerSize : ArrowSpread;
            if (startArrows.Count() > 1)
            {
                if (DoStraightenFrom) startSpread /= (startArrows.Count - 1);
                if (outDirection % 2 == 1)
                {
                    start.Y = FromFrame.GlobalRectangle.Center.Y - ArrowSpread * ((float)(startArrows.Count - 1) / 2 - startArrows.IndexOf(this)) + StartOffset;

                }
                else
                {
                    start.X = FromFrame.GlobalRectangle.Center.X - ArrowSpread * ((float)(startArrows.Count - 1) / 2 - startArrows.IndexOf(this)) + StartOffset;
                }
            }
            else
            {
                if (outDirection % 2 == 1)
                {
                    start.Y += StartOffset;

                }
                else
                {
                    start.X +=  StartOffset;
                }
            }

            if (endarrows.Count() > 1)
            {
                if (DoStraightenTo) endSpread /= (endarrows.Count - 1);
                if (inDirection % 2 == 1)
                {
                    end.Y = ToFrame.GlobalRectangle.Center.Y - ArrowSpread * ((float)(endarrows.Count - 1) / 2 - endarrows.IndexOf(this)) + EndOffset;
                }
                else
                {
                    end.X = ToFrame.GlobalRectangle.Center.X - ArrowSpread * ((float)(endarrows.Count - 1) / 2 - endarrows.IndexOf(this)) + EndOffset;
                }
            }
            else
            {
                if (inDirection % 2 == 1)
                {
                    end.Y += EndOffset;
                }
                else
                {
                    end.X += EndOffset;
                }
            }

            if (DoStraightenFrom)
            {
                if (outDirection % 2 == 1)
                {
                    start.Y = (int)DMathUtil.Clamp(end.Y, start.Y - startSpread / 2, start.Y + startSpread / 2);
                }
                else
                {
                    start.X = (int)DMathUtil.Clamp(end.X, start.X - startSpread / 2, start.X + startSpread / 2);
                }
            }

            if (DoStraightenTo)
            {

                if (inDirection % 2 == 1)
                {
                    end.Y = (int)DMathUtil.Clamp(start.Y, end.Y - endSpread / 2, end.Y + endSpread / 2);
                }
                else
                {
                    end.X = (int)DMathUtil.Clamp(start.X, end.X - endSpread / 2, end.X + endSpread / 2);
                }
            }

            #endregion



            if (Math.Abs(start.X - end.X) <= 10) start.X = end.X = (start.X + end.X) / 2;
            if (Math.Abs(start.Y - end.Y) <= 10) start.Y = end.Y = (start.Y + end.Y) / 2;

            var whiteTex = Game.Content.Load<DiscTexture>("UI/beam");//this.Game.RenderSystem.WhiteTexture);

            bool isContrArrow = ArrowController.Instance.OutArrowsByFrame[ToFrame].Any(a => a.ToFrame == FromFrame);

            if ((inDirection + outDirection) % 2 == 1 && !IsStraight)
            {
                Vector2 center = outDirection % 2 == 0 ? new Vector2(start.X, end.Y) : new Vector2(end.X, start.Y);
                var d1 = center - start;
                var d2 = end - center;
                float l1 = d1.Length(), l2 = d2.Length();
                d1.Normalize(); d2.Normalize();
                if (isContrArrow) start += d1 * ArrowPointerSize / 4;
                end -= d2 * ArrowPointerSize / 4;
                DrawLine(spriteLayer, whiteTex, start + d1 * ArrowPointerSize / 4, center, BackColor, ArrowWidth);
                DrawLine(spriteLayer, whiteTex, center, end - d2 * ArrowPointerSize / 4, BackColor, ArrowWidth);
                DrawArrow(spriteLayer, start, end);
                if (IsAnimation)
                {
                    float length = l1 + l2;
                    var circleTex = Game.Content.Load<DiscTexture>("UIArrow");
                    for (int i = 0; i < (int)(length / GlobalAnimvelocity) + 1; i++)
                    {
                        float l = GlobalAnimvelocity * (i + animProgress);
                        if (l > length - DotMarkerSize || l < DotMarkerSize) continue;
                        if (l > l1)
                        {
                            var p = center + d2 * (l - l1);
                            var d = d2;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                        else
                        {
                            var p = start + d1 * l;
                            var d = d1;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                    }
                }
            }
            else if (inDirection == outDirection && !IsStraight)
            {
                Vector2 center1 = new Vector2(), center2 = new Vector2();
                switch (outDirection)
                {
                    case 1:
                        center1 = new Vector2((start.X + end.X) / 2 + 4 * ArrowPointerSize, start.Y);
                        center2 = new Vector2((start.X + end.X) / 2 + 4 * ArrowPointerSize, end.Y);
                        break;
                    case 2:
                        center1 = new Vector2(start.X, (start.Y + end.Y) / 2 + 4 * ArrowPointerSize);
                        center2 = new Vector2(end.X, (start.Y + end.Y) / 2 + 4 * ArrowPointerSize);
                        break;
                    case 3:
                        center1 = new Vector2((start.X + end.X) / 2 - 4 * ArrowPointerSize, start.Y);
                        center2 = new Vector2((start.X + end.X) / 2 - 4 * ArrowPointerSize, end.Y);
                        break;
                    case 4:
                        center1 = new Vector2(start.X, (start.Y + end.Y) / 2 - 4 * ArrowPointerSize);
                        center2 = new Vector2(end.X, (start.Y + end.Y) / 2 - 4 * ArrowPointerSize);
                        break;
                }

                var d1 = center1 - start;
                var d2 = center2 - center1;
                var d3 = end - center2;
                float l1 = d1.Length(), l2 = d2.Length(), l3 = d3.Length();
                d1.Normalize(); d2.Normalize(); d3.Normalize();
                if (isContrArrow) start += d1 * ArrowPointerSize / 4;
                end -= d3 * ArrowPointerSize / 4;
                DrawLine(spriteLayer, whiteTex, start + d1 * ArrowPointerSize / 4, center1, BackColor, ArrowWidth);
                DrawLine(spriteLayer, whiteTex, center1, center2, BackColor, ArrowWidth);
                DrawLine(spriteLayer, whiteTex, center2, end - d3 * ArrowPointerSize / 4, BackColor, ArrowWidth);
                DrawArrow(spriteLayer, start, end);
                if (IsAnimation)
                {
                    float length = l1 + l2 + l3;
                    var circleTex = Game.Content.Load<DiscTexture>("UIArrow");
                    for (int i = 0; i < (int)(length / GlobalAnimvelocity) + 1; i++)
                    {
                        float l = GlobalAnimvelocity * (i + animProgress);
                        if (l > length - DotMarkerSize || l < DotMarkerSize) continue;
                        if (l > l1 + l2)
                        {
                            var p = center2 + d3 * (l - l1 - l2);
                            var d = d3;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                        else if (l > l1)
                        {
                            var p = center1 + d2 * (l - l1);
                            var d = d2;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                        else
                        {
                            var p = start + d1 * l;
                            var d = d1;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                    }
                }

            }
            else if (!IsStraight)
            {

                Vector2 center1 = new Vector2(), center2 = new Vector2();
                switch (outDirection)
                {
                    case 1:
                        center1 = new Vector2((start.X + end.X) / 2, start.Y);
                        center2 = new Vector2((start.X + end.X) / 2, end.Y);
                        break;
                    case 2:
                        center1 = new Vector2(start.X, (start.Y + end.Y) / 2);
                        center2 = new Vector2(end.X, (start.Y + end.Y) / 2);
                        break;
                    case 3:
                        center1 = new Vector2((start.X + end.X) / 2, start.Y);
                        center2 = new Vector2((start.X + end.X) / 2, end.Y);
                        break;
                    case 4:
                        center1 = new Vector2(start.X, (start.Y + end.Y) / 2);
                        center2 = new Vector2(end.X, (start.Y + end.Y) / 2);
                        break;
                }

                var d1 = center1 - start;
                var d2 = center2 - center1;
                var d3 = end - center2;
                float l1 = d1.Length(), l2 = d2.Length(), l3 = d3.Length();
                d1.Normalize(); d2.Normalize(); d3.Normalize();
                if (isContrArrow) start += d1 * ArrowPointerSize / 4;
                end -= d3 * ArrowPointerSize / 4;
                DrawLine(spriteLayer, whiteTex, start + d1 * ArrowPointerSize / 4, center1, BackColor, ArrowWidth);
                DrawLine(spriteLayer, whiteTex, center1 - d2 * ArrowWidth * 0.33f * 0.5f, center2 + d2 * ArrowWidth * 0.33f * 0.5f, BackColor, ArrowWidth);
                DrawLine(spriteLayer, whiteTex, center2, end - d3 * ArrowPointerSize / 4, BackColor, ArrowWidth);
                DrawArrow(spriteLayer, start, end);
                if (IsAnimation)
                {
                    float length = l1 + l2 + l3;
                    var circleTex = Game.Content.Load<DiscTexture>("UIArrow");
                    for (int i = 0; i < (int)(length / GlobalAnimvelocity) + 1; i++)
                    {
                        float l = GlobalAnimvelocity * (i + animProgress);
                        if (l > length - DotMarkerSize || l < DotMarkerSize) continue;
                        if (l > l1 + l2)
                        {
                            var p = center2 + d3 * (l - l1 - l2);
                            var d = d3;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - ArrowWidth/4, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);

                        }
                        else if (l > l1)
                        {
                            var p = center1 + d2 * (l - l1);
                            var d = d2;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                        else
                        {
                            var p = start + d1 * l;
                            var d = d1;
                            var r = new Vector2(d.Y, -d.X);
                            spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                            //spriteLayer.Draw(circleTex, p.X - DotMarkerSize / 2, p.Y - DotMarkerSize / 2, ArrowWidth / 2, ArrowWidth / 2, BackColor);
                        }
                    }
                }


            }
            else
            {
                var d = end - start;
                var length = d.Length();
                d.Normalize();

                if (isContrArrow) start += d * ArrowPointerSize / 4;
                end -= d * ArrowPointerSize / 4;
                DrawLine(spriteLayer, whiteTex, start + d * ArrowPointerSize / 4, end - d * ArrowPointerSize / 4, BackColor, ArrowWidth);
                DrawArrow(spriteLayer, start, end);
                if (IsAnimation)
                {

                    var circleTex = Game.Content.Load<DiscTexture>("UIArrow");
                    for (int i = 0; i < (int)(length / GlobalAnimvelocity) + 1; i++)
                    {
                        float l = GlobalAnimvelocity * (i + animProgress);
                        if (l > length - DotMarkerSize || l < DotMarkerSize) continue;
                        var p = start + d * l;
                        var r = new Vector2(d.Y, -d.X);
                        spriteLayer.DrawFreeUV(circleTex, p + (-d + r) * DotMarkerSize / 2, p + (d + r) * DotMarkerSize / 2, p + (-d - r) * DotMarkerSize / 2, p + (d - r) * DotMarkerSize / 2, ForeColor,
                            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
                    }
                }

                //if (!string.IsNullOrWhiteSpace(Text))
                //{
                //    var b = Font.MeasureStringF(Text);
                //    var offset = (length - b.Width) / 2;

                //    Font.DrawString(spriteLayer, Text, (start + offset * d).X, (start + offset * d).Y, UIConfig.ActiveTextColor);
                //}

            }

            
        }

        private void DrawArrow(SpriteLayer spriteLayer, Vector2 start, Vector2 end)
        {
            var arrowTex = Game.Content.Load<DiscTexture>("UIArrow");
            if (IsStraight)
            {
                var forward = end - start;
                forward.Normalize();
                var right1 = Vector3.Cross(new Vector3(forward, 0), Vector3.BackwardRH);
                var right = new Vector2(right1.X, right1.Y);
                spriteLayer.DrawFreeUV(arrowTex,
                    end - right / 2 * ArrowPointerSize,
                    end + right / 2 * ArrowPointerSize,
                    end - right / 2 * ArrowPointerSize - forward * ArrowPointerSize,
                    end + right / 2 * ArrowPointerSize - forward * ArrowPointerSize, BackColor,
                    new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
            }
            else switch (inDirection)
                {

                    case 2:
                        spriteLayer.DrawFreeUV(arrowTex,
                    end + new Vector2(-ArrowPointerSize / 2, 0),
                    end + new Vector2(ArrowPointerSize / 2, 0),
                    end + new Vector2(-ArrowPointerSize / 2, ArrowPointerSize),
                    end + new Vector2(ArrowPointerSize / 2, ArrowPointerSize), BackColor,
                    new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
                        break;
                    case 4:
                        spriteLayer.DrawFreeUV(arrowTex,
                            end + new Vector2(-ArrowPointerSize / 2, 0),
                            end + new Vector2(ArrowPointerSize / 2, 0),
                            end + new Vector2(-ArrowPointerSize / 2, -ArrowPointerSize),
                            end + new Vector2(ArrowPointerSize / 2, -ArrowPointerSize), BackColor,
                            new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
                        break;
                    case 1:
                        spriteLayer.DrawFreeUV(arrowTex,
                            end + new Vector2(0, -ArrowPointerSize / 2),
                            end + new Vector2(0, ArrowPointerSize / 2),
                            end + new Vector2(ArrowPointerSize, -ArrowPointerSize / 2),
                            end + new Vector2(ArrowPointerSize, ArrowPointerSize / 2), BackColor,
                            new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
                        break;
                    case 3:
                        spriteLayer.DrawFreeUV(arrowTex,

                            end + new Vector2(0, -ArrowPointerSize / 2),
                            end + new Vector2(0, ArrowPointerSize / 2),
                            end + new Vector2(-ArrowPointerSize, -ArrowPointerSize / 2),
                            end + new Vector2(-ArrowPointerSize, ArrowPointerSize / 2), BackColor,
                            new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
                        break;
                }
        }

        private void DrawLine(SpriteLayer spriteLayer, Texture tex, Vector2 p0, Vector2 p1, Color color, float width, float scale = 1, float offset = 0, int clipRectIndex = 0)
        {
            if (!IsDottedLine)
            {
                spriteLayer.DrawBeam(tex, p0, p1, color, color, width);
            }
            else
            {
                if (p0.X > p1.X)
                {
                    var p = p1;
                    p1 = p0;
                    p0 = p;
                }
                var v = p1 - p0;
                float l = 0;
                var length = v.Length();
                v.Normalize();
                int i = 0;
                Vector2 v0 = p0;
                while (l < length)
                {
                    if (i % 2 == 0) 
                    {
                        //draw dot;
                        var v1 = v0 + v * Math.Min(LineDotSizeGU * ApplicationInterface.ScaleMod, length - l);
                        l += LineDotSizeGU * ApplicationInterface.ScaleMod;
                        spriteLayer.DrawBeam(tex, v0, v1, color, color, width);
                        v0 = v1;
                    }
                    else
                    {
                        //not draw;
                        var v1 = v0 + v * Math.Min(LineSpaceSizeGU * ApplicationInterface.ScaleMod, length - l);
                        l += LineSpaceSizeGU * ApplicationInterface.ScaleMod;
                        v0 = v1;
                    }

                    i++;
                }
            }
        }
    }

    
}
