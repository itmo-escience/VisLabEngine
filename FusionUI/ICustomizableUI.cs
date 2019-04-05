using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Managing;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FusionUI
{
	public interface ICustomizableUI
	{
        UIManager GetUIManager();
		Assembly ProjectAssembly { get; }
		List<Type> CustomUIComponentTypes { get; }
	}
}
