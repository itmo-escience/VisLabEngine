using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonGroupController : UIController
    {
        public Slot ButtonsContainer { get; }
        public Slot Background { get; }

        public RadioButtonController CheckedRadioButton { get; private set; }

        public RadioButtonGroupController(float x, float y, float width, float height) : base(x, y, width, height)
        {
            ButtonsContainer = new Slot("ButtonsContainer");
            Background = new Slot("Background");

            SlotsInternal.Add(Background);
            SlotsInternal.Add(ButtonsContainer);

            ButtonsContainer.ComponentAttached += (s0, e0) =>
            {
                if (!(ButtonsContainer.Component is UIContainer))
                {
                    Log.Warning("ButtonsContainer isn't UIContainer!");
                    return;
                }

                UIContainer container = ButtonsContainer.Component as UIContainer;
                container.Children.CollectionChanged += (s1, e1) =>
                {
                    if (e1.NewItems != null)
                    {
                        foreach (UIComponent component in e1.NewItems)
                        {
                            if (!(component is RadioButtonController))
                            {
                                Log.Warning("ButtonsContainer's element isn't RadioButtonController!");
                            }
                            else
                            {
                                RadioButtonController radioButton = component as RadioButtonController;
                                radioButton.RadioButtonClick += (s2, e2) => ChangeCheckedRadioButtonTo(e2.RadioButton);
                            }
                        }
                    }
                    if (e1.OldItems != null)
                    {
                        foreach (UIComponent component in e1.OldItems)
                        {
                            if (component is RadioButtonController)
                            {
                                RadioButtonController radioButton = component as RadioButtonController;
                                radioButton.RadioButtonClick -= (s2, e2) => ChangeCheckedRadioButtonTo(e2.RadioButton);
                            }
                        }
                    }
                };
            };
        }

        private void ChangeCheckedRadioButtonTo(RadioButtonController newButton)
        {
            if (newButton == CheckedRadioButton) return;

            CheckedRadioButton?.ChangeState(State.Default);
            CheckedRadioButton = newButton;
        }
    }
}
