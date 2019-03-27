namespace WpfEditorTest.Commands
{
	public interface IEditorCommand
	{
		bool IsDirty { get; }

		void Do();

		void Undo();
	}
}
