using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using FusionUI.UI.Elements.TextFormatting;

namespace FusionUI.UI.Factories
{
    public class TextBlockFactory
    {
        public static UIContainer<RichTextBlock> TextBlockHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text,
            bool capSize = false)
        {
            RichTextBlock tb;
            return TextBlockHolder(ui, OffsetX, OffsetY, parent, font, minHeight, text, out tb, capSize);
        }

        public static UIContainer<RichTextBlock> TextBlockHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text, out RichTextBlock textBlock, bool capSize = false)
        {
            UIContainer<RichTextBlock> holder = new UIContainer<RichTextBlock>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, minHeight + OffsetY, "", Color.Zero)
            {
            };

            textBlock = new RichTextBlock(ui, OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX,
                minHeight, text, Color.Zero, font, 0, minHeight:minHeight, isShortText:capSize)
            {
            };
            holder.Add(textBlock);
            var refBlock = textBlock;
            holder.ActionUpdate += time =>
            {
                holder.UnitHeight = refBlock.UnitHeight + 2 * OffsetY;
                refBlock.UnitX = OffsetX;
                refBlock.UnitWidth = holder.UnitWidth - 2 * OffsetX;
            };
            holder.Item = textBlock;
            return holder;
        }

        public static UIContainer<RichTextBlock> TextBlockHolderWithOffset(FrameProcessor ui, float OffsetX, float OffsetYTop, float OffsetYBottom,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text, out RichTextBlock textBlock, bool capSize = false)
        {
            UIContainer<RichTextBlock> holder = new UIContainer<RichTextBlock>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, minHeight + OffsetYTop, "", Color.Zero)
            {
            };

            textBlock = new RichTextBlock(ui, OffsetX, OffsetYTop, holder.UnitWidth - 2 * OffsetX,
                minHeight, text, Color.Zero, font, 0, minHeight: minHeight, isShortText: capSize)
            {
            };
            holder.Add(textBlock);
            var refBlock = textBlock;
            holder.ActionUpdate += time =>
            {
                holder.UnitHeight = refBlock.UnitHeight + OffsetYTop + OffsetYBottom;
                refBlock.UnitX = OffsetX;
                refBlock.UnitWidth = holder.UnitWidth - 2 * OffsetX;
            };
            holder.Item = textBlock;
            return holder;
        }

        public static UIContainer<FormatTextBlock> FormatTextBlockHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text,
            bool capSize = false)
        {
            FormatTextBlock tb;
            return FormatTextBlockHolder(ui, OffsetX, OffsetY, parent, font, minHeight, text, out tb, capSize);
        }

        public static UIContainer<FormatTextBlock> FormatTextBlockHolder(FrameProcessor ui, float OffsetX, float OffsetY,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text, out FormatTextBlock textBlock, bool capSize = false)
        {
            UIContainer<FormatTextBlock> holder = new UIContainer<FormatTextBlock>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, minHeight + OffsetY, "", Color.Zero)
            {
            };

            textBlock = new FormatTextBlock(ui, OffsetX, OffsetY, holder.UnitWidth - 2 * OffsetX,
                minHeight, text, Color.Zero, font, 0, minHeight: minHeight, isShortText: capSize)
            {
            };
            holder.Add(textBlock);
            var refBlock = textBlock;
            holder.ActionUpdate += time =>
            {
                holder.UnitWidth = parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight;
                holder.UnitHeight = refBlock.UnitHeight + 2 * OffsetY;

                refBlock.UnitX = OffsetX;
                refBlock.UnitWidth = holder.UnitWidth - 2 * OffsetX;
            };
            holder.Item = textBlock;
            return holder;
        }

        public static UIContainer<FormatTextBlock> FormatTextBlockHolderWithOffset(FrameProcessor ui, float OffsetX, float OffsetYTop, float OffsetYBottom,
            ScalableFrame parent, UIConfig.FontHolder font, float minHeight, string text, out FormatTextBlock textBlock, bool capSize = false)
        {
            UIContainer<FormatTextBlock> holder = new UIContainer<FormatTextBlock>(ui, parent.UnitPaddingLeft, 0, parent.UnitWidth - parent.UnitPaddingLeft - parent.UnitPaddingRight, minHeight + OffsetYTop, "", Color.Zero)
            {
            };

            textBlock = new FormatTextBlock(ui, OffsetX, OffsetYTop, holder.UnitWidth - 2 * OffsetX,
                minHeight, text, Color.Zero, font, 0, minHeight: minHeight, isShortText: capSize)
            {
            };
            holder.Add(textBlock);
            var refBlock = textBlock;
            holder.ActionUpdate += time =>
            {
                holder.UnitHeight = refBlock.UnitHeight + OffsetYTop + OffsetYBottom;
                refBlock.UnitX = OffsetX;
                refBlock.UnitWidth = holder.UnitWidth - 2 * OffsetX;
            };
            holder.Item = textBlock;
            return holder;
        }
    }
}
