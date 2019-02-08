using System.Collections.Generic;
using Fusion.Engine.Common;

namespace Fusion.Engine.Frames2
{
    public abstract class UIController
    {
        public bool IsAttached { get; private set; }
        public UIContainer Host { get; private set; }

        protected UIContainer Holder { get; set; }

        public abstract IReadOnlyList<StateName> States { get; }

        private readonly Dictionary<StateName, IReadOnlyList<PropertyValue>> _modifications = new Dictionary<StateName, IReadOnlyList<PropertyValue>>();
        public IReadOnlyDictionary<StateName, IReadOnlyList<PropertyValue>> StateModifications => _modifications;
        public StateName CurrentState { get; protected set; }

        public void AttachTo(UIContainer host)
        {
            Host = host;
            AttachAction();
            IsAttached = true;
        }

        public void Detach()
        {
            DetachAction();
            Host = null;
            IsAttached = false;
        }

        protected virtual void AttachAction() { }
        protected virtual void DetachAction() { }

        public void Update(GameTime gameTime)
        {

        }

        public struct PropertyValue
        {
            public string Property;
            public object Value;
        }

        public class StateName
        {
            public string Name;
            internal StateName(string name)
            {
                Name = name;
            }
        }
    }
}
