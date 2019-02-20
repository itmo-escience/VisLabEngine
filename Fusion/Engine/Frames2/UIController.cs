using System;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.Engine.Frames2
{
    public abstract class UIController
    {
        public State CurrentState { get; protected set; } = State.Default;

        public IEnumerable<State> States
        {
            get
            {
                yield return State.Default;
                foreach (var state in NonDefaultStates)
                {
                    yield return state;
                }
            }
        }

        protected abstract IEnumerable<State> NonDefaultStates { get; }

        public abstract IReadOnlyList<Slot> Slots { get; }

        public void ChangeState(State newState)
        {
            if(newState == CurrentState) return;
            if(!States.Contains(newState)) return;

            foreach (var fragment in Slots)
            {
                var component = fragment.Component;
                var type = component.GetType();                
                foreach (var propertyValue in fragment.Properties)
                {
                    var info = type.GetProperty(propertyValue.Name);
                    if(info == null) continue;

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
        }

        public class Slot
        {
            public string Name { get; }
            public UIComponent Component { get; private set; }
            public List<PropertyValue> Properties { get; } = new List<PropertyValue>();

            public Slot(string name)
            {
                Name = name;
            }

            public void Attach(UIComponent component)
            {
                var old = Component;
                Component = component;
                ComponentAttached?.Invoke(this, 
                    new ComponentAttachedEventArgs(old, component)
                );
            }

            public event EventHandler<ComponentAttachedEventArgs> ComponentAttached;

            public class ComponentAttachedEventArgs : EventArgs
            {
                public UIComponent Old { get; }
                public UIComponent New { get; }

                public ComponentAttachedEventArgs(UIComponent oldComponent, UIComponent newComponent)
                {
                    Old = oldComponent;
                    New = newComponent;
                }
            }
        }

        public class PropertyValue
        {
            public string Name { get; }
            public object Default { get; }

            private readonly Dictionary<State, object> _storedValues = new Dictionary<State, object>();

            public PropertyValue(string name, object defaultValue)
            {
                Name = name;
                Default = defaultValue;
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
        }

        public struct State : IEquatable<State>
        {
            public readonly string Name;
            public State(string name)
            {
                Name = name;
            }

            public static State Default = new State("default");

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
        }
    }
}
