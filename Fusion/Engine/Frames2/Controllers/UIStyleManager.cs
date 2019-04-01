using System;
using System.Collections.Generic;
using Fusion.Engine.Frames2.Managing;

namespace Fusion.Engine.Frames2.Controllers
{
    public class UIStyleManager
    {
        public static UIStyleManager Instance = new UIStyleManager();

        public const string DefaultStyle = "Default";

        private readonly Dictionary<Type, Dictionary<string, IUIStyle>> _styles = new Dictionary<Type, Dictionary<string, IUIStyle>>();

        internal UIStyleManager() { }

        public IUIStyle GetStyle(Type type, string name = DefaultStyle)
        {
            return _styles[type][name];
        }

        public void AddStyle(IUIStyle style)
        {
            if(!_styles.ContainsKey(style.ControllerType))
                _styles[style.ControllerType] = new Dictionary<string, IUIStyle>();

            _styles[style.ControllerType][style.Name] = style;
        }
    }

    public interface IUIStyle
    {
        string Name { get; }
        Type ControllerType { get; }
        IEnumerable<PropertyValueStates> this[string slotName] { get; }
    }

    public class UISimpleStyle : IUIStyle
    {
        // map slotName => props
        private readonly Dictionary<string, List<PropertyValueStates>> _slots = new Dictionary<string, List<PropertyValueStates>>();

        public string Name { get; }
        public Type ControllerType { get; }

        public UISimpleStyle(Type controllerType, string name = UIStyleManager.DefaultStyle)
        {
            Name = name;
            ControllerType = controllerType;
        }

        public IEnumerable<PropertyValueStates> this[string slotName]
        {
            get => _slots.GetOrDefault(slotName, new List<PropertyValueStates>());
            set => _slots[slotName] = new List<PropertyValueStates>(value);
        }
    }

    public sealed class PropertyValueStates
    {
        public string Name { get; }
        public object Default { get; }

        private readonly Dictionary<State, object> _storedValues = new Dictionary<State, object>();

        public PropertyValueStates(string name, object defaultValue)
        {
            Name = name;
            Default = defaultValue;

            _storedValues[State.Default] = Default;
        }

        public object this[State s]
        {
            get
            {
                if (!_storedValues.TryGetValue(s, out var result))
                    result = Default;
                return result;
            }
            set => _storedValues[s] = value;
        }

        public override string ToString() => Name;
    }
}
