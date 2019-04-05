//using Fusion.Engine.Frames2.Components;
//using Fusion.Engine.Frames2.Containers;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Fusion.Engine.Frames2.Controllers
//{
//    public class RadioButtonGroupController : UIController
//    {
//        public Slot ButtonsContainer { get; }
//        public Slot Background { get; }

//        public RadioButtonController CheckedRadioButton { get; private set; }

//        public RadioButtonGroupController() : this(0, 0, 0, 0) {}

//        public RadioButtonGroupController(float x, float y, float width, float height) : base(x, y, width, height)
//        {
//            ButtonsContainer = new Slot("ButtonsContainer");
//            Background = new Slot("Background");

//            SlotsInternal.Add(Background);
//            SlotsInternal.Add(ButtonsContainer);

//            ButtonsContainer.ComponentAttached += (s, e) =>
//            {
//                if (e.New is UIContainer newContainer)
//                {
//                    newContainer.Children.CollectionChanged += OnRadioButtonCollectionChange;
//                }
//                else
//                {
//                    Log.Warning("ButtonsContainer isn't UIContainer!");
//                }

//                if (e.Old is UIContainer oldContainer)
//                {
//                    oldContainer.Children.CollectionChanged -= OnRadioButtonCollectionChange;
//                }
//            };
//        }

//        public override void DefaultInit()
//        {
//            Width = 100;
//            Height = 100;
//            Background.Attach(new Border(0, 0, 100, 100));
//            ButtonsContainer.Attach(new FreePlacement(0, 0, 100, 100));
//        }

//        private void OnRadioButtonCollectionChange(object sender, NotifyCollectionChangedEventArgs args)
//        {
//            if (args.NewItems != null)
//            {
//                foreach (UIComponent component in args.NewItems)
//                {
//                    if (component is RadioButtonController radioButton)
//                    {
//                        radioButton.RadioButtonClick += ChangeCheckedRadioButtonTo;
//                    }
//                    else
//                    {
//                        Log.Warning("ButtonsContainer's element isn't RadioButtonController!");
//                    }
//                }
//            }
//            if (args.OldItems != null)
//            {
//                foreach (UIComponent component in args.OldItems)
//                {
//                    if (component is RadioButtonController radioButton)
//                    {
//                        radioButton.RadioButtonClick -= ChangeCheckedRadioButtonTo;
//                    }
//                }
//            }
//        }

//        private void ChangeCheckedRadioButtonTo(object sender,  RadioButtonController.RadioButtonClickEventArgs args)
//        {
//            var newButton = args.RadioButton;
//            if (newButton == CheckedRadioButton) return;

//            CheckedRadioButton?.ChangeState(State.Default);
//            CheckedRadioButton = newButton;
//        }
//    }
//}
