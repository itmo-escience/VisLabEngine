//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FloodVision3.UI.Plots;
//using Fusion.Core.Mathematics;
//using Fusion.Engine.Common;
//using Fusion.Engine.Frames;
//using Fusion.Engine.Graphics;
//using Fusion.Engine.Graphics.Graph;

//namespace FloodVision3.UI
//{
//    public class Plot : ScalableFrame
//    {        
//        public static DateTime MinDate = DateTime.Now;
//        public DateTime StartDate = DateTime.Now;

//        public bool IsSecond = false;
//        public List<float> stepsForY = new List<float>() { 10e-9f, 10e-7f, 10e-6f, 10e-5f, 10e-3f, 0.1f, 0.5f, 1, 5, 10, 25, 50, 100, 200, 500, 1000, 2000, 3000, 5000, 10000 };
//        public int thicknessLine = 3;

//        public string dateFormat = @"dd/MM HH:mm";

//        private Texture beamTexture;
//        //fonts 
//        private SpriteFont AxisTextFont;
//        // main 
//        double minX = 0;
//        double maxX = 0;
//        double minY = 0;
//        double maxY = 0;
//        private bool IsDrawing = true;
//        private double DistanceXMax = 0;
//        private double DistanceYMax = 0;
//        //public parameters not start from capitals. My eyes bleeding!
//        public int pixelAxisX = 0;
//        public int pixelAxisY = 0;
//        public bool UpdateStepX = false, UpdateStepY = false;
//        //public Dictionary<string, List<Vector2>> ValueList;

//        public float AddValueForMax = 0;
//        public double UserMinValue = 0;
//        public double UserMaxValue = 0;
//        public bool IsTime = false;

//        // for limit the field of point
//        public float limitMinPointPercent = 0;
//        public float limitMaxPointPercent = 1;

//        // for limit the count of points
//        public int RangePoint = 0;
//        public float maxDifferenceX = 0;
//        //        public float limitMaxPointPercent = 1;

//        // for drawing
//        //private Dictionary<int, int> pointForSmoothDrawing;
//        //private Dictionary<int, Int2> pointForDrawing;
//        public Dictionary<string, PlotData> PlotData;
//        private float time = 0;
//        // grid
//                public int CountAxisY = 10;
//        public int CountAxisX = 4;
//        public int minStepGridY = 30;
//        public int maxStepGridY = 60;

//        public int stepGridX = 0;
//        public int stepGridY = 0;

//        public int currentStepIndexX = 0;
//        public int currentStepIndexY = 0;

//        public bool IsAxisX = true;
//        public bool IsAxisY = true;
//        public bool IsAxisXTextDown = false;
//        public bool IsAxisSerifY = false;

//        public bool IsAxisXText = true;
//        public bool IsAxisYText = true;

//        public bool IsFilled = false;
//        public bool IsStacked = false;

//        public bool IsMainAxisX = false;
//        public bool IsMainAxisY = false;

//        public Color MainAxisColor = Color.Yellow;
//        public int   MainAxisThiсkness = 2;

//        public bool IsDrawNameLegend = false;

//        public bool IsDrawRunner = false;
//        public Vector2 positionRunner;

//        public Plot(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
//        {
//            init();
//        }

//        public void init()
//        {
//            PlotData = new Dictionary<string, PlotData>();
//            beamTexture = Game.Content.Load<DiscTexture>(@"ui\beam.tga");
//            //ValueList = new Dictionary<int, List<Vector2>>();
//            //pointForDrawing = new Dictionary<int, Int2>();
//            //pointForSmoothDrawing = new Dictionary<int, int>();
//            stepGridX = this.Width / CountAxisX;
//            stepGridY = this.Height / CountAxisY; ;// (int)translateToPixel(new Vector2(0, stepsForY[currentStep])).Y;
//            AxisTextFont = this.Game.Content.Load<SpriteFont>(@"fonts\opensansR14");
//            minX = 0;
//            minY = 0;
//            maxX = 0;
//            maxY = 0;
//            positionRunner = new Vector2(GlobalRectangle.X, 0);
//            //            updateMinMaxPlot();
//        }

