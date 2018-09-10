using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements
{
    public class ConnectorArrow : ScalableFrame
    {
        public ScalableFrame FromFrame, ToFrame;
        public float ArrowWidth, ArrowPointerSize;
        public int ForceInDirection = 0, ForceOutDirection = 0;
        public bool IsStraight = false;
        public ConnectorArrow(FrameProcessor ui, ScalableFrame from, ScalableFrame to, float width, float arrowSize) : base(ui)
        {
            FromFrame = from;
            ToFrame = to;
            ArrowWidth = width;
            ArrowPointerSize = arrowSize;
        }
       
        protected override void Update(GameTime gameTime)
        {                        
            base.Update(gameTime);
        }

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
            int inDirection = 0, outDirection = 0;
            Vector2 start = new Vector2(), end = new Vector2();
            switch (xDir * 3 + yDir)
            {
                case 0: outDirection = ForceOutDirection > 0 ? ForceOutDirection : 3;
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




            var whiteTex = Game.Content.Load<DiscTexture>("UI/beam");//this.Game.RenderSystem.WhiteTexture);
            if ((inDirection + outDirection) % 2 == 1 && !IsStraight)
            {
                Vector2 center = outDirection % 2 == 0 ? new Vector2(start.X, end.Y) : new Vector2(end.X, start.Y);
                var l1 = center - start;
                var l2 = end - center;
                l1.Normalize(); l2.Normalize();                                
                spriteLayer.DrawBeam(whiteTex, start + l1 * ArrowPointerSize / 2, center, BackColor, BackColor, ArrowWidth);
                spriteLayer.DrawBeam(whiteTex, center, end - l2 * ArrowPointerSize / 2, BackColor, BackColor, ArrowWidth);
            }
            else if (inDirection == outDirection && !IsStraight)
            {
                Vector2 center1 = new Vector2(), center2 = new Vector2();                    
                switch (outDirection)
                {
                    case 1: center1 = new Vector2((start.X + end.X)/2 + 4 * ArrowPointerSize, start.Y);
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
                var l1 = center1 - start;
                var l2 = end - center2;
                l1.Normalize(); l2.Normalize();                                
                spriteLayer.DrawBeam(whiteTex, start + l1 * ArrowPointerSize / 2, center1, BackColor, BackColor, ArrowWidth);
                spriteLayer.DrawBeam(whiteTex, center1, center2, BackColor, BackColor, ArrowWidth);
                spriteLayer.DrawBeam(whiteTex, center2, end - l2 * ArrowPointerSize / 2, BackColor, BackColor, ArrowWidth);
            }
            else if (!IsStraight)
            {
                
                Vector2 center1 = new Vector2(), center2 = new Vector2();
                switch (outDirection)
                {
                    case 1:
                        center1 = new Vector2((start.X + end.X)/2, start.Y);
                        center2 = new Vector2((start.X + end.X)/2, end.Y);
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

                var l1 = center1 - start;
                var l2 = end - center2;
                l1.Normalize();l2.Normalize();
                
                
                spriteLayer.DrawBeam(whiteTex, start + l1 * ArrowPointerSize / 2, center1, BackColor, BackColor, ArrowWidth);
                spriteLayer.DrawBeam(whiteTex, center1, center2, BackColor, BackColor, ArrowWidth);
                spriteLayer.DrawBeam(whiteTex, center2, end - l2 * ArrowPointerSize / 2, BackColor, BackColor, ArrowWidth);
            }
            else
            {
                var l = end - start;
                l.Normalize();
                
                spriteLayer.DrawBeam(whiteTex, start + l * ArrowPointerSize / 2, end - l * ArrowPointerSize / 2, BackColor, BackColor, ArrowWidth);
            }
                        

            var arrowTex = Game.Content.Load<DiscTexture>("UIArrow");
            if (IsStraight)
            {
                var forward = end - start;
                forward.Normalize();
                var right1 = Vector3.Cross(new Vector3(forward, 0), Vector3.BackwardRH);
                var right = new Vector2(right1.X, right1.Y);
                spriteLayer.DrawFreeUV(arrowTex,
                    end - right/2 * ArrowPointerSize,
                    end + right/2 * ArrowPointerSize,
                    end - right/2 * ArrowPointerSize - forward * ArrowPointerSize,
                    end + right/2 * ArrowPointerSize - forward * ArrowPointerSize, BackColor,
                    new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));                
            } else switch (inDirection)
            {                

                case 2: spriteLayer.DrawFreeUV(arrowTex, 
                    end + new Vector2(-ArrowPointerSize/2, 0),
                    end + new Vector2(ArrowPointerSize / 2, 0),
                    end + new Vector2(-ArrowPointerSize / 2, ArrowPointerSize),
                    end + new Vector2(ArrowPointerSize/2, ArrowPointerSize), BackColor,
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
    }
}
