using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class RadioButtonGroup : LayoutFrame
    {
        public Dictionary<string, RadioButtonElement> buttons = new Dictionary<string, RadioButtonElement>();
        private string value;

        public string Value
        {
            get { return value; }
            set
            {
                if (this.value == value) return;
                foreach (var cb in buttons)
                {
                    if (cb.Key == value)
                    {
                        cb.Value.IsChecked = true;
                        this.value = value;
                    }
                }
            }
        }

        public RadioButtonElement currentActive
        {
            get { return buttons[Value]; }
        }

        public Action<string> updateAction;

        public void Add(RadioButtonElement button)
        {
            if (Value == null) button.IsChecked = true;
            if (button.IsChecked)
            {
                foreach (var b in buttons.Values)
                {
                    b.IsChecked = false;
                }
                value = button.Value;
            }
            buttons.Add(button.Value, button);

            button.Check.Changed += b =>
            {
                if (b)
                {
                    foreach (var butt in buttons.Values)
                    {
                        if (butt != button) butt.IsChecked = false;
                    }
                    value = button.Value;
                    updateAction?.Invoke(Value);
                }
            };
            base.Add(button);
        }
        public RadioButtonGroup(FrameProcessor ui, float x, float y, float w, float h, Color backColor) : base(ui, x, y, w, h, backColor)
        {
            UpdateParent = true;
        }
    }
}