//        public void addPoint(Vector2 newPoint, string plotId)
//        {
//            var pd = PlotData[plotId];
//            if (pd.ValueList == null) pd.ValueList = new List<Vector2>();
//            if (pd.pointsForDrawing == null) {
//                pd.pointsForDrawing = new Int2(0, 0);                            
//                minX = Math.Min(minX, newPoint.X);
//                minY = Math.Min(minY, newPoint.Y);
//                maxX = Math.Max(maxX, newPoint.X);
//                maxY = Math.Max(maxY, newPoint.Y);
//            }

//            pd.ValueList.Add(newPoint);
//            pd.pointsForDrawing = getRangePoint(pd.ValueList.Count);
//            updateMinMaxPlotPoint(newPoint);
//            IsDrawing = true;
//        }

//        public void RemovePlot(string plotId)
//        {
//            PlotData.Remove(plotId);
//            var pd = PlotData[plotId];            
//            updateMinMaxPlot();
//        }

//        public void addPlot(PlotData data, string plotId)
//        {
//            if (data.ValueList == null) return;
//            PlotData.Add(plotId, data);                        
//            data.pointsForDrawing = getRangePoint(data.ValueList.Count);
//            updateMinMaxPlot();
//            IsDrawing = true;
//        }

//        public void setPlot(PlotData data, string plotId)
//        {
//            if (data.ValueList == null) return;            
//            PlotData[plotId] = data;            
//            data.pointsForDrawing = getRangePoint(data.ValueList.Count);
//            updateMinMaxPlot();
//            IsDrawing = true;
//        }

////        public void addPlots(List<List<Vector2>> PlotList)
////        {
////            if (PlotList == null)
////                return;
////            foreach (var plot in PlotList)
////            {
////                addPlot(plot);
////            }
////            updateMinMaxPlot();
////            IsDrawing = true;
////        }

//        public Vector2 getMaxLastElement()
//        {
//            return PlotData.Count > 0 ? new Vector2(PlotData.Values.Max(e => e.ValueList.Last().X), (float)maxY) : Vector2.Zero;
//        }

//        private Int2 getRangePoint(int countPoints)
//        {
//            var range = new Int2((int)(countPoints * limitMinPointPercent), (int)(countPoints * limitMaxPointPercent));
//            if (RangePoint == 0 && maxDifferenceX == 0)
//            {
//                return range;
//            }
//            if (RangePoint > 0)
//            {
//                var startPosition = countPoints * limitMaxPointPercent - RangePoint;
//                if (startPosition < 0)
//                    startPosition = 0;
//                range.X = (int)startPosition;
//            }
//            //            if (maxDifferenceX < DistanceXMax)
//            //            {
//            //                minX = maxX - maxDifferenceX;
//            //                DistanceXMax = maxDifferenceX;
//            //            }

//            return range;
//        }

//        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
//        {
//            //stepGridX = this.Width / CountAxisX;
//            //            stepGridY = this.Height / CountAxisY;
//            if (IsDrawing)
//            {
//                if (time > 0.01f)
//                {
//                    foreach (var key in PlotData.Keys)
//                    {
//                        //while (pointForSmoothDrawing[key] < pointForDrawing[key].Y - 1)
//                        //    pointForSmoothDrawing[key]++;
//                        PlotData[key].pointForSmoothDrawing = Math.Max(PlotData[key].pointForSmoothDrawing, PlotData[key].pointsForDrawing.Y - 1);
//                    }
//                    time = 0;
//                }
//                time += gameTime.ElapsedSec;
//            }
//            else
//            {
//                foreach (var key in PlotData.Keys)
//                {
//                    PlotData[key].pointForSmoothDrawing = PlotData[key].pointsForDrawing.Y;
//                }
//            }

//#warning PASS CLIPRECTINDEX TO THESE FUNCTION TO ENABLE CLIPPING
//            updateMinMaxPlot();
//            updateStep();
//            UpdateCoorAxis();

//            drawAxis(sb);
//            drawGraphic(sb);
//            drawMainAxis(sb);
//            drawLegendName(sb);
//            drawRunner(sb);
//        }

