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
using Fusion.Engine.Frames2.Managing;

namespace WpfEditorTest.Utility
{
	/// <summary>
	/// Interaction logic for ParentHighlightPanel.xaml
	/// </summary>
	public partial class ParentHighlightPanel : Border
	{

		private UIComponent _selectedFrame;
		private UIManager _uiManager;

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


				Width = _selectedFrame.Placement.Width;
				Height = _selectedFrame.Placement.Height;

				var globalTransform = _uiManager.GlobalTransform(_selectedFrame.Placement);

				var transform = new MatrixTransform(globalTransform.M11, globalTransform.M12,
													globalTransform.M21, globalTransform.M22,
													globalTransform.M31, globalTransform.M32);
				RenderTransform = transform;

				this.Visibility = Visibility.Visible;
			}
		}

		public ParentHighlightPanel( Fusion.Engine.Frames2.Managing.UIManager uiManager )
		{
			InitializeComponent();
			_uiManager = uiManager;
		}
	}
}
