using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Containers;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.SpritesD2D;
using System.Text.RegularExpressions;
using Fusion.Engine.Frames2.Events;

namespace Fusion.Engine.Frames2.Managing
{
    internal class RootSlot : Slot
    {
        public override float X => 0;
        public override float Y => 0;
        public override float Angle => 0;
        public override float Width { get; internal set; }
        public override float Height { get; internal set; }
        public override float AvailableWidth => Width;
        public override float AvailableHeight => Height;

        internal override Matrix3x2 Transform => Matrix3x2.Identity;
        public override bool Clip => false;
        public override bool Visible => true;
        public override IUIContainer<Slot> Parent => null;

        public override SolidBrushD2D DebugBrush => new SolidBrushD2D(Color4.White);
        public override TextFormatD2D DebugTextFormat => new TextFormatD2D("Calibri", 10);
        public override void DebugDraw(SpriteLayerD2D layer) {}

        internal RootSlot(float width, float height, IUIContainer<Slot> rootContainer)
        {
            Width = width;
            Height = height;
            Component = rootContainer;
            rootContainer.Placement = this;
        }

        // This object is immutable
        //public event PropertyChangedEventHandler PropertyChanged;
    }

    public class UIManager
    {
        public UIEventProcessor UIEventProcessor { get; }

        private readonly RootSlot _rootSlot;
        public FreePlacement Root { get; }

        public bool DebugEnabled { get; set; }

        private readonly Dictionary<Slot, Matrix3x2> _localTransforms = new Dictionary<Slot, Matrix3x2>();
        private readonly Dictionary<Slot, Matrix3x2> _globalTransforms = new Dictionary<Slot, Matrix3x2>();
        private readonly Dictionary<Slot, bool> _dirtyTransforms = new Dictionary<Slot, bool>();

        public UIManager(RenderSystem rs)
        {
            Root = new FreePlacement();
            Root.Name = "Root";

            _rootSlot = new RootSlot(rs.DisplayBounds.Width, rs.DisplayBounds.Height, Root);
            var t = _rootSlot.Transform;
            _localTransforms[_rootSlot] = t;
            _globalTransforms[_rootSlot] = Matrix3x2.Identity;
            _dirtyTransforms[_rootSlot] = false;

            UIEventProcessor = new UIEventProcessor(this, Root);

            rs.DisplayBoundsChanged += (s, e) =>
            {
                _rootSlot.Width = rs.DisplayBounds.Width;
                _rootSlot.Height = rs.DisplayBounds.Height;
            };
        }

        public void Update(GameTime gameTime)
        {
            UIEventProcessor.Update(gameTime);
            RecalculateAllTransforms();

            foreach (var c in UIHelper.DFSTraverse(Root))
            {
                c.Update(gameTime);
                var slot = c.Placement;

                if (SlotTransformChanged(slot))
                {
                    InvalidateTransformsDown(slot);
                }
            }

            RecalculateAllTransforms();
        }

        private bool SlotTransformChanged(Slot slot)
        {
            return !_localTransforms.TryGetValue(slot, out var storedTransform) || storedTransform != slot.Transform;
        }

        public Matrix3x2 GlobalTransform(Slot slot)
        {
            if (!_dirtyTransforms.TryGetValue(slot, out var isDirty))
            {
                isDirty = true;
            }

            if (isDirty)
            {
                RecalculateTransformsUp(slot);
            }

            return _globalTransforms[slot];
        }

        private void InvalidateTransformsDown(Slot slot)
        {
            foreach (var component in UIHelper.DFSTraverse(slot.Component))
            {
                _dirtyTransforms[component.Placement] = true;
            }
        }

        private void RecalculateTransformsUp(Slot slot)
        {
            if (slot == null) return;

            if(!_dirtyTransforms.GetOrDefault(slot, true)) return;

            if (slot == _rootSlot)
            {
                _localTransforms[slot] = slot.Transform;
                _globalTransforms[slot] = _localTransforms[slot];
                _dirtyTransforms[slot] = false;

                return;
            }

            RecalculateTransformsUp(slot.Parent.Placement);

            var local = slot.Transform;
            _localTransforms[slot] = local;
            _globalTransforms[slot] = _globalTransforms[slot.Parent.Placement] * local;
            _dirtyTransforms[slot] = false;
        }

