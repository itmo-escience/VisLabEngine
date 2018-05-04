using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class Table : ScalableFrame {

        public List<List<Cell>> Cells;

        const float defaultCellWidth = 5;
        const float defaultCellHeight = 4;

        public float defaultOffsetX = 2;
        public float defaultOffsetY = 0.5f;


        public float CellWidth = defaultCellWidth;
        public float CellHeight = defaultCellHeight;

        public int rows;
        public int columns;

        public int CellBorder = 0;
        public Color CellBorderColor = UIConfig.BorderColor;

        public Action<Vector2> OnResize;

        public Table(FrameProcessor ui, float x, float y, Color backColor, int i, int j, float cellWidth = defaultCellWidth, float cellHeight = defaultCellHeight) : base(ui, x, y, cellWidth * i, cellHeight * j, "", backColor)
        {            
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            InitTable(i, j);            
        }

        public void InitTable(int i, int j)
        {
            foreach (var child in this.Children)
            {
                child.Clear(this);                
            }
            this.Children.Clear();
            Cells = new List<List<Cell>>();
            columns = i;
            rows = j;
            for (int k = 0; k < i; k++)
            {
                if (Cells.Count <= k)
                {
                    Cells.Add(new List<Cell>());
                }
                for (int k2 = 0; k2 < j; k2++)
                {
                    var cell = new Cell(ui, k * CellWidth, k2 * CellHeight, CellWidth, CellHeight, "", Color.Zero, this)
                    {
                        IndexRow = k,
                        IndexColumn = k2,
                        Border = CellBorder,
                        BorderColor = CellBorderColor,
                    };
                    Cells[k].Add(cell);
                    this.Add(cell);
                }
            }
        }

        public void Set(string[,] values)
        {
            InitTable(values.GetLength(0), values.GetLength(1));
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    SetValueCell(values[i,j] ?? "", i, j);
                }
            }
        }

        //public void AddColumn()
        //{
        //    columns++;
        //    Cells.Add(new List<Cell>());
        //    var k = columns;
        //    for (int k2 = 0; k2 < rows; k2++)
        //    {
        //        var cell = new Cell(ui, k * CellWidth, k2 * CellHeight, CellWidth, CellHeight, "", Color.Zero, this)
        //        {
        //            IndexRow = k,
        //            IndexColumn = k2
        //        };
        //        Cells[k].Add(cell);
        //        this.Add(cell);
        //    }
        //}

        //public void AddRow()
        //{
            
        //}

        public void SetValueCell(string value, int i, int j)
        {
            while (i > Cells.Count - 1)
            {
                Cells.Add(new List<Cell>());
            }
            var sizeText = Cells[i][j].FontHolder[ApplicationInterface.uiScale].MeasureString(value);            
            Cells[i][j].Text = value;

            if (Cells[i][j].Width < sizeText.Width + defaultOffsetX * 2 * ApplicationInterface.ScaleMod &&
                value != "")
            {
                ResizeTable(i, j, (sizeText.Width - Cells[i][j].Width) / ApplicationInterface.ScaleMod + defaultOffsetX * 2, 0);
            }
            if (Cells[i][j].Height < sizeText.Height + defaultOffsetY * 2 * ApplicationInterface.ScaleMod)
            {
                ResizeTable(i, j, 0, (sizeText.Height - Cells[i][j].Height)/ApplicationInterface.gridUnitDefault + defaultOffsetY * 2);
            }
        }

        public void ResizeTable(int indexX, int indexY, float diffX, float diffY)
        {
            if (diffX != 0)
            {
                for (var i = 0; i < Cells[indexX].Count; i++)
                {
                    Cells[indexX][i].UnitWidth += diffX;
                }
                for (var i = indexX + 1; i < Cells.Count; i++)
                {
                    for (var j = 0; j < Cells[i].Count; j++)
                    {
                        Cells[i][j].UnitX += diffX;
                    }
                }
                
            }

            if (diffY != 0)
            {
                for (var i = 0; i < Cells.Count; i++)
                {
                    Cells[i][indexY].UnitHeight += diffY;
                }
                for (var i = 0; i < Cells.Count; i++)
                {
                    for (var j = indexY + 1; j < Cells[i].Count; j++)
                    {
                        Cells[i][j].UnitY += diffY;
                    }
                }
            }
            var rowNumber = Cells.Count - 1;
            var columnNumber = Cells[Cells.Count - 1].Count-1;
            UnitWidth = Cells[rowNumber][columnNumber].UnitX + Cells[rowNumber][columnNumber].UnitWidth;
            UnitHeight = Cells[rowNumber][columnNumber].UnitY + Cells[rowNumber][columnNumber].UnitHeight;

            OnResize?.Invoke(new Vector2(diffX, diffY));
        }
    }

    public class Cell : ScalableFrame {

        public int IndexRow;
        public int IndexColumn;

        public Table Table;

        public float clickableFieldWidth = 0f;

        private Vector2 prevPositon = Vector2.Zero;

        public Cell(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor, Table t) : base(ui, x, y, w, h, text, backColor)
        {
            this.Table = t;
//            Border = 1;
            TextAlignment = Alignment.MiddleCenter;

            var rightSide = new ScalableFrame(ui, this.UnitWidth - clickableFieldWidth, clickableFieldWidth, 
                clickableFieldWidth, this.UnitHeight - clickableFieldWidth*2,
                "", Color.Zero)
            {
                Anchor = FrameAnchor.Right | FrameAnchor.Bottom | FrameAnchor.Top
            };
            rightSide.ActionDrag += (ControlActionArgs args, ref bool flag) => {
                if (prevPositon.Equals(Vector2.Zero))
                {
                    prevPositon = GlobalRectangle.Location;
                    prevPositon.X += GlobalRectangle.Width;
                }
                    
                var diff = (args.Position - prevPositon).X / ApplicationInterface.gridUnitDefault;
                if(this.UnitWidth > clickableFieldWidth+3 || diff > 0)
                    Table.ResizeTable(IndexRow, IndexColumn, diff, 0);
                prevPositon = args.Position;
            };
            rightSide.ActionLost += (ControlActionArgs args, ref bool flag) =>
            {
                prevPositon = Vector2.Zero;
            };

            var bottomSide = new ScalableFrame(ui, clickableFieldWidth, this.UnitHeight - clickableFieldWidth,
                this.UnitWidth - 2 * clickableFieldWidth, clickableFieldWidth, "", Color.Zero)
            {
                Anchor = FrameAnchor.Left | FrameAnchor.Right | FrameAnchor.Bottom
            };
            bottomSide.ActionDrag += (ControlActionArgs args, ref bool flag) => {
                if (prevPositon.Equals(Vector2.Zero))
                {
                    prevPositon = GlobalRectangle.Location;
                    prevPositon.Y += GlobalRectangle.Height;
                }
                var diff = (args.Position - prevPositon).Y / ApplicationInterface.gridUnitDefault;
                if (this.UnitHeight > clickableFieldWidth + 3 || diff > 0)
                    Table.ResizeTable(IndexRow, IndexColumn, 0, diff);
                prevPositon = args.Position;
            };
            bottomSide.ActionLost += (ControlActionArgs args, ref bool flag) =>
            {
                prevPositon = Vector2.Zero;
            };
            this.Add(rightSide);
            this.Add(bottomSide);
        }
    }
}
