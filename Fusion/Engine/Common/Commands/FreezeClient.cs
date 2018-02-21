﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("freezeClient", CommandAffinity.Server)]
	public class FreezeClient : NoRollbackCommand {

		static public int freezeTime = 0;


		[CommandLineParser.Required]
		public int FreezeTime { get; set; }


		public static void FreezeTest ()
		{
			if (freezeTime>0) {
				for (int i=0; i<freezeTime; i++) {
					Log.Message("CL Freeze: {0}/{1} seconds", i, freezeTime);
					Thread.Sleep(1000);
				}
				freezeTime = 0;
			}
		}

			
		public FreezeClient ( Invoker invoker ) : base(invoker) 
		{
		}


		public override void Execute ()
		{
			freezeTime	= 	FreezeTime;
		}
	}
}
