using Floe.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Floe.UI.DebugControl
{
    /// <summary>
    /// Interaction logic for DebugControl.xaml
    /// </summary>
    public partial class DebugControl : ChatPage
    {
        public DebugControl(IrcSession session, string id = "@debug")
            : base(ChatPageType.Debug, session, null, id)
        {
            InitializeComponent();
            this.Header = base.Id;
            this.Title = string.Format("{0} - {1} - Debug data on {2}", App.Product, this.Session.Nickname, this.Session.NetworkName);
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            this.Session.RawMessageReceived += new EventHandler<IrcEventArgs>(Session_RawMessageReceived);
            this.Session.RawMessageSent += new EventHandler<IrcEventArgs>(Session_RawMessageSent);
        }

        private void UnsubscribeEvents()
        {
            this.Session.RawMessageReceived -= new EventHandler<IrcEventArgs>(Session_RawMessageReceived);
            this.Session.RawMessageSent -= new EventHandler<IrcEventArgs>(Session_RawMessageSent);
        }

        private void Session_RawMessageReceived(object sender, IrcEventArgs e)
        {
            Write("Default", string.Format("[ IN] {0}", e.Message.ToString()));
        }

        private void Session_RawMessageSent(object sender, IrcEventArgs e)
        {
            Write("Default", string.Format("[OUT] {0}", e.Message.ToString()));
        }

        #region Write-ies
        private void Write(string styleKey, DateTime date, int nickHashCode, string nick, string text)
        {
            var cl = new ChatLine(styleKey, date, nickHashCode, nick, text, ChatMarker.None);

            if (this.VisualParent == null)
            {
                this.NotifyState = NotifyState.ChatActivity;
            }

            debugOutputBox.AppendLine(cl);
        }

        private void Write(string styleKey, int nickHashCode, string nick, string text)
        {
            Write(styleKey, DateTime.Now, nickHashCode, nick, text);
        }

        private void Write(string styleKey, DateTime date, string text)
        {
            this.Write(styleKey, date, 0, null, text);
        }

        private void Write(string styleKey, string text)
        {
            this.Write(styleKey, DateTime.Now, 0, null, text);
        }
        #endregion

        public override void Dispose()
        {
            UnsubscribeEvents();
        }
    }
}
