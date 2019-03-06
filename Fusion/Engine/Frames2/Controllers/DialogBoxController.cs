using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Fusion.Engine.Frames2.Controllers
{
    public class DialogBoxController : UIController
    {
        public Slot TitleBackground { get; }
        public Slot Background { get; }
        public Slot Title { get; }
        public Slot ExitButton { get; }
        public Slot Content { get; }

        public DialogBoxController(float x, float y, float width, float height) : base(x, y, width, height)
        {
            Background = new Slot("Background");
            TitleBackground = new Slot("TitleBackground");
            Title = new Slot("Title");
            ExitButton = new Slot("ExitButton");
            Content = new Slot("Content");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(TitleBackground);
            SlotsInternal.Add(Title);
            SlotsInternal.Add(ExitButton);
            SlotsInternal.Add(Content);

            ExitButton.ComponentAttached += (s, e) =>
            {
                if (e.New is ButtonController newButton)
                {
                    newButton.ButtonClick += Close;
                }
                else
                {
                    Log.Warning("ExitButton isn't Button!");
                }

                if (e.Old is ButtonController oldButton)
                {
                    oldButton.ButtonClick -= Close;
                }
            };
        }

        public void Close(object sender, ButtonController.ButtonClickEventArgs args) => Close();

        public void Close() => Parent.Remove(this);
    }
}