        private void RecalculateAllTransforms()
        {
            foreach (var component in UIHelper.BFSTraverse(Root))
            {
                var slot = component.Placement;
                if (slot == _rootSlot)
                {
                    _localTransforms[_rootSlot] = _rootSlot.Transform;
                    _globalTransforms[_rootSlot] = _localTransforms[_rootSlot];
                    _dirtyTransforms[_rootSlot] = false;
                    continue;
                }

                var parent = slot.Parent.Placement;
#if DEBUG
                // at this point transforms for holder must be already calculated
                Debug.Assert(!_dirtyTransforms.GetOrDefault(parent, true));
                Debug.Assert(_globalTransforms.ContainsKey(parent));
#endif

                _localTransforms[slot] = slot.Transform;
                _globalTransforms[slot] = _globalTransforms[parent] * _localTransforms[slot];
                _dirtyTransforms[slot] = false;
            }
        }

        /// <summary>
        /// Considers only slot as input, doesn't consider parent clipping
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        internal bool IsInsideSlotInternal(Slot slot, Vector2 globalPoint)
        {
            var invertTransform = GlobalTransform(slot);
            invertTransform.Invert();
            var localPoint = Matrix3x2.TransformPoint(invertTransform, globalPoint);
            return ((localPoint.X >= 0) && (localPoint.Y >= 0) && (localPoint.X < slot.Width) && (localPoint.Y < slot.Height));
        }

        /// <summary>
        /// Takes into account ancestors clipping
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsPointInsideSlot(Slot slot, Vector2 point)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteLayerD2D layer)
        {
            layer.Clear();

            layer.Draw(TransformCommand.Identity);
            DrawRecursive(layer, Root);

            layer.Draw(TransformCommand.Identity);
        }

        private void DrawRecursive(SpriteLayerD2D layer, UIComponent component)
        {
            var globalTransform = _globalTransforms[component.Placement];

            if (!component.Placement.Visible)
            {
                if (DebugEnabled)
                {
                    DebugDraw(layer, component.Placement, globalTransform);
                }

                return;
            }

            layer.Draw(new TransformCommand(globalTransform));

            if (component.Placement.Clip)
            {
                layer.Draw(new StartClippingAlongRectangle(component.Placement.BoundingBox, AntialiasModeD2D.Aliased));
            }

            component.Draw(layer);

            if (component is IUIContainer<Slot> container)
            {
                foreach (var child in container.Slots)
                {
                    DrawRecursive(layer, child.Component);
                }
            }

            if (component.Placement.Clip)
            {
                layer.Draw(new EndClippingAlongRectangle());
            }

            if (DebugEnabled)
            {
                DebugDraw(layer, component.Placement, globalTransform);
            }
        }

        private void DebugDraw(SpriteLayerD2D layer, Slot slot, Matrix3x2 transform)
        {
            layer.Draw(new TransformCommand(transform));

            slot.DebugDraw(layer);

            var b = slot.BoundingBox;
            layer.Draw(new Rect(b.X, b.Y, b.Width, b.Height, slot.DebugBrush));

            var debugText = $"{slot.Component.Name} X:{b.X:0.00} Y:{b.Y:0.00} W:{b.Width:0.00} H:{b.Height:0.00}";
            var dtl = new TextLayoutD2D(debugText, slot.DebugTextFormat, float.MaxValue, float.MaxValue);
            layer.Draw(new Text(debugText, new RectangleF(b.X, b.Y - dtl.Height, dtl.Width + 1, dtl.Height), slot.DebugTextFormat, slot.DebugBrush));
        }

        public static bool IsComponentNameValid(string name, UIComponent root, UIComponent ignoredComponent = null)
        {
            foreach (var component in UIHelper.BFSTraverse(root))
            {
                if (component == root) continue;
                if ((component.Name == name) && (component != ignoredComponent)) return false;
            }
            return true;
        }

        public static void MakeComponentNameValid(UIComponent component, UIComponent root, UIComponent ignoredComponent = null)
        {
            string name = component.Name;

            if (IsComponentNameValid(name, root, ignoredComponent)) return;

            Regex nameEnd = new Regex(@"_\d+$");

            string nameBase;
            int index;
            if (!nameEnd.IsMatch(name))
            {
                nameBase = name;
                index = 1;
            }
            else
            {
                int underscoreIndex = name.LastIndexOf('_');
                nameBase = name.Remove(underscoreIndex, name.Length - underscoreIndex);
                index = int.Parse(name.Substring(underscoreIndex + 1));
            }

            name = nameBase + '_' + index.ToString();
            while (!IsComponentNameValid(name, root, ignoredComponent))
            {
                index++;
                name = nameBase + '_' + index.ToString();
            }

            component.Name = name;
        }
    }
}
