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
    public class RadioButtonManager
    {
        private static int _lastId = 0;
        internal static int GenerateId() => _lastId++;

        private static List<RadioButtonGroup> _groups = new List<RadioButtonGroup>();

        public static RadioButtonGroup CreateNewGroup(string groupName)
        {
            var group = new RadioButtonGroup(groupName);
            _groups.Add(group);
            return group;
        }

        internal static RadioButtonGroup CreateNewGroup(string groupName, int id)
        {
            var group = new RadioButtonGroup(groupName, id);
            _groups.Add(group);
            return group;
        }

        internal static RadioButtonGroup GetGroupBy(string groupName, int id)
        {
            if (!_groups.Exists(g => g.Id == id))
            {
                return CreateNewGroup(groupName);
            };

            return _groups.Find(g => g.Id == id);
        }
    }

    public class RadioButtonGroup
    {
        public string Name { get; set; }
        public int Id { get; internal set; }

        public RadioButtonController CheckedRadioButton { get; private set; }

        internal RadioButtonGroup(string name)
        {
            Name = name;
            Id = RadioButtonManager.GenerateId();
        }

        internal RadioButtonGroup(string name, int id)
        {
            Name = name;
            Id = id;
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

            CheckedRadioButtonChange?.Invoke(this, new CheckedRadioButtonChangeEventArgs(newButton, CheckedRadioButton));
            CheckedRadioButton = newButton;
        }

        public event EventHandler<CheckedRadioButtonChangeEventArgs> CheckedRadioButtonChange;

        public class CheckedRadioButtonChangeEventArgs : EventArgs
        {
            public RadioButtonController OldRadioButton { get; }
            public RadioButtonController NewRadioButton { get; }

            public CheckedRadioButtonChangeEventArgs(RadioButtonController newRb, RadioButtonController oldRb)
            {
                NewRadioButton = newRb;
                OldRadioButton = oldRb;
            }
        }
    }
}
