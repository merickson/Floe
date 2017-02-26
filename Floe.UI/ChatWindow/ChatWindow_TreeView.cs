using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Floe.Net;

namespace Floe.UI
{
    public class NetworkTreeViewItem
    {
        public ChatPage Page { get; private set; }
        public string NetworkName { get { return Page.Header == null ? "Not loaded" : Page.Header; } }

        public NetworkTreeViewItem(ChatPage page)
        {
            this.Page = page;
            ChannelItems = new ObservableCollection<ChannelTreeViewItem>();
        }

        public ObservableCollection<ChannelTreeViewItem> ChannelItems { get; set; }
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
