using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2
{
    public abstract class UIController : UIContainer
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

        protected UIController(float x, float y, float width, float height) : base(x, y, width, height, true) { }

        protected virtual IEnumerable<State> NonDefaultStates => new List<State>();

        protected readonly List<Slot> SlotsInternal = new List<Slot>();
        public IReadOnlyList<Slot> Slots => SlotsInternal;

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

            Log.Verbose(CurrentState);
        }

        internal void Attach(Slot slot, UIComponent component)
        {
            var idx = SlotsInternal.IndexOf(slot);
            base.AddAt(component, idx);

            ResizeAccordingly();
        }

        private void ResizeAccordingly()
        {
            var b = base.BoundingBox;
            foreach (var child in Children)
            {
                b = RectangleF.Union(b, child.BoundingBox);
            }

            // Do not take into account to the left and top
            Width = b.Width - MathUtil.Clamp(BoundingBox.X - b.X, float.NegativeInfinity, 0);
            Height = b.Height - MathUtil.Clamp(BoundingBox.Y - b.Y, float.NegativeInfinity, 0);
        }

        private bool _initialized = false;

        private void Initialize()
        {
            foreach (var slot in Slots)
            {
                if (slot.Component != null)
                {
                    Attach(slot, slot.Component);
                }
                slot.ComponentAttached += ComponentAttachedToSlot;
            }
            _initialized = true;
        }

        private void ComponentAttachedToSlot(object sender, Slot.ComponentAttachedEventArgs e)
        {
            var slot = (Slot) sender;

            Attach(slot, e.New);
        }

        public override void Update(GameTime gameTime)
        {
            if(!_initialized)
                Initialize();

            base.Update(gameTime);
        }

        #region Disable default children operations

        public override void Add(UIComponent child)
        {
            Log.Error("Direct addition is not supported");
        }

        public override void AddAt(UIComponent child, int index)
        {
            Log.Error("Direct addition is not supported");
        }

        public override bool Remove(UIComponent child)
        {
            Log.Error("Direct removal is not supported");
            return false;
        }

        #endregion

        public class Slot
        {
            public string Name { get; }
            public UIComponent Component { get; private set; }
            public ObservableCollection<PropertyValue> Properties { get; } = new ObservableCollection<PropertyValue>();

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

            public override string ToString() => Name;
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

        public struct State : IEquatable<State>
        {
            public readonly string Name;
            public State(string name)
            {
                Name = name;
            }

            public static State Default  = new State("Default");
            public static State Hovered  = new State("Hovered");
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
    }
}
