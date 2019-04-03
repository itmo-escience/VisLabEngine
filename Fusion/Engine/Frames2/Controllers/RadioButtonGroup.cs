using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Components;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public class RadioButtonGroup
    {
        public string Name { get; set; }

        public RadioButtonController CheckedRadioButton { get; private set; }

        public RadioButtonGroup(string name)
        {
            Name = name;
        }

        internal void Add(RadioButtonController radioButton)
        {
            radioButton.RadioButtonClick += ChangeCheckedRadioButtonTo;
        }

        internal void Remove(RadioButtonController radioButton)
        {
            radioButton.RadioButtonClick -= ChangeCheckedRadioButtonTo;
        }

        private void ChangeCheckedRadioButtonTo(object sender,  RadioButtonController.RadioButtonClickEventArgs args)
        {
            var newButton = args.RadioButton;
            if (newButton == CheckedRadioButton) return;

            CheckedRadioButton?.ChangeState(ControllerState.Default);
            CheckedRadioButton = newButton;
        }
    }
}