//        private void drawRunner(SpriteLayer sb)
//        {
//            if(!IsDrawRunner || positionRunner.X < this.GlobalRectangle.Left || positionRunner.X > this.GlobalRectangle.Right)
//                return;
//            sb.DrawBeam(beamTexture, new Vector2(positionRunner.X, this.GlobalRectangle.Y), new Vector2(positionRunner.X, this.GlobalRectangle.Y + this.Height), Color.White, Color.White, 4);
//        }

//        private void drawLegendName(SpriteLayer sb)
//        {
//            if (!IsDrawNameLegend)
//                return;
//            List<LegendInfo> legendElement = new List<LegendInfo>();
//            foreach (var plot in PlotData)
//            {
//                var config = PlotData[plot.Key];
//                if (!config.visible)
//                    continue;
//                if (plot.Value.ValueList.Last().X < minX)
//                    continue;
//                legendElement.Add(new LegendInfo(config.legend, translateToPixel(plot.Value.ValueList.Last()), config.color));
//            }
//            var sortedPositionLegend = legendElement.OrderBy(e => e.pos.Y).ToList();
//            var i = 0;
//            while (i < sortedPositionLegend.Count)
//            {
//                var upLegend = sortedPositionLegend[i];
//                var nextLegend = i + 1 < sortedPositionLegend.Count ? sortedPositionLegend[i + 1] : null;
//                this.Font.DrawString(sb, upLegend.legend, upLegend.pos.X, upLegend.pos.Y, upLegend.color);
//                if (nextLegend != null && upLegend.pos.Y + this.Font.CapHeight > nextLegend.pos.Y && upLegend.pos.X.Equals(nextLegend.pos.X))
//                {
//                    nextLegend.pos.Y = upLegend.pos.Y + this.Font.CapHeight;
//                }
//                i++;
//            }
//        }

//        private class LegendInfo
//        {

//            public string legend;
//            public Vector2 pos;
//            public Color color;

//            public LegendInfo(string legend, Vector2 vector2, Color c)
//            {
//                this.legend = legend;
//                this.pos = vector2;
//                this.color = c;
//            }
//        }

//        private void calculateStep(int minStepGrid, int maxStepGrid, List<float> steps, double distance, int currentStepIn, out int step, out int currentStep, out int countAxis)
//        {
//            countAxis = 0;
//            step = 0;
//            var minusCircle = false;
//            var plusCircle = false;
//            currentStep = currentStepIn;
//            if (currentStep + 1 == steps.Count)
//                currentStep--;
//            while (steps.Count >= currentStep + 2)
//            {
//                if (distance == 0)
//                    break;
//                countAxis = (int)Math.Ceiling(distance / steps[currentStep]) + 1;
//                step = this.Height / countAxis;
//                if (step >= minStepGrid &&
//                    step <= maxStepGrid)
//                {
//                    break;
//                }
//                else if (step > maxStepGrid)
//                {
//                    if (plusCircle)
//                    {
//                        //currentStep--;
//                        break;
//                    }
//                    currentStep--;
//                    minusCircle = true;
//                    if (currentStep == -1)
//                    {
//                        steps.Insert(0, steps[0] / 2);
//                        currentStep = 0;
//                    }
//                }
//                else if (step < minStepGrid)
//                {
//                    if (minusCircle)
//                    {
//                        currentStep++;
//                        countAxis = (int)Math.Ceiling(distance / steps[currentStep]) + 1;
//                        step = this.Height / countAxis;
//                        break;
//                    }
//                    currentStep++;
//                    plusCircle = true;
//                }
//            }            
//        }

