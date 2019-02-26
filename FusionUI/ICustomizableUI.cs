using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Managing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionUI
{
	public interface ICustomizableUI
	{
		UIContainer GetUIRoot();
        UIManager GetUIManager();
    }
}
