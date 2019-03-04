using System;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Frames2
{
    public interface ISlot
    {
        Vector2 Position { get; }

        UIComponent Component { get; }

        void Attach(UIComponent component);

        void Detach();

        event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
    }

    public abstract class UISlotStab : ISlot
    {
        public abstract Vector2 Position { get; }

        public UIComponent Component { get; protected set; }

        public virtual void Attach(UIComponent component)
        {
            var old = Component;

            Component = component;

            ComponentAttached?.Invoke(this,
                new SlotAttachmentChangedEventArgs(old, component)
            );
        }

        public virtual void Detach()
        {
            Attach(null);
        }

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;
    }

    public class FreePlacementSlot : UISlotStab
    {
        public float X { get; set; }
        public float Y { get; set; }

        public override Vector2 Position => new Vector2(X, Y);
    }

    public class SlotAttachmentChangedEventArgs : EventArgs
    {
        public UIComponent Old { get; }
        public UIComponent New { get; }

        public SlotAttachmentChangedEventArgs(UIComponent oldComponent, UIComponent newComponent)
        {
            Old = oldComponent;
            New = newComponent;
        }
    }
}