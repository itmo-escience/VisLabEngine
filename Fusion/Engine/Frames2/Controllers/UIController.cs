using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2.Controllers
{
    public interface IControllerSlot : ISlot
    {
        string Name { get; }
    }

    public abstract class UIController<T> : IUIContainer<T> where T : IControllerSlot
    {
        public IUIStyle Style { get; protected set; }

        public ControllerState CurrentState { get; protected set; } = ControllerState.Default;

        public IEnumerable<ControllerState> States
        {
            get
            {
                yield return ControllerState.Default;
                yield return ControllerState.Hovered;
                yield return ControllerState.Disabled;

                foreach (var state in NonDefaultStates)
                {
                    yield return state;
                }
            }
        }

        protected virtual IEnumerable<ControllerState> NonDefaultStates => new List<ControllerState>();

        public abstract IEnumerable<T> Slots { get; }

        public void ChangeState(ControllerState newState)
        {
            if (!States.Contains(newState)) return;

            CurrentState = newState;
            Log.Debug($"{this} Changed state to {CurrentState}");

            foreach (var slot in Slots)
            {
                var component = slot.Component;
                var type = component.GetType();

                foreach (var propertyValue in Style[slot.Name])
                {
                    var info = type.GetProperty(propertyValue.Name);
                    if (info == null) continue;

                    info.SetValue(
                        component,
                        Convert.ChangeType(
                            propertyValue[CurrentState],
                            info.PropertyType),
                        null
                    );
                }
            }
        }

        private bool _initialized = false;
        private void Initialize()
        {
            ChangeState(ControllerState.Default);
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

    public struct ControllerState : IEquatable<ControllerState>
    {
        public readonly string Name;
        public ControllerState(string name)
        {
            Name = name;
        }

        public static ControllerState Default = new ControllerState("Default");
        public static ControllerState Hovered = new ControllerState("Hovered");
        public static ControllerState Disabled = new ControllerState("Disabled");

        public static bool operator ==(ControllerState self, ControllerState other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(ControllerState self, ControllerState other)
        {
            return !self.Equals(other);
        }

        public bool Equals(ControllerState other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ControllerState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"State: {Name}";
        }

        public static implicit operator string(ControllerState s) => s.ToString();
    }
}
