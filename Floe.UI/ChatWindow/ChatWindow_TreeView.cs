﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Floe.Net;

namespace Floe.UI
{
	public class NetworkTreeViewItem : INotifyPropertyChanged
	{
		public ChatPage Page { get; private set; }
		public string NetworkName
		{
			get
			{
				return Page.Header == null ? "Not loaded" : Page.Header;
			}
			private set
			{
				OnPropertyChanged("NetworkName");
			}
		}

		private string _activityColor;
		public string ActivityColor
		{
			get { return _activityColor; }
			set
			{
				_activityColor = value;
				OnPropertyChanged("ActivityColor");
			}
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
				}
				OnPropertyChanged("IsSelected");
			}
		}

		public NetworkTreeViewItem(ChatPage page)
		{
			this.ActivityColor = "Black";
			this.Page = page;
			this.Page.Session.StateChanged += Session_StateChanged;
			ChannelItems = new ObservableCollection<ChannelTreeViewItem>();
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			this.NetworkName = Page.Header;
		}

		public ObservableCollection<ChannelTreeViewItem> ChannelItems { get; set; }

		public void OnNotifyStateChanged()
		{
			if (Page.NotifyState == NotifyState.None)
				ActivityColor = "Black";
			if (Page.NotifyState == NotifyState.NoiseActivity)
				ActivityColor = "Blue";
            if (Page.NotifyState == NotifyState.ActionActivity)
                ActivityColor = "Magenta";
            if (Page.NotifyState == NotifyState.ChatActivity)
				ActivityColor = "Red";
			if (Page.NotifyState == NotifyState.Alert)
				ActivityColor = "#FF00FF00";
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

	public class ChannelTreeViewItem : INotifyPropertyChanged
	{
		public ChatPage Page { get; private set; }
		public string ChannelName
		{
			get
			{
				if (Page.Target != null)
					return Page.Target.Name;

				if (Page.Type == ChatPageType.ChannelList)
					return "Channel List";

				if (Page.Type == ChatPageType.Debug)
					return Page.Id;

				return "Unknown ChatPageType";
			}
		}

		public ContextMenu CloseMenu { get; private set; }

		private string _activityColor;
		public string ActivityColor
		{
			get { return _activityColor; }
			set
			{
				_activityColor = value;
				OnPropertyChanged("ActivityColor");
			}
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
				}
				OnPropertyChanged("IsSelected");
			}
		}

		public ChannelTreeViewItem(ChatPage page)
		{
			this.ActivityColor = "Black";
			this.Page = page;
			this.Page.IsCloseable = true;
			CloseMenu = new ContextMenu();
			MenuItem CloseItem = new MenuItem();
			CloseItem.Command = ChatWindow.CloseTabCommand;
			CloseItem.CommandParameter = this.Page;
			CloseMenu.Items.Add(CloseItem);
		}

		public void OnNotifyStateChanged()
		{
			if (Page.NotifyState == NotifyState.None)
				ActivityColor = "Black";
			if (Page.NotifyState == NotifyState.NoiseActivity)
				ActivityColor = "Blue";
            if (Page.NotifyState == NotifyState.ActionActivity)
                ActivityColor = "Magenta";
            if (Page.NotifyState == NotifyState.ChatActivity)
				ActivityColor = "Red";
			if (Page.NotifyState == NotifyState.Alert)
				ActivityColor = "#FF00FF00";
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
