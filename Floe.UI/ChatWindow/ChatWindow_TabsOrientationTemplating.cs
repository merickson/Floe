using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Floe.UI
{
	public class TabsOrientationTemplate : INotifyPropertyChanged
	{
		private Point _linearGradientBrushEndPoint;
		public Point LinearGradientBrushEndPoint
		{
			get { return _linearGradientBrushEndPoint; }
			set
			{
				_linearGradientBrushEndPoint = value;
				OnPropertyChanged("LinearGradientBrushEndPoint");
			}
		}

		private string _borderDock;
		public string BorderDock
		{
			get { return _borderDock; }
			set
			{
				_borderDock = value;
				OnPropertyChanged("BorderDock");
			}
		}

		private ScrollBarVisibility _verticalScrollBarVisibility;
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return _verticalScrollBarVisibility; }
			set
			{
				_verticalScrollBarVisibility = value;
				OnPropertyChanged("VerticalScrollBarVisibility");
			}
		}

		private Visibility _stackPanelVisibility;
		public Visibility StackPanelVisibility
		{
			get { return _stackPanelVisibility; }
			set
			{
				_stackPanelVisibility = value;
				OnPropertyChanged("StackPanelVisibility");
			}
		}

		private Orientation _stackPanelOrientation;
		public Orientation StackPanelOrientation
		{
			get { return _stackPanelOrientation; }
			set
			{
				_stackPanelOrientation = value;
				OnPropertyChanged("StackPanelOrientation");
			}
		}

		private Thickness _winTabSepBorderThickness;
		public Thickness WinTabSepBorderThickness
		{
			get { return _winTabSepBorderThickness; }
			set
			{
				_winTabSepBorderThickness = value;
				OnPropertyChanged("WinTabSepBorderThickness");
			}
		}

		private HorizontalAlignment _borderGridHorizontalAlignment;
		public HorizontalAlignment BorderGridHorizontalAlignment
		{
			get { return _borderGridHorizontalAlignment; }
			set
			{
				_borderGridHorizontalAlignment = value;
				OnPropertyChanged("BorderGridHorizontalAlignment");
			}
		}

		public TabsOrientationTemplate(Configuration.TabStripPosition NewPos)
		{
			ChangeOrientation(NewPos);
		}

		public void ChangeOrientation(Configuration.TabStripPosition NewPos)
		{
			switch (NewPos)
			{
				case Configuration.TabStripPosition.Top:
					LinearGradientBrushEndPoint = new Point(0, 1);
					BorderDock = "Top";
					VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
					StackPanelVisibility = Visibility.Visible;
					StackPanelOrientation = Orientation.Horizontal;
					WinTabSepBorderThickness = new Thickness(0, 0, 1, 0);
					BorderGridHorizontalAlignment = HorizontalAlignment.Center;
					break;
				case Configuration.TabStripPosition.Bottom:
					LinearGradientBrushEndPoint = new Point(0, 1);
					BorderDock = "Bottom";
					VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
					StackPanelVisibility = Visibility.Visible;
					StackPanelOrientation = Orientation.Horizontal;
					WinTabSepBorderThickness = new Thickness(0, 0, 1, 0);
					BorderGridHorizontalAlignment = HorizontalAlignment.Center;
					break;
				case Configuration.TabStripPosition.Left:
					LinearGradientBrushEndPoint = new Point(1, 0);
					BorderDock = "Left";
					VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
					StackPanelVisibility = Visibility.Collapsed;
					StackPanelOrientation = Orientation.Vertical;
					WinTabSepBorderThickness = new Thickness(0, 0, 0, 1);
					BorderGridHorizontalAlignment = HorizontalAlignment.Stretch;
					break;
				case Configuration.TabStripPosition.Right:
				default:
					LinearGradientBrushEndPoint = new Point(1, 0);
					BorderDock = "Right";
					VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
					StackPanelVisibility = Visibility.Collapsed;
					StackPanelOrientation = Orientation.Vertical;
					WinTabSepBorderThickness = new Thickness(0, 0, 0, 1);
					BorderGridHorizontalAlignment = HorizontalAlignment.Stretch;
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