//        private void updateStep()
//        {
//            int step, currentStep, countAxis;
//            if (UpdateStepX)
//            {
//                calculateStep(minStepGridY, maxStepGridY, stepsForY, DistanceYMax, currentStepIndexY, out step, out currentStep,
//                    out countAxis);
//                if (stepsForY[currentStep] != 0)
//                {
//                    currentStepIndexY = currentStep;
//                    stepGridY = step > 0 ? step : minStepGridY;
//                    minY = minY > 0 ?((int)(minY/stepsForY[currentStepIndexY])) * stepsForY[currentStepIndexY]
//                        : Math.Floor(minY / stepsForY[currentStepIndexY]) * stepsForY[currentStepIndexY];
//                    maxY = minY + stepsForY[currentStepIndexY] *countAxis;
//                    DistanceYMax = maxY - minY;
//                    UpdateCoorAxis();
//                }
//            }
//            if (UpdateStepY)
//            {
//                calculateStep(minStepGridY, maxStepGridY, stepsForY, DistanceXMax, currentStepIndexX, out step, out currentStep, out countAxis);
//                if (stepsForY[currentStep] != 0)
//                {
//                    currentStepIndexX = currentStep;
//                    stepGridX = step > 0 ? step : minStepGridY;
//                    minX = minX > 0 ? ((int)(minX / stepsForY[currentStepIndexX])) * stepsForY[currentStepIndexX]
//                        : Math.Floor(minX / stepsForY[currentStepIndexX]) * stepsForY[currentStepIndexX];
//                    //                        minX - stepsForY[currentStep];
//                    maxX = minX + stepsForY[currentStepIndexX] * countAxis;
//                    DistanceXMax = maxX - minX;
//                    UpdateCoorAxis();
//                }
//            }
//            else
//            {
//                stepGridX = this.Width / CountAxisX;
//            }
//        }

//        private void drawGraphic(SpriteLayer sb)
//        {
//            //updateMinMaxPlot();
//            //            int plotId = 0;
//            foreach (var data in PlotData)
//            {
//                PlotData confPlotData = PlotData[data.Key];
//                var barChartColor = confPlotData.color;
//                if (!confPlotData.visible)
//                    continue;
//                for (int i = data.Value.pointsForDrawing.X; i < data.Value.pointForSmoothDrawing; i++)
//                {
//                    if (minX > data.Value.ValueList[i].X)
//                        continue;
//                    if (float.IsNaN(data.Value.ValueList[i].X) || float.IsNaN(data.Value.ValueList[i].Y))
//                        continue;
//                    if (float.IsNaN(data.Value.ValueList[i + 1].X) || float.IsNaN(data.Value.ValueList[i + 1].Y))
//                        continue;
//                    var firstPoint = translateToPixel(data.Value.ValueList[i]);
//                    var secondPoint = translateToPixel(data.Value.ValueList[i + 1]);

//                    if (confPlotData.isFilled)
//                    {
//                        var countPoint = secondPoint.X - firstPoint.X;
//                        for (var x = 0; x < countPoint; x++)
//                        {
//                            var xPointTop = new Vector2(firstPoint.X + x,
//                                (secondPoint.Y - firstPoint.Y) / (secondPoint.X - firstPoint.X) * x + firstPoint.Y);
//                            sb.DrawBeam(beamTexture, xPointTop, new Vector2(xPointTop.X, pixelAxisX),
//                                new Color(barChartColor.ToVector3(), 0.5f), new Color(barChartColor.ToVector3(), 0.5f), 1);
//                        }
//                        //                        sb.DrawBeam(beamTexture, firstPoint, new Vector2(firstPoint.X, pixelAxisX),
//                        //                           new Color(barChartColor.ToVector3(), 0.5f), new Color(barChartColor.ToVector3(), 0.5f), 1);

//                        //                        var yGraphic = 0f;
//                        //                        if (list.Value[i].Y > 0)
//                        //                        {
//                        //                            yGraphic = pixelAxisX + (firstPoint.Y - pixelAxisX) / 2;
//                        //                        }
//                        //                        else
//                        //                        {
//                        //                            yGraphic = pixelAxisX - (pixelAxisX - firstPoint.Y) / 2;
//                        //                        }
//                        //                        sb.DrawSprite(this.Game.RenderSystem.WhiteTexture, firstPoint.X + (secondPoint.X - firstPoint.X) / 2, yGraphic, secondPoint.X - firstPoint.X, Math.Abs(pixelAxisX - firstPoint.Y), 0, new Color(barChartColor.ToVector3(), 0.2f));
//                    }
//                    if (!confPlotData.isFilled)
//                        sb.DrawBeam(beamTexture, firstPoint, secondPoint, barChartColor, barChartColor, thicknessLine);

