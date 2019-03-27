using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfEditorTest.Commands
{
	class CommandGroup : IEditorCommand
	{
		private readonly List<IEditorCommand> _nestedCommands;
	    private bool _isNew = true;

	    public bool IsDirty { get; private set; } = false;
	    public bool IsEmpty => _nestedCommands.Count == 0;

		public CommandGroup(params IEditorCommand[] commands) : this(commands.AsEnumerable()) { }

	    public CommandGroup(IEnumerable<IEditorCommand> commands)
	    {
	        _nestedCommands = new List<IEditorCommand>(commands);
	        IsDirty = _nestedCommands.Any(nc => nc.IsDirty);
	    }

        /// <summary>
        /// Append command to a non-executed command group.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if command was executed beforehand.</exception>
        /// <param name="command"></param>
	    public void Append(IEditorCommand command)
	    {
            if(!_isNew)
                throw new InvalidOperationException("This CommandGroup was previously executed.");

            _nestedCommands.Add(command);
	        IsDirty |= command.IsDirty;
	    }

		public void Do()
		{
		    _isNew = false;
			foreach (var command in _nestedCommands)
			{
				command.Do();
			}
		}

		public void Undo()
		{
		    _nestedCommands.Reverse();
			foreach (var command in _nestedCommands)
			{
				command.Undo();
			}

            //reverse order again
		    _nestedCommands.Reverse();
		}
	}
}
