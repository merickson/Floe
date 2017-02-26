using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatWindow : Window
	{
		public ObservableCollection<ChatTabItem> Items { get; private set; }
		public ObservableCollection<NetworkTreeViewItem> NetworkTreeViewList;
		public ChatControl ActiveControl { get { return tabsChat.SelectedContent as ChatControl; } }

		public ChatWindow()
		{
			this.Items = new ObservableCollection<ChatTabItem>();
			this.NetworkTreeViewList = new ObservableCollection<NetworkTreeViewItem>();
			this.DataContext = this;
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(ChatWindow_Loaded);
			chatTreeView.ItemsSource = NetworkTreeViewList;
			chatTreeView.SelectedItemChanged += ChatTreeView_SelectedItemChanged;
		}

		private void ChatTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is NetworkTreeViewItem)
			{
				SwitchToPage(((NetworkTreeViewItem)e.NewValue).Page);
			}
			if (e.NewValue is ChannelTreeViewItem)
			{
				SwitchToPage(((ChannelTreeViewItem)e.NewValue).Page);
			}
		}

		public void AddPage(ChatPage page, bool switchToPage)
		{
			var item = new ChatTabItem(page);

			if (page.Type == ChatPageType.Server)
			{
				this.Items.Add(item);
				this.SubscribeEvents(page.Session);
				this.NetworkTreeViewList.Add(new NetworkTreeViewItem(item.Page));
			}
			else
			{
				for (int i = this.Items.Count - 1; i >= 0; --i)
				{
					if (this.Items[i].Page.Session == page.Session)
					{
						this.Items.Insert(i + 1, item);
						break;
					}
				}
				for (int i = this.NetworkTreeViewList.Count - 1; i >= 0; --i)
				{
					if (this.NetworkTreeViewList[i].Page.Session == page.Session)
					{
						this.NetworkTreeViewList[i].ChannelItems.Add(new ChannelTreeViewItem(item.Page));
						break;
					}
				}
			}
			if (switchToPage)
			{
				var oldItem = tabsChat.SelectedItem as TabItem;
				if (oldItem != null)
				{
					oldItem.IsSelected = false;
				}
				item.IsSelected = true;
			}
		}

		public void RemovePage(ChatPage page)
		{
			if (page.Type == ChatPageType.Server)
			{
				this.UnsubscribeEvents(page.Session);
			}
			page.Dispose();
			this.Items.Remove(this.Items.Where((i) => i.Page == page).FirstOrDefault());
			if (page.Type == ChatPageType.Server)
			{
				this.NetworkTreeViewList.Remove(NetworkTreeViewList.Where(tr => tr.Page == page).FirstOrDefault());
			}
			else
			{
				foreach (var nitem in this.NetworkTreeViewList)
				{
					nitem.ChannelItems.Remove(nitem.ChannelItems.Where(c => c.Page == page).FirstOrDefault());
				}
			}
		}

		public void SwitchToPage(ChatPage page)
		{
			var index = this.Items.Where((tab) => tab.Page == page).Select((t, i) => i).FirstOrDefault();
			if (index == 0)
			{
				foreach (var tbItem in this.Items)
				{
					if (tbItem.Page.Session == page.Session && tbItem.Page.Target == page.Target)
						index = this.Items.IndexOf(tbItem);
				}
			}
			tabsChat.SelectedIndex = index;
		}

		public ChatPage FindPage(ChatPageType type, IrcSession session, IrcTarget target)
		{
			return this.Items.Where((i) => i.Page.Type == type && i.Page.Session == session && i.Page.Target != null &&
				i.Page.Target.Equals(target)).Select((i) => i.Page).FirstOrDefault();
		}

		public void Attach(ChatPage page)
		{
			for (int i = this.Items.Count - 1; i >= 0; --i)
			{
				if (this.Items[i].Page.Session == page.Session)
				{
					this.Items.Insert(++i, new ChatTabItem(page));
					tabsChat.SelectedIndex = i;
					break;
				}
			}

			this.SwitchToPage(page);
		}

		public void Alert(string text)
		{
			if (_notifyIcon != null && _notifyIcon.IsVisible)
			{
				_notifyIcon.Show("IRC Alert", text);
			}
		}

		private void QuitAllSessions()
		{
			foreach (var i in this.Items.Where((i) => i.Page.Type == ChatPageType.Server).Select((i) => i))
			{
				if (i.Page.Session.State == IrcSessionState.Connected)
				{
					i.Page.Session.AutoReconnect = false;
					i.Page.Session.Quit("Leaving");
				}
			}
		}
	}
}
