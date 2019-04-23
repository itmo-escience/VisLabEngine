using Fusion.Engine.Frames2;
using Fusion.Engine.Frames2.Managing;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FusionUI
{
	public interface ICustomizableUI
	{
		Assembly ProjectAssembly { get; }
		List<Type> CustomUIComponentTypes { get; }
	}
}
