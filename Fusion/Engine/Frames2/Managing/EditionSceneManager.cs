﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Frames2.Managing
{
	public static class EditionSceneManager
	{
		public static bool ReadSceneFromFile(string filePath, out UIContainer scene)
		{
			UIComponent readScene = null;
			try
			{
				Fusion.Core.Utils.UIComponentSerializer.Read(filePath, out readScene);
			}
			catch (Exception)
			{
				throw;
			}

			if (readScene is UIContainer)
			{
				scene = readScene as UIContainer;
				return true;
			}
			else
			{
				scene = null;
				return false;
			}
		}

		public static bool GetComponentByName(UIContainer sourceContainer, string name, out UIComponent component )
		{
			component = UIHelper.BFSTraverse(sourceContainer).Where(child => child.Name == name).FirstOrDefault();

			if (component != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}