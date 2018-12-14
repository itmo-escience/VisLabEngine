﻿using Fusion.Engine.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfEditorTest.FrameSelection;

namespace WpfEditorTest.UndoRedo
{
	internal class DeselectFrameCommand : IEditorCommand
	{
		private Frame _frame;

		public DeselectFrameCommand()
		{
			this._frame = SelectionManager.Instance.SelectedFrame;
		}

		public void Do()
		{
			SelectionManager.Instance.DeselectFrame();
		}

		public void Undo()
		{
			SelectionManager.Instance.SelectFrame(_frame);
		}
	}
}
