using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fusion.Engine.Frames2
{
    public interface IControllerType { }

    public abstract class UIController<T> : UIContainer where T : IControllerType
    {
        public State<T> CurrentState { get; protected set; } = State<T>.Default;

        public IEnumerable<State<T>> States
        {
            get
            {
                yield return State<T>.Default;
                yield return State<T>.Hovered;
                yield return State<T>.Disabled;

                foreach (var state in NonDefaultStates)
                {
                    yield return state;
                }
            }
        }

        protected virtual IEnumerable<State<T>> NonDefaultStates => new List<State<T>>();

        protected readonly List<IS> SlotsInternal = new List<PlacementSlot>();
        //public IReadOnlyList<Slot<T>> Slots => SlotsInternal;

        public void ChangeState(State<T> newState)
        {
            /*
            if (!States.Contains(newState)) return;

            foreach (var fragment in Slots)
            {
                var component = fragment.Component;
                var type = component.GetType();
                foreach (var propertyValue in fragment.Properties)
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

            */
            CurrentState = newState;

            Log.Verbose(CurrentState);
        }
        /*
        internal void Attach(ISlot<T> slot, UIComponent component)
        {
            var idx = SlotsInternal.IndexOf(slot);
            base.InsertAt(slot.Component, idx);

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

            ChangeState(State<T>.Default);
            _initialized = true;
        }

        private void ComponentAttachedToSlot(object sender, SlotAttachmentChangedEventArgs e)
        {
            var slot = (Slot<T>)sender;

            Attach(slot, e.New);
        }

        public override void Update(GameTime gameTime)
        {
            if (!_initialized)
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

        */
    }

    public interface IControllerSlot<T> : PlacementSlot where T : IControllerType
    {
        ObservableCollection<PropertyValueStates<T>> Properties { get; }
    }

    public struct State<T> : IEquatable<State<T>> where T : IControllerType
    {
        public readonly string Name;
        public State(string name)
        {
            Name = name;
        }

        public static State<T> Default = new State<T>("Default");
        public static State<T> Hovered = new State<T>("Hovered");
        public static State<T> Disabled = new State<T>("Disabled");

        public static bool operator ==(State<T> self, State<T> other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(State<T> self, State<T> other)
        {
            return !self.Equals(other);
        }

        public bool Equals(State<T> other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is State<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"State: {Name}";
        }

        public static implicit operator string(State<T> s) => s.ToString();
    }

    public class PropertyValueStates<T> where T : IControllerType
    {
        public string Name { get; }
        public object Default { get; }


        private readonly Dictionary<State<T>, object> _storedValues = new Dictionary<State<T>, object>();

        public PropertyValueStates(string name, object defaultValue)
        {
            Name = name;
            Default = defaultValue;

            _storedValues[State<T>.Default] = Default;
        }

        public object this[State<T> s]
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