//                }
//            }
//        }


//        private void drawAxis(SpriteLayer sb)
//        {

//            // grid x > 0
//            var i = 0;
//            for (int y = pixelAxisX; y >= this.GlobalRectangle.Y; y -= stepGridY)
//            {
//                drawAxisY(sb, y, i);
//                i++;
//            }

//            // grid x < 0
//            i = 0;
//            for (int y = pixelAxisX; y <= this.GlobalRectangle.Y + this.Height; y += stepGridY)
//            {
//                drawAxisY(sb, y, i, true);
//                i++;
//            }
//            //if(!IsAxisYText)
//            //    AxisTextFont.DrawString(sb, maxY.ToString(), this.GlobalRectangle.X, this.GlobalRectangle.Y, ColorConstant.TextColor);
//            // grid Y > 0
//            i = 0;
//            for (int x = pixelAxisY; x <= this.GlobalRectangle.X + this.Width; x += stepGridX)
//            {
//                drawAxisX(sb, x, i);
//                i++;
//            }


//            // grid Y < 0
//            i = 0;
//            for (int x = pixelAxisY; x >= this.GlobalRectangle.X; x -= stepGridX)
//            {
//                drawAxisX(sb, x, i, true);
//                i++;
//            }
//        }
//        private void drawMainAxis(SpriteLayer sb)
//        {
//            // Axis x
//            if(IsMainAxisY)
//                sb.DrawBeam(beamTexture, new Vector2(this.GlobalRectangle.X, pixelAxisX), new Vector2(this.GlobalRectangle.X + this.Width, pixelAxisX), MainAxisColor, MainAxisColor, MainAxisThiсkness);
//            //this.Font.DrawString(sb, "", this.GlobalRectangle.X - this.Font.MeasureString("x").Width, pixelAxisX, ColorConstant.TextColor);
//            // Axis y
//            if (IsMainAxisX)
//            {
//                sb.DrawBeam(beamTexture, new Vector2(pixelAxisY, this.GlobalRectangle.Y), new Vector2(pixelAxisY, this.GlobalRectangle.Y + this.Height), MainAxisColor, MainAxisColor, MainAxisThiсkness);
//            }
//            //this.Font.DrawString(sb, "", pixelAxisY, this.GlobalRectangle.Y - this.Font.MeasureString("y").Height / 2, ColorConstant.TextColor);
//        }


//        private void drawAxisX(SpriteLayer sb, int x, int indexArray, bool minus=false)
//        {
//            var whiteTex = this.Game.RenderSystem.WhiteTexture;
//            var stringValue = 0D;
//            if (IsTime)
//            {
//                stringValue = DistanceXMax / (this.Width) * (x - pixelAxisY);
//                stringValue += minX > 0 ? minX : maxX < 0 ? maxX : 0;
//                stringValue = Math.Round(stringValue, 2);
//            }
//            else
//            {
//                if (minX * maxX > 0)
//                {
//                    if (maxX < 0)
//                        stringValue = maxX - indexArray * stepsForY[currentStepIndexX];
//                    else
//                        stringValue = minX + indexArray * stepsForY[currentStepIndexX];
//                }
//                else
//                {
//                    stringValue = indexArray * stepsForY[currentStepIndexX];
//                    stringValue *= minus ? -1 : 1;
//                }
//            }

//            if (IsAxisY)
//            {
//                sb.Draw(whiteTex, new Rectangle(x, this.GlobalRectangle.Y, 1, this.Height),
//                    new Color(Color.White.ToVector3(), 0.2f));
//            }

//            if (IsAxisSerifY)
//            {
//                sb.Draw(whiteTex, new Rectangle(x, pixelAxisX, 1, 5),
//                    new Color(Color.White.ToVector3(), 0.2f));
//            }
//            var str = stringValue.ToString("0.00");
//            if (IsTime)
//            {
//                if (!IsSecond)
//                    str = ConvertFromUnixTimestamp(stringValue + MinDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString(dateFormat);
//                else
//                    str = StartDate.Add(TimeSpan.FromSeconds(stringValue)).ToString(dateFormat);
//            }

