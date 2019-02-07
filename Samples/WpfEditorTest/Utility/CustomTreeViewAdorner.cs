using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WpfEditorTest.ChildPanels;

namespace WpfEditorTest.Utility
{
	class CustomTreeViewAdorner : Adorner
	{
		FrameTreeView _window;
		DrawingContext _drawingContext;
		// Be sure to call the base class constructor.
		public CustomTreeViewAdorner( UIElement adornedElement, FrameTreeView window ) : base(adornedElement)
		{
			_window = window;
			IsHitTestVisible = false;
		}

		// A common way to implement an adorner's rendering behavior is to override the OnRender
		// method, which is called by the layout system as part of a rendering pass.
		protected override void OnRender( DrawingContext drawingContext )
		{
			_drawingContext = drawingContext;

			TreeView adornedElement = this.AdornedElement as TreeView;

			// Some arbitrary drawing implements.
			SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
			renderBrush.Opacity = 0.2;
			Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 2);
			var edgeHeight = 6;

			var leftEdgeUpperPoint = new Point(/*_window.LeftTextBlockPoint.X*/ - 0, /*_window.LeftTextBlockPoint.Y*/ - edgeHeight);
			var leftEdgeLowerPoint = new Point(/*_window.LeftTextBlockPoint.X*/ + 0, /*_window.LeftTextBlockPoint.Y*/ + edgeHeight);
			var rightEdgeUpperPoint = new Point(_window.RightTextBlockPoint.X - 0, /*_window.RightTextBlockPoint.Y*/ - edgeHeight);
			var rightEdgeLowerPoint = new Point(_window.RightTextBlockPoint.X + 0, /*_window.RightTextBlockPoint.Y*/ + edgeHeight);
			// Draw a circle at each corner.
			drawingContext.DrawLine(renderPen, new Point(0, 0), new Point(_window.RightTextBlockPoint.X, 0));
			drawingContext.DrawLine(renderPen, leftEdgeUpperPoint, leftEdgeLowerPoint);
			drawingContext.DrawLine(renderPen, rightEdgeUpperPoint, rightEdgeLowerPoint);
		}
	}
}
