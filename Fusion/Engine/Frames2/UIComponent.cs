using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.SpritesD2D;

namespace Fusion.Engine.Frames2
{
    public abstract class UIComponent
    {
        protected bool BoundingBoxChanged;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public RectangleF BoundingBox { get; private set; }

        private Matrix _globalTransform = Matrix.Identity;
        private UIContainer _parent;
        public UIContainer Parent
        {
            get => _parent;
            internal set
            {
                _parent = value;
                RecalculateGlobalTransform();
            }
        }

        protected void RecalculateGlobalTransform()
        {
            _globalTransform = Transform;
            foreach (var ancestor in Ancestors())
            {
                _globalTransform = ancestor.Transform * _globalTransform;
            }


        }

        public bool Visible { get; set; }
        public Matrix Transform { get; set; }
        public object Tag { get; set; }
        public string Name { get; }

        protected UIComponent(float x, float y, float width, float height)
        {
            Name = GenerateName(GetType());
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public IEnumerable<UIContainer> Ancestors()
        {
            var current = Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        // TODO: Anchors

        public IList<IUIController> Controllers { get; } = new List<IUIController>();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteLayerD2D layer);

        private static readonly Dictionary<Type, int> GeneratedCountOfType = new Dictionary<Type, int>();
        private static string GenerateName(Type type)
        {
            if (GeneratedCountOfType.TryGetValue(type, out var value))
            {
                GeneratedCountOfType[type] = value + 1;
            }
            else
            {
                GeneratedCountOfType[type] = 1;
            }

            return $"{type.Name}_{value}";
        }
    }
}
