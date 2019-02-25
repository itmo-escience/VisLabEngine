using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using SharpDX.Direct3D9;

namespace Fusion.Engine.Graphics.GIS
{
    public class TextPointsGisLayer : PointsGisLayer
    {

        public SpriteFont Font { set; get; }
        public SpriteLayer TextSpriteLayer { set; get; }

        private List<String> PointTexts = new List<string>();                
        public Color TextColor;

        public List<Gis.GeoPoint> PointsCPUList = new List<Gis.GeoPoint>();

        public TextPointsGisLayer(Game engine, int maxPointsCount, SpriteFont font , bool isDynamic = false,
            bool useSecondBuffer = false) : base(engine, maxPointsCount, isDynamic = false, useSecondBuffer = false)
        {
            Font = font;
            //Flags = (int)(PointFlags.DOTS_SCREENSPACE);
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {            
            double distMax = 8000;
            double distMin = 6390;
            PointsCountToDraw = PointsCPUList.Count;
            var clamped = DMathUtil.Clamp(GlobeCamera.Instance.CameraDistance, distMin, distMax);
            var t = (clamped - distMin) / (distMax - distMin);
            float mult = (float)DMathUtil.Lerp(0.7, 50.0, t);
            var tu = (GlobeCamera.Instance.CameraDistance - distMin) / (distMax - distMin);
            SizeMultiplier = mult * 1;
            DotsBuffer.SetData(dotsData);
            base.Draw(gameTime, constBuffer);
            var whiteTex = this._game.RenderSystem.WhiteTexture;
            for (int i = 0; i < PointsCountToDraw; i++ )
            {                
                var cartPos = GeoHelper.SphericalToCartesian(new DVector2(PointsCpu[i].Lon, PointsCpu[i].Lat),
                    GeoHelper.EarthRadius);
                DMatrix camMatrixInvert;
                DMatrix.Invert(ref GlobeCamera.Instance.ViewMatrix, out camMatrixInvert);
                var camVector = camMatrixInvert.Forward;
                if (DVector3.Dot(cartPos.Normalized(), camVector.Normalized()) < -0.13)
                {
                    var text = PointTexts[i];
                    var screenPos = GlobeCamera.Instance.CartesianToScreen(cartPos);
                    var textRect = Font.MeasureStringF(text);
                    float fontDelta = (float)((textRect.Height + 16) * Math.Min(1, t/tu));
                    TextSpriteLayer.DrawSprite(whiteTex, screenPos.X, screenPos.Y - fontDelta, textRect.Width, textRect.Height, 0, Color.Black);
                    Font.DrawString(TextSpriteLayer, text, screenPos.X - textRect.Width / 2,
                        screenPos.Y - textRect.Height / 2 - fontDelta, TextColor, useBaseLine: false);
                }
            }
        }

        private Dictionary<int, Gis.GeoPoint> pointsById = new Dictionary<int, Gis.GeoPoint>();



        public void AddPoint(int id, Vector3 point, Color color, string text)
        {
            var lonLat = GeoHelper.CartesianToSpherical(new DVector3(point));
            Gis.GeoPoint gisPoint = new Gis.GeoPoint()
            {
                Color = color,
                Lon = lonLat.X,
                Lat = lonLat.Y,
                Tex0 = new Vector4(0, 0, 1, 0),
            };

            PointsCPUList.Add(gisPoint);            
            PointTexts.Add(text);
            pointsById[id] = gisPoint;
            UpdatePointsBuffer();
        }

        public void UpdatePoint(int id, Vector3 point, Color color, string text)
        {
            var lonLat = GeoHelper.CartesianToSpherical(new DVector3(point));
            Gis.GeoPoint v = pointsById[id];
            int ind = PointsCPUList.IndexOf(v);
            if (ind < 0)
            {
                Log.Error("WTF?");
                return;
            }
            v.Color = color;
            v.Lon = lonLat.X;
            v.Lat = lonLat.Y;
            pointsById[id] = v;
            UpdatePointsBuffer();

            PointsCpu[ind] = v;
            PointsCPUList[ind] = v;
            PointTexts[ind] = text;
        }

        public void RemovePoint(int id)
        {
            var point = pointsById[id];
            var index = PointsCPUList.IndexOf(point);

            PointsCPUList.RemoveAt(index);
            PointTexts.RemoveAt(index);

            pointsById.Remove(id);
            UpdatePointsBuffer();
        }

        void UpdatePointsBuffer()
        {
            Array.Copy(PointsCPUList.ToArray(), PointsCpu, PointsCPUList.Count);
            PointsCountToDraw = PointsCPUList.Count;
            base.UpdatePointsBuffer();
        }
    }
}
