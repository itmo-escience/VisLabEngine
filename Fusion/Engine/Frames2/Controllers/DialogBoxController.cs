using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames.Abstract;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Input;
using MoveEventArgs = Fusion.Engine.Frames.Abstract.MoveEventArgs;

namespace Fusion.Engine.Frames2.Controllers
{
    public class DialogBoxController : UIController
    {
        public static State Dragged = new State("Dragged");
        protected override IEnumerable<State> NonDefaultStates => new List<State> { Dragged };

        public Slot TitleBackground { get; }
        public Slot ContentBackground { get; }
        public Slot Title { get; }
        public Slot ExitButton { get; }
        public Slot Content { get; }

        private Vector2 _dragLastPosition;

        private const float _titleHeight = 25;

        public DialogBoxController(float x, float y, float width, float height) : base(x, y, width, height)
        {
            ContentBackground = new Slot("ContentBackground");
            TitleBackground = new Slot("TitleBackground");
            Title = new Slot("Title");
            ExitButton = new Slot("ExitButton");
            Content = new Slot("Content");

            SlotsInternal.Add(ContentBackground);
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

            TitleBackground.ComponentAttached += (s, e) =>
            {
                e.New.MouseDown += OnTitleBackgroundMouseDown;
                e.New.MouseUp += OnTitleBackgroundMouseUp;
                if (e.Old != null)
                {
                    e.Old.MouseDown -= OnTitleBackgroundMouseDown;
                    e.Old.MouseUp -= OnTitleBackgroundMouseUp;
                }
            };

            MouseDrag += TryMove;

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Width" || e.PropertyName == "Height") UpdateSlotsComponentsSizes();
            };

            SetAttachEvents();
        }

        private void TryMove(UIComponent sender, DragEventArgs args) => TryMove(args.Position);

        private void TryMove(Vector2 mousePosition)
        {
            if (CurrentState == Dragged)
            {
                var delta = mousePosition - _dragLastPosition;
                X += delta.X;
                Y += delta.Y;
                _dragLastPosition = mousePosition;
            }
        }

        private void OnTitleBackgroundMouseDown(UIComponent sender, ClickEventArgs args)
        {
            if (args.Key == Keys.LeftButton)
            {
                _dragLastPosition = args.Position;
                ChangeState(Dragged);
            }
        }

        private void OnTitleBackgroundMouseUp(UIComponent sender, ClickEventArgs args)
        {
            if (args.Key == Keys.LeftButton) ChangeState(State.Default);
        }

        public event EventHandler OnClose;

        public void Close(object sender, ButtonController.ButtonClickEventArgs args) => Close();

        public void Close()
        {
            Parent.Remove(this);
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        private void SetAttachEvents()
        {
            TitleBackground.ComponentAttached += (s, e) =>
            {
                e.New.SetPositionAndSize(0, 0, Width, _titleHeight);
            };
            Title.ComponentAttached += (s, e) =>
            {
                e.New.SetPositionAndSize(0, 0, Width - _titleHeight, _titleHeight);
            };
            ExitButton.ComponentAttached += (s, e) =>
            {
                e.New.SetPositionAndSize(Width - _titleHeight, 0, _titleHeight, _titleHeight);
            };
            ContentBackground.ComponentAttached += (s, e) =>
            {
                e.New.SetPositionAndSize(0, _titleHeight, Width, Height - _titleHeight);
            };
            Content.ComponentAttached += (s, e) =>
            {
                e.New.SetPositionAndSize(0, _titleHeight, Width, Height - _titleHeight);
            };       
        }

        private void UpdateSlotsComponentsSizes()
        {
            TitleBackground.Component?.SetPositionAndSize(0, 0, Width, _titleHeight);
            Title.Component?.SetPositionAndSize(0, 0, Width - _titleHeight, _titleHeight);
            ExitButton.Component?.SetPositionAndSize(Width - _titleHeight, 0, _titleHeight, _titleHeight);
            ContentBackground.Component?.SetPositionAndSize(0, _titleHeight, Width, Height - _titleHeight);
            Content.Component?.SetPositionAndSize(0, _titleHeight, Width, Height - _titleHeight);
        }
    }
}
