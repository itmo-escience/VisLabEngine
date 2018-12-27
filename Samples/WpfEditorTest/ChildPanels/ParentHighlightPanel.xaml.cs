using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for ParentHighlightPanel.xaml
	/// </summary>
	public partial class ParentHighlightPanel : Border
	{

		private Fusion.Engine.Frames.Frame _selectedFrame;
		public Fusion.Engine.Frames.Frame SelectedFrame
		{
			get => _selectedFrame;
			set
			{
				_selectedFrame = value;
				if (_selectedFrame == null)
				{
					this.Visibility = Visibility.Collapsed;
					return;
				}


				Width = _selectedFrame.Width;
				Height = _selectedFrame.Height;

				var delta = new TranslateTransform();
				RenderTransform = delta;
				delta.X = _selectedFrame.GlobalRectangle.X;
				delta.Y = _selectedFrame.GlobalRectangle.Y;

				this.Visibility = Visibility.Visible;
			}
		}

		public ParentHighlightPanel()
		{
			InitializeComponent();
		}
	}
}
