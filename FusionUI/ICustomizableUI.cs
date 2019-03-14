using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Managing;

namespace FusionUI
{
	public interface ICustomizableUI
	{
		UIContainer<ISlot> GetUIRoot();
        UIManager GetUIManager();
    }
}
