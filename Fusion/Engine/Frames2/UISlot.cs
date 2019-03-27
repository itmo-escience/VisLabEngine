﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames2.Managing;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class Slot : PropertyChangedHelper, IUIDebugDrawable, INotifyPropertyChanged
    {
        private float _x;
        public virtual float X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        private float _y;
        public virtual float Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        private float _angle;
        public virtual float Angle
        {
            get => _angle;
            set => SetAndNotify(ref _angle, value);
        }

        internal virtual Matrix3x2 Transform => Matrix3x2.Transformation(1.0f, 1.0f, Angle, X, Y);

        /* Current width and height */
        private float _width;
        public virtual float Width
        {
            get => _width;
            internal set => SetAndNotify(ref _width, value);
        }

        private float _height;
        public virtual float Height
        {
            get => _height;
            internal set => SetAndNotify(ref _height, value);
        }

        public RectangleF BoundingBox => Visible ? new RectangleF(X, Y, Width, Height) : new RectangleF(0, 0, 0, 0);

        /* Container specifies available dimensions for component */
        public virtual float AvailableWidth => MathUtil.Clamp(Parent.Placement.Width - X, 0, float.MaxValue);   //TODO: Use angle
        public virtual float AvailableHeight => MathUtil.Clamp(Parent.Placement.Height - Y, 0, float.MaxValue);

        private bool _clip = true;
        public virtual bool Clip
        {
            get => _clip;
            set => SetAndNotify(ref _clip, value);
        }

        private bool _visible = true;
        public virtual bool Visible
        {
            get => _visible;
            set => SetAndNotify(ref _visible, value);
        }

        public abstract IUIContainer<Slot> Parent { get; }
        
        private UIComponent _component;
        public virtual UIComponent Component
        {
            get => _component;
            protected set => SetAndNotify(ref _component, value);
        }

        public abstract SolidBrushD2D DebugBrush { get; }
        public abstract TextFormatD2D DebugTextFormat { get; }
        public abstract void DebugDraw(SpriteLayerD2D layer);

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public abstract class AttachableSlot : Slot
    {
        public void Attach(UIComponent newComponent)
        {
            var s = newComponent.Placement;

            if (s != null)
            {
                if (s is AttachableSlot sa)
                {
                    sa.Detach();
                }
                else
                {
                    Log.Error("Attempt to attach component from unmodifiable");
                    return;
                }
            }

            UIComponent old = null;
            if (Component != null)
            {
                old = Component;
                Component.Placement = null;
            }

            newComponent.Placement = this;
            Component = newComponent;
            ComponentAttached?.Invoke(this, new SlotAttachmentChangedEventArgs(old, newComponent));
        }

        public event EventHandler<SlotAttachmentChangedEventArgs> ComponentAttached;

        public void Detach() => Attach(null);
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