using System;
using System.Collections.Generic;
using System.ComponentModel;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames2.Events;

namespace Fusion.Engine.Frames2
{
    public interface UIComponent : IUIDrawable, INotifyPropertyChanged //, IUIInputAware
    {
        ISlot Placement { get; set; }

        UIEventsHolder Events { get; }

        float DesiredWidth { get; set; }
        float DesiredHeight { get; set; }

        bool IsInside(Vector2 point);

        object Tag { get; set; }
        string Name { get; set; }

        void DefaultInit();

        void Update(GameTime gameTime);
	}

    public static class UIComponentExtensions
    {
        public static IUIContainer Parent(this UIComponent component) => component.Placement?.Parent;
    }

    public static class UINameManager
    {
        #region Naming

        private static readonly Dictionary<Type, int> GeneratedCountOfType = new Dictionary<Type, int>();
        public static string GenerateName(Type type)
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

        #endregion Naming
    }
}