//            var sizeText = AxisTextFont.MeasureString(str);
//            var xText = x - sizeText.Width / 2;
//            var yText = IsAxisXTextDown ? this.GlobalRectangle.Y + this.Height + AxisTextFont.CapHeight + 3: pixelAxisX + 4 * 3 + AxisTextFont.CapHeight;
//            if (IsAxisXText)
//                AxisTextFont.DrawString(sb, str, xText, yText, ColorConstant.TextColor);
//        }

//        private void drawAxisY(SpriteLayer sb, int y, int indexArray, bool minus=false)
//        {
//            var whiteTex = this.Game.RenderSystem.WhiteTexture;
//            var stringValue = 0D;
//            if (minY*maxY > 0)
//            {
//                if (maxY < 0)
//                    stringValue = maxY - indexArray*stepsForY[currentStepIndexY];
//                else
//                    stringValue = minY + indexArray*stepsForY[currentStepIndexY];
//            }
//            else
//            {
//                stringValue = indexArray * stepsForY[currentStepIndexY];
//                stringValue *= minus ? -1 : 1;
//            }
                



//            //stringValue += minY > 0 ? minY : maxY < 0 ? maxY : 0;
//            //stringValue = Math.Round(stringValue, 1);
//            if (IsAxisX)
//            {
//                sb.Draw(whiteTex, new Rectangle(this.GlobalRectangle.X, y, this.Width, 1),
//                    new Color(Color.White.ToVector3(), 0.2f));
//            }
//            var sizeText = AxisTextFont.MeasureString(stringValue.ToString());
//            var xText = this.GlobalRectangle.X - sizeText.Width - 4 * 2;
//            var yText = y + AxisTextFont.CapHeight / 2;
//            if (IsAxisYText)
//                AxisTextFont.DrawString(sb, stringValue.ToString(), xText, yText, ColorConstant.TextColor);
//        }


//        private void updateMinMaxPlot()
//        {
//            bool firstIter = true;
//            foreach (var plotData in PlotData)
//            {
//                if (plotData.Value.ValueList.Count == 0)
//                    continue;
//                if (!PlotData[plotData.Key].visible)
//                    continue;
//                if (firstIter)
//                {
//                    var startPoint = plotData.Value.pointsForDrawing.X;
//                    while (plotData.Value.ValueList.Count > startPoint && (float.IsNaN(plotData.Value.ValueList[startPoint].X)
//                           || float.IsNaN(plotData.Value.ValueList[startPoint].Y)))
//                    {
//                        startPoint++;
//                    }
//                    if(plotData.Value.ValueList.Count <= startPoint)
//                        continue;
//                    minX = plotData.Value.ValueList[startPoint].X;
//                    maxX = plotData.Value.ValueList[startPoint].X;

//                    minY = plotData.Value.ValueList[startPoint].Y;
//                    maxY = plotData.Value.ValueList[startPoint].Y;

//                    for (int i = plotData.Value.pointsForDrawing.X; i < plotData.Value.pointForSmoothDrawing + 1; i++)
//                    {
//                        updateMinMaxPlotPoint(plotData.Value.ValueList[i]);
//                    }
//                    firstIter = false;
//                }
//                else
//                {
//                    for (int i = plotData.Value.pointsForDrawing.X; i < plotData.Value.pointForSmoothDrawing + 1; i++)
//                    {
//                        updateMinMaxPlotPoint(plotData.Value.ValueList[i]);
//                    }
//                }
//            }
//            DistanceXMax = maxX - minX;
//            if (DistanceXMax > maxDifferenceX && maxDifferenceX > 0)
//            {
//                minX = maxX - maxDifferenceX;
//                DistanceXMax = maxDifferenceX;
//            }
//            DistanceYMax = maxY - minY;
//            UpdateCoorAxis();
//        }

//        private void updateMinMaxPlotPoint(Vector2 element)
//        {
//            if (float.IsNaN(element.X) || float.IsNaN(element.Y))
//                return;
//            var resultMaxX = element.X + AddValueForMax;
//            if (minX > element.X)
//                minX = element.X;
//            if (maxX < resultMaxX)
//                maxX = resultMaxX;

