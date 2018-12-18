using System;
using System.Collections.Generic;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Frames2
{
    public abstract class UIComponent
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public virtual RectangleF BoundingBox => new RectangleF(X, Y, Width, Height);
        public bool Visible { get; set; }
        public Matrix Transform { get; set; }
        public object Tag { get; set; }
        public string Name { get; }

        protected UIComponent()
        {
            Name = GenerateName(GetType());
        }

        public UIContainer Parent { get; internal set; }

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
        public abstract void Draw(SpriteLayer layer);

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
