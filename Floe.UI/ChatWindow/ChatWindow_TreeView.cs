using System;
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

		public NetworkTreeViewItem(ChatPage page)
		{
			this.Page = page;
			this.Page.Session.StateChanged += Session_StateChanged;
			ChannelItems = new ObservableCollection<ChannelTreeViewItem>();
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			this.NetworkName = Page.Header;
		}

		public ObservableCollection<ChannelTreeViewItem> ChannelItems { get; set; }

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

	public class ChannelTreeViewItem
	{
		public ChatPage Page { get; private set; }
		public string ChannelName { get { return Page.Target.Name; } }

		public ChannelTreeViewItem(ChatPage page)
		{
			this.Page = page;
		}
	}
}