//            if (minY > element.Y)
//                minY = element.Y;
//            if (maxY < element.Y)
//                maxY = element.Y;

//            if (UserMaxValue != 0)
//                maxX = UserMaxValue;
//            if (UserMinValue != 0)
//                minX = UserMinValue;

//            DistanceXMax = maxX - minX;
//            DistanceYMax = maxY - minY;
//            UpdateCoorAxis();
//        }

//        private void UpdateCoorAxis()
//        {
//            if (DistanceYMax <= 0)
//            {
//                pixelAxisX = (this.GlobalRectangle.Y + this.GlobalRectangle.Height);
//            }
//            else
//            {
//                if (maxY >= 0 && minY <= 0)
//                {
//                    pixelAxisX = this.GlobalRectangle.Y + Height - (int)((this.Height) * (DistanceYMax - maxY) / DistanceYMax);
//                }
//                if (maxY > 0 && minY > 0)
//                {
//                    pixelAxisX = this.GlobalRectangle.Y + this.Height;
//                }
//                else if (maxY < 0 && minY < 0)
//                {
//                    pixelAxisX = this.GlobalRectangle.Y;
//                }
//            }


//            // Axis y
//            if (DistanceXMax <= 0)
//            {
//                pixelAxisY = (this.GlobalRectangle.X);
//            }
//            else
//            {
//                if (maxX > 0 && minX <= 0)
//                {
//                    pixelAxisY = this.GlobalRectangle.X + (int)((this.Width) * (DistanceXMax - maxX) / DistanceXMax);
//                }
//                if (maxX > 0 && minX > 0)
//                {
//                    pixelAxisY = this.GlobalRectangle.X;
//                }
//                else if (maxX < 0 && minX < 0)
//                {
//                    pixelAxisY = this.GlobalRectangle.X + this.GlobalRectangle.Width;
//                }
//            }
//            pixelAxisX += 1;

//        }

//        public Vector2 translateToPixel(Vector2 coor)
//        {
//            int pixelX = pixelAxisY;
//            if (Math.Abs(maxX - minX) > 1e-6)
//            {
//                pixelX = this.GlobalRectangle.X + (int)((this.Width) * ((coor.X - minX) / (maxX - minX)));
//            }
//            int pixelY = pixelAxisX;
//            if (Math.Abs(maxY - minY) > 1e-6)
//            {
//                pixelY = this.GlobalRectangle.Y + Height - (int)((this.Height) * ((coor.Y - minY) / (maxY - minY)));
//            }
//            return new Vector2(pixelX, pixelY);
//        }

//        public void updateLimit(float limitMin, float limitMax)
//        {
//            limitMinPointPercent = limitMin;
//            limitMaxPointPercent = limitMax;
//            foreach (var plotData in PlotData)
//            {
//                plotData.Value.pointsForDrawing = new Int2((int)((plotData.Value.ValueList.Count - 1) * limitMinPointPercent), (int)((plotData.Value.ValueList.Count - 1) * limitMaxPointPercent));
//                plotData.Value.pointForSmoothDrawing = plotData.Value.pointsForDrawing.X;
//            }
//            IsDrawing = false;
//        }

//        public int getCountPlot()
//        {
//            return PlotData.Count;
//        }

//        public double GetValueFromPixel(float valueX)
//        {
//            var stringValue = DistanceXMax / (this.Width) * (valueX - pixelAxisY);
//            stringValue += minX > 0 ? minX : maxX < 0 ? maxX : 0;
//            stringValue = Math.Round(stringValue, 0);
//            if (IsTime)
//            {
//                if (!IsSecond)
//                    stringValue = stringValue + MinDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
//            }
//            return stringValue;
//        }

//        public static DateTime ConvertFromUnixTimestamp(double timestamp)
//        {
//            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
//            return origin.AddMilliseconds(timestamp).ToLocalTime();
//        }

//        public void updateRunner(float currentTime)
//        {
//            positionRunner = translateToPixel(new Vector2(currentTime, 0));
//        }
//    }
//}
