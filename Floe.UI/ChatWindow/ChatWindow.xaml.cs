using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;
using System.Windows.Media;

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
			this.tabsChat.SelectionChanged += TabsChat_SelectionChanged;
			chatTreeView.ItemsSource = NetworkTreeViewList;
			chatTreeView.SelectedItemChanged += ChatTreeView_SelectedItemChanged;
		}

		private void TabsChat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			return;

			/* TODO: 
			 * Disabled because very much seems like TabsChat_SelectionChanged and ChatTreeView_SelectedItemChanged calls each other in an endless loop.
			 */
			ChatTabItem oldTabItem = null;
			ChatTabItem newTabItem = null;
			if ((e.RemovedItems.Count > 0) && (e.RemovedItems[0] is ChatTabItem))
				oldTabItem = (ChatTabItem)e.RemovedItems[0];
			else
				return;
			if ((e.AddedItems.Count > 0) && (e.AddedItems[0] is ChatTabItem))
				newTabItem = (ChatTabItem)e.AddedItems[0];
			else
				return;
			if (oldTabItem != null && newTabItem != null)
			{
				if (newTabItem.Page.Type == ChatPageType.Server)
				{
					NetworkTreeViewItem newNetworkItem = this.NetworkTreeViewList.Where((tr) => tr.Page == newTabItem.Page).FirstOrDefault();
					SelectTreeViewItem(chatTreeView.Items, newNetworkItem.NetworkName);
				}
				else
				{
					var newChanItem = FindTreeViewItem(newTabItem.Page);
					if (newChanItem is ChannelTreeViewItem)
					{
						ChannelTreeViewItem cItem = newChanItem as ChannelTreeViewItem;
						SelectTreeViewItem(chatTreeView.Items, cItem.ChannelName);
					}
				}
			}
		}

		private void SelectTreeViewItem(ItemCollection Collection, String Header)
		{
			if (Collection == null) return;
			foreach (var Item in Collection)
			{
				if (Item is NetworkTreeViewItem)
				{
					NetworkTreeViewItem nItem = Item as NetworkTreeViewItem;
					var tvItem = chatTreeView.ItemContainerGenerator.ContainerFromItem(nItem) as TreeViewItem;
					if (tvItem != null)
					{
						if (nItem.NetworkName.Equals(Header))
						{
							tvItem.IsSelected = true;
							return;
						}
						SelectTreeViewItem(tvItem.Items, Header);
					}
				}
				if (Item is ChannelTreeViewItem)
				{
					ChannelTreeViewItem cItem = Item as ChannelTreeViewItem;
					if (cItem.ChannelName.Equals(Header))
					{
						foreach (var tItem in chatTreeView.Items)
						{
							TreeViewItem TV = tItem as TreeViewItem;
							if (TV != null)
							{
								var tvItem = TV.ItemContainerGenerator.ContainerFromItem(cItem) as TreeViewItem;
								if (tvItem != null)
								{
									tvItem.IsSelected = true;
									return;
								}
							}
						}
					}
				}
			}
		}

		private void ChatTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is NetworkTreeViewItem)
			{
				NetworkTreeViewItem newItem = (NetworkTreeViewItem)e.NewValue;
				newItem.IsSelected = true;
				SwitchToPage(newItem.Page);
				if (e.OldValue is NetworkTreeViewItem)
				{
					NetworkTreeViewItem oldItem = (NetworkTreeViewItem)e.OldValue;
					oldItem.IsSelected = false;
				}
			}
			if (e.NewValue is ChannelTreeViewItem)
			{
				//SwitchToPage(((ChannelTreeViewItem)e.NewValue).Page);
				ChannelTreeViewItem newItem = (ChannelTreeViewItem)e.NewValue;
				newItem.IsSelected = true;
				SwitchToPage(newItem.Page);
				if (e.OldValue is ChannelTreeViewItem)
				{
					ChannelTreeViewItem oldItem = (ChannelTreeViewItem)e.OldValue;
					oldItem.IsSelected = false;
				}
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

		public object FindTreeViewItem(ChatPage page)
		{
			if (page.Type == ChatPageType.Server)
			{
				return this.NetworkTreeViewList.Where((tr) => tr.Page == page).FirstOrDefault();
			}
			else
			{
				for (int i = this.NetworkTreeViewList.Count - 1; i >= 0; --i)
				{
					if (this.NetworkTreeViewList[i].Page.Session == page.Session)
					{
						return this.NetworkTreeViewList[i].ChannelItems.Where(c => c.Page == page).FirstOrDefault();
					}
				}
			}
			return null;
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

		private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}
