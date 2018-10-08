using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;


namespace ZWpfLib
{
	public static class DeviceUtil
	{
		public static int AdapterCount
		{
			get
			{
				if (sAdapterCount == -1)
					using (var f = new Factory1())
						sAdapterCount = f.GetAdapterCount();
				return sAdapterCount;
			}
		}
		static int sAdapterCount = -1; // cache it, as the underlying code rely on Exception to find the value!!!

		public static IEnumerable<Adapter> GetAdapters(DisposeGroup dg)
		{
			using (var f = new Factory1()) {
				int n = AdapterCount;
				for (int i = 0; i < n; i++)
					yield return dg.Add(f.GetAdapter(i));
			}
		}

		public static Adapter GetBestAdapter(DisposeGroup dg)
		{
			SharpDX.Direct3D.FeatureLevel high = SharpDX.Direct3D.FeatureLevel.Level_9_1;
			Adapter ada = null;
			foreach (var item in GetAdapters(dg)) {
				var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(item);
				if (ada == null || level > high) {
					ada		= item;
					high	= level;
				}
			}
			return ada;
		}

		
		public static SharpDX.Direct3D11.Device Create11(DeviceCreationFlags cFlags = DeviceCreationFlags.None,	FeatureLevel minLevel = FeatureLevel.Level_9_1)
		{
			using (var dg = new DisposeGroup())
			{
				var ada = GetBestAdapter(dg);
				if (ada == null)
					return null;
				var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(ada);
				if (level < minLevel)
					return null;
				return new SharpDX.Direct3D11.Device(ada, cFlags, level);
			}
		}
	}
}
