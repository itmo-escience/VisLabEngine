using System.Collections.Generic;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics.SpritesD2D;
using Label = Fusion.Engine.Frames2.Components.Label;

namespace Fusion.Engine.Frames2.Controllers
{
    public class ButtonController : UIController
    {
        public UIComponent Foreground { get; set; }
        public UIComponent Background { get; set; }

        private static StateName None = new StateName("None");
        private static StateName Hovered = new StateName("Hovered");
        private static StateName Pressed = new StateName("Pressed");
        public override IReadOnlyList<StateName> States => new List<StateName> { None, Hovered, Pressed };


        public ButtonController(string text, float width, float height)
        {
            var f = new TextFormatD2D("Calibry", 20);

            Background = new Border(0, 0, width, height);
            Foreground = new Label(text, f, 0, 0, width, height);

            var h = new FreePlacement(0, 0, width, height);
            h.Add(Background);
            h.Add(Foreground);

            Holder = h;
        }

        protected override void AttachAction()
        {
            CurrentState = None;
            Holder.Click += (processor, args) => { Foreground.Visible = !Foreground.Visible; };
        }

        protected override void DetachAction() { }
    }
}
