using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEditorTest.UndoRedo
{
	public interface IEditorCommand
	{
		void Do();

		void Undo();
	}
}
