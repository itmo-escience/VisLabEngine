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
using Fusion.Engine.Frames2;

namespace WpfEditorTest.ChildPanels
{
	/// <summary>
	/// Interaction logic for ParentHighlightPanel.xaml
	/// </summary>
	public partial class ParentHighlightPanel : Border
	{

		private UIComponent _selectedFrame;
		public UIComponent SelectedFrame
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
				delta.X = _selectedFrame.BoundingBox.X;
				delta.Y = _selectedFrame.BoundingBox.Y;

				this.Visibility = Visibility.Visible;
			}
		}

		public ParentHighlightPanel()
		{
			InitializeComponent();
		}
	}
}
