using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.UndoRedo
{
	class CommandGroup : IEditorCommand
	{
		private readonly IEditorCommand[] nestedCommands;

		public CommandGroup(params IEditorCommand[] commands)
		{
			nestedCommands = commands;
		}

		public void Do()
		{
			foreach (var command in nestedCommands)
			{
				command.Do();
			}
		}

		public void Undo()
		{
			foreach (var command in nestedCommands.Reverse())
			{
				command.Undo();
			}
		}
	}
}
