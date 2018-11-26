using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Input;

namespace FusionUI.UI.Elements.DropDown
{
    public class SelectorList<TR> : LayoutFrame where TR : DropDownSelectorRow, new()
    {

        public HashSet<string> ValueList = new HashSet<string>();
        private List<TR> rows = new List<TR>();
        public HashSet<string> SelectedList = new HashSet<string>();
        public SelectorList(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w, h, backColor)
        {

        }

        public Action<HashSet<string>> OnUpdateSelection;

        public void Init()
        {
            CreateList();
        }

        private int lastSelectIndex = -1;
        public void CreateList()
        {
            foreach (var row in rows)
            {
                Remove(row);
                row.Clear(this);
                row.Clean();
            }
            rows.Clear();

            int ind = 0;
            foreach (var value in ValueList)
            {
                var valueRow = new TR()
                {
                    Border = 1,
                };

                valueRow.Initialize(
                    0, UIConfig.UnitSelectorRowHeight * (ind) + UIConfig.UnitSelectorRowOffset, UnitWidth,
                    UIConfig.UnitSelectorRowHeight / 2, value, Color.Zero);

                Add(valueRow);
                rows.Add(valueRow);
                int index = ind;
                valueRow.ActionClick += (ControlActionArgs args, ref bool flag) =>
                {
                    if (args.IsClick)
                    {
                        if (SelectedList.Contains(valueRow.Value))
                        {
                            valueRow.ForeColor = UIConfig.ActiveTextColor;
                            valueRow.BackColor = Color.Zero;
                            SelectedList.Remove(valueRow.Value);
                            if ((Game.Instance.Keyboard.IsKeyDown(Keys.LeftShift) || Game.Instance.Keyboard.IsKeyDown(Keys.RightShift)) &&
                                lastSelectIndex != -1)
                            {
                                for (int i = lastSelectIndex; i != index; i = index > lastSelectIndex ? ++i : --i)
                                {
                                    var Row = rows[i];
                                    Row.ForeColor = UIConfig.ActiveTextColor;
                                    Row.BackColor = Color.Zero;
                                    SelectedList.Remove(Row.Value);
                                }
                            }
                        }
                        else
                        {
                            valueRow.ForeColor = UIConfig.ActiveTextColor;
                            valueRow.BackColor = UIConfig.ActiveColor;
                            SelectedList.Add(valueRow.Value);
                            if ((Game.Instance.Keyboard.IsKeyDown(Keys.LeftShift) || Game.Instance.Keyboard.IsKeyDown(Keys.RightShift)) &&
                                lastSelectIndex != -1)
                            {
                                for (int i = lastSelectIndex; i != index; i = index > lastSelectIndex ? ++i : --i)
                                {
                                    var Row = rows[i];
                                    Row.ForeColor = UIConfig.ActiveTextColor;
                                    Row.BackColor = UIConfig.ActiveColor;
                                    SelectedList.Add(Row.Value);
                                }
                            }
                        }
                        OnUpdateSelection?.Invoke(SelectedList);
                        if (lastSelectIndex != -1) rows[lastSelectIndex].Border = 1;
                        lastSelectIndex = index;
                        rows[lastSelectIndex].Border = 3;
                    } else if (args.IsAltClick)
                    {
                        foreach (var Row in rows)
                        {
                            Row.ForeColor = UIConfig.ActiveTextColor;
                            Row.BackColor = Color.Zero;
                            SelectedList.Remove(Row.Value);
                        }
                        valueRow.ForeColor = UIConfig.ActiveTextColor;
                        valueRow.BackColor = UIConfig.ActiveColor;
                        SelectedList.Add(valueRow.Value);
                        OnUpdateSelection?.Invoke(SelectedList);
                    }
                };
            ind++;
            }

        }
    }
}
