using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Factories
{

    /// <summary>
    /// Frame containing single active item and some decorative stuff
    /// </summary>
    /// <typeparam name="T">Active item type</typeparam>
    public class UIContainer<T> : ScalableFrame where T : ScalableFrame
    {
        public UIContainer(FrameProcessor ui) : base(ui)
        {
        }

        public UIContainer(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        public virtual T Item { get; set; }
    }

    /// <summary>
    /// Frame containing two active items and some decorative stuff
    /// </summary>
    /// <typeparam name="T1">Active item 1 type</typeparam>
    /// <typeparam name="T2">Active item 2 type</typeparam>
    public class UIContainer<T1, T2> : ScalableFrame where T1 : ScalableFrame where T2 : ScalableFrame
    {
        public UIContainer(FrameProcessor ui) : base(ui)
        {
        }

        public UIContainer(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        public virtual T1 Item1 { get; set; }
        public virtual T2 Item2 { get; set; }
    }

    /// <summary>
    /// Frame containing three active items and some decorative stuff
    /// </summary>
    /// <typeparam name="T1">Active item 1 type</typeparam>
    /// <typeparam name="T2">Active item 2 type</typeparam>
    /// <typeparam name="T3">Active item 3 type</typeparam>
    public class UIContainer<T1, T2, T3> : ScalableFrame where T1 : ScalableFrame where T2 : ScalableFrame where T3 : ScalableFrame
    {
        public UIContainer(FrameProcessor ui) : base(ui)
        {
        }

        public UIContainer(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        public virtual T1 Item1 { get; set; }
        public virtual T2 Item2 { get; set; }
        public virtual T3 Item3 { get; set; }
    }

}
