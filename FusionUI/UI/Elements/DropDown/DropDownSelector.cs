using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace FusionUI.UI.Elements.DropDown
{
    public class DropDownSelector<TR> : ScalableFrame where TR : DropDownSelectorRow, new()
    {
        public List<string> Values;
        private string current;
        public string Current
        {
            get { return current; }
            set
            {
                if (rows.ContainsKey(value))
                {
                    if (!DisabledRows.ContainsKey(value))
                    {
                        current = value;
                    }
                    else
                    {
                        Log.Error("Trying to set Current to Disabled Value");
                    }
                }
                else
                {
                    Log.Error("Value not present in the list");
                }
            }
        }
        public Action<string> OnClicked;
		private Action<string> updateValue;
		public bool IsOpen = false;
        private Dictionary<string, TR> rows = new Dictionary<string, TR>();
        public Dictionary<string, bool> DisabledRows = new Dictionary<string, bool>();
        private bool isClick = false;

        public int Capacity = 4;

	    private EventHandler<MouseEventArgs> mouseScrollAction;
        public DropDownSelector(FrameProcessor ui, float x, float y, float w, float h, Color backColor, List<string> values,
            Action<string> selectAction, Color borderColor, bool drawButton = true, UIConfig.FontHolder? font = null) : base(ui, x, y, w, h, "", backColor)
        {
            FontHolder = font ?? UIConfig.FontBase;
            BorderColor = borderColor;
            this.Values = values;
            init();


			OnClicked = selectAction;
            current = values.Any() ? values[0] : "";            
            UpdateLabel();
            if (!drawButton) ArrowButton.Visible = false;
        }

        public ScalableFrame ArrowButton, SelectorHolder;
        public FullScreenFrame FullScreenHolder;
        public TR MainRow;

        private bool isSelectorSummoned = false;

        void init()
        {

            MainRow = new TR();
            MainRow.Initialize(0, 0, UnitWidth, UIConfig.UnitSelectorHeight, Values.Any() ? Values[0] : "", Color.Zero);
            MainRow.TextAlignment = Alignment.MiddleLeft;
            MainRow.UnitTextOffsetX = 2;
            MainRow.Name = $"mainRow";
            MainRow.FontHolder = FontHolder;
            this.Add(MainRow);

            ArrowButton = new ScalableFrame(ui, UnitWidth - UIConfig.UnitSelectorArrowButtonWidth, 0, UIConfig.UnitSelectorArrowButtonWidth, UIConfig.UnitSelectorHeight, "", Color.Zero)
            {
                Image = ui.Game.Content.Load<DiscTexture>(@"UI-new\fv-icons_open-list"),
                Name = $"arrow",
                ImageMode = FrameImageMode.Fitted,
            };
            
            MainRow.Add(ArrowButton);

            initList();            

            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (args.IsClick)
                {
                    if (!IsOpen && Values.Count > 0) OpenList();
                    else CloseList();
                }
                flag |= true;                
            };   
            
        }

        public void UpdateList(List<string> values)
        {
            this.Values = values;
            initList();
            if (values.Count > 0)
                current = values[0];
            else 
                current = "Nothing to select";
            UpdateLabel();
        }

        public void AddValue(string value)
        {
            if (!Values.Contains(value))
            {
                Values.Add(value);
                AddRow(Values.Count - 1);
                UpdateRows();
            }
        }

        private void UpdateRows()
        {
            int w = Width;
            //foreach (var r in rows.Values)
            //{
            //    w = Math.Max(w, SelectorHolder.Font.MeasureString(r.Text).Width + r.TextOffsetX * 2);
            //}            
            SelectorHolder.Width = w;

            foreach (var r in rows.Values)
            {
                r.Width = w;
            }
        }

        void initList()
        {
            if (SelectorHolder != null) {
                SelectorHolder.Clean();
                ApplicationInterface.Instance.rootFrame.Remove(SelectorHolder);
            }

			if(FullScreenHolder != null) {
				ApplicationInterface.Instance.rootFrame.Remove(FullScreenHolder);
			}
            FullScreenHolder = new FullScreenFrame(ui) {
                Visible			= false,
                ZOrder			= 1000,  
                SuppressActions = true,
            };
            ApplicationInterface.Instance.rootFrame.Add(FullScreenHolder);

            SelectorHolder = new ScalableFrame(ui, -BorderLeft / ScaleMultiplier, -BorderTop / ScaleMultiplier, UnitWidth + BorderLeft + BorderRight, 0, "", UIConfig.PopupColor)
            {
                Border = 1,
                BorderColor = BorderColor,
                Visible = true,
                Name = $"holder",
                Active = true,     
                ZOrder           = 1000,
                SuppressActions = true,
            };
            //((Frame) SelectorHolder).Ghost = false;

			rows.Clear();


			for (int i = 0; i < Values.Count; i++) {
                AddRow(i);
            }
            UpdateRows();
            //if (mouseScrollAction != null) Game.Mouse.Scroll -= mouseScrollAction;

            SelectorHolder.ActionDown += (ControlActionArgs args, ref bool flag) =>
            {
                flag = true;
            };

            SelectorHolder.ActionDrag += (ControlActionArgs args, ref bool flag) =>
            {
                float delta = -(int)(args.DY) / ScaleMultiplier;
                float min = float.MaxValue, max = float.MinValue;
                foreach (var row in rows.Values)
                {
                    min = Math.Min(min, row.UnitY - UIConfig.UnitSelectorRowOffset);
                    max = Math.Max(max, row.UnitY + row.UnitHeight - SelectorHolder.UnitHeight + UIConfig.UnitSelectorRowOffset);
                }
                delta = MathUtil.Clamp(delta, min, max);
                foreach (var row in rows.Values)
                {
                    row.UnitY -= delta;
                }
                flag = true;
            };

            FullScreenHolder.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (IsOpen && !SelectorHolder.Selected) CloseList();
                isSelectorSummoned = false;
                flag = true;
            };

            mouseScrollAction = (s, e) =>
            {
                var rootFrame = ApplicationInterface.Instance.rootFrame;
                if (MainFrame.IsChildOf(rootFrame, MainFrame.GetHoveredFrame(rootFrame, Game.Mouse.Position), SelectorHolder))
                {
                    float delta = -(int)(e.Wheel) / ScaleMultiplier / 10;
                    float min = float.MaxValue, max = float.MinValue;
                    foreach (var row in rows.Values)
                    {
                        min = Math.Min(min, row.UnitY - UIConfig.UnitSelectorRowOffset);
                        max = Math.Max(max, row.UnitY + row.UnitHeight - SelectorHolder.UnitHeight + UIConfig.UnitSelectorRowOffset);
                    }
                    delta = MathUtil.Clamp(delta, min, max);
                    foreach (var row in rows.Values)
                    {
                        row.UnitY -= delta;
                    }
                }
            };



            SelectorHolder.MouseWheel += mouseScrollAction;

            FullScreenHolder.Add(SelectorHolder);
            updateValue = s => {
                UpdateLabel();
            };
            UpdateScroller();
        }

        void AddRow(int index)
        {
            TR valueRow = new TR();
            valueRow.Initialize(
                0, UIConfig.UnitSelectorRowHeight*(index) + UIConfig.UnitSelectorRowOffset, UnitWidth,
                UIConfig.UnitSelectorRowHeight, Values[index], Color.Zero);
            valueRow.BorderColor = BorderColor;
            valueRow.Name = $"row_{index}";
            valueRow.Visible = false;
            valueRow.Tooltip = Values[index];
            valueRow.FontHolder = FontHolder;

            valueRow.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                current = valueRow.Value;
                OnClicked(current);
	            updateValue(current);
                flag = true;
            };
            rows.Add(Values[index], valueRow);
            SelectorHolder.Add(valueRow);  
        }

        public void UpdateLabel()
        {
            MainRow.Value = current;
            CloseList();
        }

        public void UpdateScroller()
        {
            float maxHeight = Capacity*UIConfig.UnitSelectorRowHeight+ 2 * UIConfig.UnitSelectorRowOffset;            
            float possibleHeight = rows.Count * UIConfig.UnitSelectorRowHeight + 2 * UIConfig.UnitSelectorRowOffset;
            float realHeight = Math.Min(maxHeight, possibleHeight);
            SelectorHolder.UnitHeight = realHeight;
        }

        public void OpenList()
        {
            if (IsOpen) return;
            IsOpen = true;
            UpdateScroller();
            FullScreenHolder.ZOrder = this.ZOrder + 1000;
            SelectorHolder.ZOrder = FullScreenHolder.ZOrder + 1000;
            
            foreach (var name in rows.Keys)
            {
                var row = rows[name];
                rows[name].Visible = true;
                if (name == current) {
                    row.ForeColor =   UIConfig.ActiveColor;
                    row.Ghost = false;
                }
                else if (DisabledRows.ContainsKey(name) && DisabledRows[name]) {
                    row.ForeColor = UIConfig.InactiveColor;
                    row.Ghost = true;
                }
                else {
                    row.ForeColor = UIConfig.ActiveTextColor;
                    row.Ghost = false;
                }
            }

            SelectorHolder.Selected = true;
                     
            SelectorHolder.X = this.GlobalRectangle.X - ApplicationInterface.Instance.rootFrame.GlobalRectangle.X + XOffset;
            SelectorHolder.Y = this.GlobalRectangle.Y - ApplicationInterface.Instance.rootFrame.GlobalRectangle.Y + YOffset;
            FullScreenHolder.Visible = true;
        }

        public int XOffset = 0, YOffset = 0;

        void CloseList()
        {
            if (!IsOpen) return;
            IsOpen = false;

            foreach (var name in rows.Keys)
            {
                rows[name].Visible = false;
            }

            FullScreenHolder.Visible = false;
        }
    }
}
