﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public interface IControllerSlot : ISlot
    {
        ObservableCollection<PropertyValueStates> Properties { get; }
    }

    public abstract class UIController<T> : IUIContainer<T> where T : IControllerSlot
    {
        public State CurrentState { get; protected set; } = State.Default;

        public IEnumerable<State> States
        {
            get
            {
                yield return State.Default;
                yield return State.Hovered;
                yield return State.Disabled;

                foreach (var state in NonDefaultStates)
                {
                    yield return state;
                }
            }
        }

        protected virtual IEnumerable<State> NonDefaultStates => new List<State>();

        public abstract IEnumerable<T> Slots { get; }

        public void ChangeState(State newState)
        {
            if (!States.Contains(newState)) return;

            foreach (var slot in Slots)
            {
                var component = slot.Component;
                var type = component.GetType();
                foreach (var propertyValue in slot.Properties)
                {
                    var info = type.GetProperty(propertyValue.Name);
                    if (info == null) continue;

                    info.SetValue(
                        component,
                        Convert.ChangeType(
                            propertyValue[newState],
                            info.PropertyType),
                        null
                    );
                }
            }


            CurrentState = newState;

            Log.Verbose(CurrentState);
        }

        private bool _initialized = false;
        private void Initialize()
        {
            ChangeState(State.Default);
            _initialized = true;
        }

        public ISlot Placement { get; set; }
        public UIEventsHolder Events { get; } = new UIEventsHolder();
        public float DesiredWidth { get; set; }
        public float DesiredHeight { get; set; }
        public object Tag { get; set; }
        public string Name { get; set; }

        public void Update(GameTime gameTime)
        {
            if (!_initialized)
                Initialize();
        }

        public void Draw(SpriteLayerD2D layer) { }

        public int IndexOf(UIComponent child)
        {
            throw new NotImplementedException();
        }

        public bool Contains(UIComponent component) => Slots.Any(slot => slot.Component == component);

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public struct State : IEquatable<State>
    {
        public readonly string Name;
        public State(string name)
        {
            Name = name;
        }

        public static State Default = new State("Default");
        public static State Hovered = new State("Hovered");
        public static State Disabled = new State("Disabled");

        public static bool operator ==(State self, State other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(State self, State other)
        {
            return !self.Equals(other);
        }

        public bool Equals(State other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is State other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"State: {Name}";
        }

        public static implicit operator string(State s) => s.ToString();
    }

    public class PropertyValueStates
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
