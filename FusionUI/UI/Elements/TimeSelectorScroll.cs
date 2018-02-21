using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class TimeSelectorScroll : ScalableFrame
    {
        private List<string> values = new List<string>();

        public List<string> Values
        {
            set { SetRows(value);}
        }
        private Dictionary<string, ScalableFrame> rows = new Dictionary<string, ScalableFrame>();

        private ScalableFrame blueHolder;

        public TimeSelectorScroll (FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base (ui, x, y, w, h, text, backColor) {
            blueHolder = new ScalableFrame(ui, 0, (Capacity - 1)/2 * UIConfig.UnitSelectorRowHeight, this.UnitWidth, UIConfig.UnitSelectorRowHeight, "", UIConfig.ActiveColor);
            Add(blueHolder);
            InitActions();
        }

        public Action<string> UpdateFunction;

        public float Capacity
        {
            get { return capacity; }
            set
            {
                capacity = value;
                if (blueHolder != null) blueHolder.UnitY = (Capacity - 1)/2*UIConfig.UnitSelectorRowHeight;
            }
        }

        private float capacity;

        private Vector2? lastPosition;
        private float delta;

        private int offset;

        public string Current
        {
            get
            {
                if (Offset >= 0 && values.Count > 0) return values[Offset].Split('_')[0];
                else return "none";
            }
            set
            {
                int i = values.IndexOf(value);
                if (i < 0)
                {
                    //Log.Error("Value not present in the list");
                    i = 0;
                }
                Offset = i;
                updateOffsets();
                updateByOffset();
            }            
        }

        public int Offset
        {
            get { return offset; }
            set
            {
                if (values.Count == 0) offset = -1;
                else
                {
                    if (value < 0) value += values.Count;
                    offset = value%values.Count;
                }
            }
        }
        public void InitActions()
        {
            ActionDrag += (ControlActionArgs args, ref bool flag) => {
                if (lastPosition != null)
                {
                    var dy = (args.Y - lastPosition.Value.Y)/ScaleMultiplier;
                    delta += dy;
                    foreach (var row in rows) {
                        row.Value.UnitY += dy;
                    }
                    if (delta > UIConfig.UnitSelectorRowHeight / 2) {                        
                        Offset -= 1;                                                
                        UpdateFunction?.Invoke (Current);
                        delta -= UIConfig.UnitSelectorRowHeight;
                        updateByOffset();
                        updateOffsets();
                    }
                    if (delta < -UIConfig.UnitSelectorRowHeight / 2) {                        
                        Offset += 1;                          
                        UpdateFunction?.Invoke (Current);
                        delta += UIConfig.UnitSelectorRowHeight;
                        updateByOffset();
                        updateOffsets();
                    }
                    updateCycling ();
                    Log.Message(delta.ToString());
                }
                lastPosition = args.Position;
                flag = true;
            };
            ActionLost += (ControlActionArgs args, ref bool flag) => {
                delta = 0;
                updateOffsets ();
                updateByOffset();               
            };

            ActionDown += (ControlActionArgs args, ref bool flag) => {
                lastPosition = args.Position;
                flag = true;
            };
        }

        private void updateCycling()
        {            
            for (int i = 0; i < values.Count; i++) {
                var yearRow = rows[values[i]];
                if (yearRow.UnitY + yearRow.UnitHeight < 0) {
                    yearRow.UnitY += UIConfig.UnitSelectorRowHeight * rows.Count;
                }
                if (yearRow.UnitY + yearRow.UnitHeight > UIConfig.UnitSelectorRowHeight * rows.Count) {
                    yearRow.UnitY -= UIConfig.UnitSelectorRowHeight * rows.Count;
                }
            }
        }

        private void updateOffsets()
        {
            for (int i = 0; i < values.Count; i++) {
                var yearRow = rows[values[i]];
                yearRow.UnitY = (Capacity * 0.5f + i - Offset - 0.5f) * UIConfig.UnitSelectorRowHeight + delta;
            }
            updateCycling();
        }

        private void updateByOffset()
        {
            for (int i = 0; i < values.Count; i++)
            {
                var row = rows[values[i]];
                if (Offset == i)
                {
                    row.ForeColor = Color.White;
                }
                else
                {
                    row.ForeColor = UIConfig.InactiveTextColor;
                }
            }
        }

        public void SetRows(List<string> values)
        {
            string cur = Current;
            foreach (var row in rows.Values)
            {
                row.Clean();
                row.Clear(this);  
                this.Remove(row);
            }
            this.values.Clear();
            rows.Clear();
            if (values.Count == 0) return;            
            for (int i = 0; this.values.Count < (Capacity / 2 * 2 + 1); i++)
            {
                this.values.AddRange(values.Select(s => i == 0? s : s + $"_{i}"));
            }

            for (int i = 0; i < this.values.Count; i++)
            {
                AddRow(this.values[i], i);
            }

            Current = cur;
                        
            updateOffsets();
            updateByOffset();            
        }

        private void AddRow(string value, int index)
        {
            ScalableFrame valueRow = new ScalableFrame(ui, 0, 0, UnitWidth,
                    UIConfig.UnitSelectorRowHeight, value.Split('_')[0], Color.Zero)
            {
                UnitTextOffsetX = 2,
                TextAlignment = Alignment.MiddleCenter,
                Name = $"row_{index}",                
            };
            rows.Add (value, valueRow);
            Add (valueRow);
        }
            
    }
}
