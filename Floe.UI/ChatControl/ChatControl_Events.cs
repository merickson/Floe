﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Floe.Net;
using System.Windows.Documents;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Floe.UI
{
    public partial class ChatControl : ChatPage
    {
        private char[] _channelModes = new char[0];
        private string _topic = "", _prefix;
        private bool _hasDeactivated = false, _usingAlternateNick = false;
        private Tuple<int, string> tabData = Tuple.Create(0, "");
        private Window _window;
		public ObservableCollection<InviteItem> InvitesList;

        private void Session_StateChanged(object sender, EventArgs e)
        {
            var state = this.Session.State;
            this.IsConnected = state != IrcSessionState.Disconnected;

            if (state == IrcSessionState.Disconnected)
            {
                this.Write("Error", "Disconnected");
            }

            if (this.IsServer)
            {
                switch (state)
                {
                    case IrcSessionState.Connecting:
                        _usingAlternateNick = false;
                        this.Header = this.Session.NetworkName;
                        this.Write("Client", string.Format(
                            "Connecting to {0}:{1}", this.Session.Server, this.Session.Port));
                        break;
                    case IrcSessionState.Connected:
                        this.Header = this.Session.NetworkName;
                        App.DoEvent("connect");
                        if (this.Perform != null)
                        {
                            DoPerform(0);
                        }
                        break;
                }
                this.SetTitle();
            }
        }

        private void Session_ConnectionError(object sender, ErrorEventArgs e)
        {
            if (this.IsServer)
            {
                this.Write("Error", string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message);
            }
        }

		private void Session_Noticed(object sender, IrcMessageEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From.Prefix, IgnoreActions.Notice))
			{
				return;
			}

            bool b_doNotice = false;

            if (this.IsServer)
                b_doNotice = true;

            if (e.To.IsChannel)
            {
                if ((this.Target != null) && (this.Target.Equals(e.To)))
                    b_doNotice = true;
            }
            else
            {
                if ((IsDefault && this.IsChannel && (e.From != null) && _nickList.Contains(e.From.Name)) 
                    || ((e.From != null) && (this.Target != null) && this.Target.Equals(e.From))) // in case of private window is open, direct the notice there too
                    b_doNotice = true;
            }

            if (b_doNotice)
            {
                if ((e.From == null) && (this.IsServer))
                    this.Write("Notice", e.Message.Time, e.Text);
                else
                    this.Write("Notice", e.Message.Time, e.From.Prefix, e.Text, false, e.To); 
            }
            App.DoEvent("notice");
		}

		private void Session_PrivateMessaged(object sender, IrcMessageEventArgs e)
        {
            if (App.IsIgnoreMatch(e.From.Prefix, e.To.IsChannel ? IgnoreActions.Channel : IgnoreActions.Private))
            {
                return;
            }

            if (!this.IsServer)
            {
                if ((this.Target.IsChannel && this.Target.Equals(e.To)) ||
                    (!this.Target.IsChannel && this.Target.Equals(e.From) && !e.To.IsChannel))
                {
                    bool attn = false;
                    if (App.IsAttentionMatch(this.Session.Nickname, e.Text))
                    {
                        attn = true;
                        if (_window != null)
                        {
                            App.Alert(_window, string.Format("You received an alert from {0}", this.Target.Name));
                        }
                        if (this.VisualParent == null)
                        {
                            this.NotifyState = NotifyState.Alert;
                            App.DoEvent("inactiveAlert");
                        }
                        else if (_window != null && !_window.IsActive)
                        {
                            App.DoEvent("inactiveAlert");
                        }
                        else
                        {
                            App.DoEvent("activeAlert");
                        }
                    }
                    if (App.IsOverlayIconMatch(this.Session.Nickname, e.Text))
                    {
                        if(_window != null && !_window.IsActive)
                        {
                            var window = _window as ChatWindow;
                            window.setOverlayIcon(OverlayIconState.OwnNickname);
                        }
                    }
                    if(_window != null && !_window.IsActive)
                    {
                        var window = _window as ChatWindow;
                        window.setOverlayIcon(OverlayIconState.ChatActivity);
                    }

                    if (e.From != null && e.From.Prefix.Equals("*buffextras!buffextras@znc.in"))
                    {
                        int space = e.Text.IndexOf(' ');
                        String subject = e.Text.Substring(0, space);
                        String text = e.Text.Substring(space + 1);

                        IrcPeer peer = new IrcPeer(subject);
                        String styleKey = "Default";

                        // TODO: Rewrite text if it doesn't match how Floe writes it.
                        if (text.StartsWith("set mode")) // sNickMask + " set mode: " + sModes + " " + sArgs
                        {
                            styleKey = "Mode";
                        }
                        else if (text.StartsWith("kicked")) // OpNick.GetNickMask() + " kicked " + sKickedNick + " Reason: [" + sMessage + "]"
                        {
                            styleKey = "Kick";
                        }
                        else if (text.StartsWith("quit")) // Nick.GetNickMask() + " quit with message: [" + sMessage + "]"
                        {
                            styleKey = "Quit";
                        }
                        else if (text.StartsWith("joined")) // Nick.GetNickMask() + " joined"
                        {
                            styleKey = "Join";
                        }
                        else if (text.StartsWith("parted")) // Nick.GetNickMask() + " parted with message: [" + sMessage + "]"
                        {
                            styleKey = "Part";
                        }
                        else if (text.StartsWith("is now known as")) // OldNick.GetNickMask() + " is now known as " + sNewNick
                        {
                            styleKey = "Nick";
                        }
                        else if (text.StartsWith("changed the topic")) // Nick.GetNickMask() + " changed the topic to: " + sTopic
                        {
                            styleKey = "Topic";
                        }
                        this.Write(styleKey, e.Message.Time, string.Format("{0} {1}", subject, text));
                        //this.Write(styleKey, e.Message.Time, e.From.Prefix, e.Text, attn);
                    }
                    else
                    {
                        this.Write("Default", e.Message.Time, e.From.Prefix, e.Text, attn);

                        if (!this.Target.IsChannel)
                        {
                            if (e.Message.From.Prefix != _prefix)
                            {
                                _prefix = e.Message.From.Prefix;
                                this.SetTitle();
                            }
                            Interop.WindowHelper.FlashWindow(_window);
                            if (this.VisualParent == null)
                            {
                                App.DoEvent("privateMessage");
                            }
                            var window = App.Current.MainWindow as ChatWindow;
                            if (_window != null && !_window.IsActive)
                            {
                                window.setOverlayIcon(OverlayIconState.PrivateMessage);
                            }
                        }
                    }
                }
            }
        }

        private void Session_Kicked(object sender, IrcKickEventArgs e)
        {
            if (!this.IsServer && this.Target.Equals(e.Channel))
            {
                this.Write("Kick", e.Message.Time,
                    e.Kicker == null ? string.Format("{0} has been kicked ({1}", e.KickeeNickname, e.Text) :
                    string.Format("{0} has been kicked by {1} ({2})", e.KickeeNickname, e.Kicker.Name, e.Text));
                _nickList.Remove(e.KickeeNickname);
            }
        }

        private void Session_SelfKicked(object sender, IrcKickEventArgs e)
        {
            if (this.IsServer)
            {
                this.Write("Kick", e.Message.Time, string.Format("You have been kicked from {0} by {1} ({2})",
                    e.Channel, e.Kicker.Name, e.Text));
            }
        }

        private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
        {
            switch (e.Code)
            {
				case IrcCode.ERR_ERROR:
					if (this.IsServer)
					{
						this.Write("Error", e.Text);
						e.Handled = true;
					}
					break;
                case IrcCode.ERR_NICKNAMEINUSE:
                    if (this.IsServer && this.Session.State == IrcSessionState.Connecting)
                    {
                        if (_usingAlternateNick || string.IsNullOrEmpty(App.Settings.Current.User.AlternateNickname))
                        {
                            this.SetInputText("/nick ");
                        }
                        else
                        {
                            this.Session.Nick(App.Settings.Current.User.AlternateNickname);
                            _usingAlternateNick = true;
                        }
                    }
                    break;
                case IrcCode.RPL_TOPIC:
                    if (e.Message.Parameters.Count == 3 && !this.IsServer &&
                        this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
                    {
                        _topic = e.Message.Parameters[2];
                        this.SetTitle();
                        this.Write("Topic", e.Message.Time, string.Format("Topic is: {0}", _topic));
                    }
                    return;
                case IrcCode.RPL_TOPICSETBY:
                    if (e.Message.Parameters.Count == 4 && !this.IsServer &&
                        this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
                    {
                        this.Write("Topic", e.Message.Time, string.Format("Topic set by {0} on {1}", e.Message.Parameters[2],
                            FormatTime(e.Message.Parameters[3])));
                    }
                    return;
                case IrcCode.RPL_CHANNELCREATEDON:
                    if (e.Message.Parameters.Count == 3 && !this.IsServer &&
                        this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
                    {
                        //this.Write("ServerInfo", string.Format("* Channel created on {0}", this.FormatTime(e.Message.Parameters[2])));
                    }
                    return;
                case IrcCode.RPL_WHOISUSER:
                case IrcCode.RPL_WHOWASUSER:
                    if (e.Message.Parameters.Count == 6 && this.IsDefault)
                    {
						this.Write("ServerInfo", e.Message.Time,
                            string.Format("{1} " + (e.Code == IrcCode.RPL_WHOWASUSER ? "was" : "is") + " {2}@{3} {4} {5}",
                            (object[])e.Message.Parameters));
                        return;
                    }
                    break;
                case IrcCode.RPL_WHOISCHANNELS:
                    if (e.Message.Parameters.Count == 3 && this.IsDefault)
                    {
						this.Write("ServerInfo", e.Message.Time, string.Format("{1} is on {2}",
                            (object[])e.Message.Parameters));
                        return;
                    }
                    break;
                case IrcCode.RPL_WHOISSERVER:
                    if (e.Message.Parameters.Count == 4 && this.IsDefault)
                    {
						this.Write("ServerInfo", e.Message.Time, string.Format("{1} using {2} {3}",
                            (object[])e.Message.Parameters));
                        return;
                    }
                    break;
                case IrcCode.RPL_WHOISIDLE:
                    if (e.Message.Parameters.Count == 5 && this.IsDefault)
                    {
						this.Write("ServerInfo", e.Message.Time, string.Format("{0} has been idle {1}, signed on {2}",
                            e.Message.Parameters[1], FormatTimeSpan(e.Message.Parameters[2]),
							FormatTime(e.Message.Parameters[3])));
                        return;
                    }
                    break;
                case IrcCode.RPL_INVITING:
                    if (e.Message.Parameters.Count == 3 && this.IsDefault)
                    {
                        this.Write("ServerInfo", e.Message.Time, string.Format("Invited {0} to channel {1}",
                            e.Message.Parameters[1], e.Message.Parameters[2]));
                        return;
                    }
                    break;
				case IrcCode.RPL_ENDOFWHOIS:
					if (e.Message.Parameters.Count == 3 && this.IsDefault)
					{
						this.Write("ServerInfo", e.Message.Time, string.Format("{0} {1}",
							e.Message.Parameters[1], e.Message.Parameters[2]));
						return;
					}
					break;
                case IrcCode.RPL_LIST:
                case IrcCode.RPL_LISTSTART:
                case IrcCode.RPL_LISTEND:
                case IrcCode.RPL_ENDOFWHO:
                case IrcCode.RPL_WHOREPLY:
                    e.Handled = true;
                    break;
            }

            if (!e.Handled && ((int)e.Code < 200 && this.IsServer || this.IsDefault))
            {
                this.Write("ServerInfo", e.Message.Time, e.Text);
            }
        }

        private bool IsDefault
        {
            get
            {
                if(_window is ChannelWindow && _window.IsActive)
                {
                    return true;
                }
                else if (_window is ChatWindow)
                {
                    if(this.IsVisible)
                    {
                        return true;
                    }

                    if(this.IsServer &&
                        !((ChatWindow)_window).Items.Any((item) => item.IsVisible && item.Page.Session == this.Session) &&
                        !App.Current.Windows.OfType<ChannelWindow>().Any((cw) => cw.Session == this.Session && cw.IsActive))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
        {
            if (App.IsIgnoreMatch(e.From.Prefix, IgnoreActions.Ctcp))
            {
                return;
            }

            if (((this.IsChannel && this.Target.Equals(e.To)) ||
                (this.IsNickname && this.Target.Equals(e.From) && !e.To.IsChannel))
                && e.Command.Command == "ACTION")
            {
                string text = string.Join(" ", e.Command.Arguments);
                bool attn = false;
                if (App.IsAttentionMatch(this.Session.Nickname, text))
                {
                    attn = true;
                    if (_window != null)
                    {
                        Interop.WindowHelper.FlashWindow(_window);
                        App.Alert(_window, string.Format("You received an alert from {0}", this.Target.Name));
                    }
                    if (this.VisualParent == null)
                    {
                        this.NotifyState = NotifyState.Alert;
                        App.DoEvent("inactiveAlert");
                    }
                    else if (_window != null && !_window.IsActive)
                    {
                        App.DoEvent("inactiveAlert");
                    }
                    else
                    {
                        App.DoEvent("activeAlert");
                    }

                }
                this.Write("Action", e.Message.Time, string.Format("{0} {1}", e.From.Name, text), attn);
            }
            else if (this.IsServer && e.Command.Command != "ACTION" && e.From != null)
            {
                this.Write("Ctcp", e.Message.Time, e.From.Prefix, string.Format("[CTCP {1}] {2}",
                    e.From.Name, e.Command.Command,
                    e.Command.Arguments.Length > 0 ? string.Join(" ", e.Command.Arguments) : ""), false);
            }
        }

        private void Session_Joined(object sender, IrcJoinEventArgs e)
        {
            bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Join);

            if (!this.IsServer && this.Target.Equals(e.Channel))
            {
                if (!isIgnored)
                {
                    this.Write("Join", e.Message.Time, string.Format("{0} ({1}@{2}) has joined channel {3}",
                        e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
                }
                _nickList.Add(e.Who.Nickname);
            }
        }

        private void Session_Parted(object sender, IrcPartEventArgs e)
        {
            bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Part);

            if (!this.IsServer && this.Target.Equals(e.Channel))
            {
                if (!isIgnored)
                {
					if (string.IsNullOrEmpty(e.Text))
					{
						this.Write("Part", e.Message.Time, string.Format("{0} ({1}@{2}) has left channel {3}",
							e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString()));
					}
					else
					{
						this.Write("Part", e.Message.Time, string.Format("{0} ({1}@{2}) has left channel {3} ({4})",
							e.Who.Nickname, e.Who.Username, e.Who.Hostname, this.Target.ToString(), e.Text));
					}
				}
                _nickList.Remove(e.Who.Nickname);
            }
        }

        private void Session_NickChanged(object sender, IrcNickEventArgs e)
        {
            bool isIgnored = App.IsIgnoreMatch(e.Message.From, IgnoreActions.NickChange);

            if (this.IsChannel && _nickList.Contains(e.OldNickname))
            {
                if (!isIgnored)
                {
                    this.Write("Nick", e.Message.Time, string.Format("{0} is now known as {1}", e.OldNickname, e.NewNickname));
                }
                _nickList.ChangeNick(e.OldNickname, e.NewNickname);
            }
        }

        private void Session_SelfNickChanged(object sender, IrcNickEventArgs e)
        {
            if (this.IsServer || this.IsChannel)
            {
                this.Write("Nick", e.Message.Time, string.Format("You are now known as {0}", e.NewNickname));
            }
            this.SetTitle();

            if (this.IsChannel)
            {
                _nickList.ChangeNick(e.OldNickname, e.NewNickname);
            }
        }

        private void Session_TopicChanged(object sender, IrcTopicEventArgs e)
        {
            if (!this.IsServer && this.Target.Equals(e.Channel))
            {
                this.Write("Topic", e.Message.Time, string.Format("{0} changed topic to: {1}", e.Who.Name, e.Text));
                _topic = e.Text;
                this.SetTitle();
            }
        }

        private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
        {
            if (this.IsServer)
            {
                this.Write("Mode", e.Message.Time, string.Format("You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
            }
            this.SetTitle();
        }

        private void Session_UserQuit(object sender, IrcQuitEventArgs e)
        {
            bool isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Quit);

            if (this.IsChannel && _nickList.Contains(e.Who.Nickname))
            {
                if (!isIgnored)
                {
                    this.Write("Quit", e.Message.Time, string.Format("{0} ({1}@{2}) has quit ({3})",
                        e.Who.Nickname, e.Who.Username, e.Who.Hostname, e.Text));
                }
                _nickList.Remove(e.Who.Nickname);
            }
        }

        private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
        {
            if (!this.IsServer && this.Target.Equals(e.Channel))
            {
                if (e.Who != null)
                {
					this.Write("Mode", e.Message.Time, string.Format("{0} sets mode: {1}", e.Who.Name,
						string.Join(" ", IrcChannelMode.RenderModes(e.Modes))));
					_channelModes = AssembleChannelModes(e).ToCharArray();
                }
                this.SetTitle();
                foreach (var mode in e.Modes)
                {
                    _nickList.ProcessMode(mode);
                }
            }
        }

        private void Session_Invited(object sender, IrcInviteEventArgs e)
        {
            if (App.IsIgnoreMatch(e.From.Prefix, IgnoreActions.Invite))
            {
                return;
            }

            if (this.IsDefault || this.IsServer)
            {
                this.Write("Invite", e.Message.Time, string.Format("{0} invited you to channel {1}", e.From.Name, e.Channel));
				InvitesList.Add(new InviteItem(e.Channel, e.From.Name, DateTime.Now));
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                int c = 0;
                switch (e.Key)
                {
                    case Key.B:
                        c = 2;
                        break;
                    case Key.K:
                        c = 3;
                        break;
                    case Key.R:
                        c = 22;
                        break;
                    case Key.O:
                        c = 15;
                        break;
                    case Key.U:
                        c = 31;
                        break;
                }
                if ((int)c != 0)
                {
                    var s = new string((char)(c + 0x2500), 1);
                    this.Insert(s);
                }
            }

            switch (e.Key)
            {
                case Key.Enter:
                    this.SubmitInput();
                    break;
            }
        }

        private void txtInput_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = e.DataObject.GetData(typeof(string)) as string;
                if (text.Contains(Environment.NewLine))
                {
                    e.CancelCommand();

                    var parts = text.Split(Environment.NewLine.ToCharArray()).Where((s) => s.Trim().Length > 0).ToArray();
                    if (parts.Length > App.Settings.Current.Buffer.MaximumPasteLines)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (!App.Confirm(_window, string.Format("Are you sure you want to paste more than {0} lines?",
                                    App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warning"))
                                {
                                    return;
                                }
                                foreach (var part in parts)
                                {
                                    txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
                                    txtInput.Text += part;
                                    this.SubmitInput();
                                }
                            }));
                    }
                    else
                    {
                        foreach (var part in parts)
                        {
                            txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
                            txtInput.Text += part;
                            this.SubmitInput();
                        }
                    }
                }
            }
        }

        private void txtInput_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            int caretIndex, cmdIndex;
            SpellingError spellingError;

            txtInput.ContextMenu = new ContextMenu();
            caretIndex = txtInput.CaretIndex;

            cmdIndex = 0;
            spellingError = txtInput.GetSpellingError(caretIndex);

            if (spellingError != null)
            {
                foreach (string str in spellingError.Suggestions)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = str;
                    mi.FontWeight = FontWeights.Bold;
                    mi.Command = EditingCommands.CorrectSpellingError;
                    mi.CommandParameter = str;
                    mi.CommandTarget = txtInput;
                    txtInput.ContextMenu.Items.Insert(cmdIndex, mi);
                    cmdIndex++;
                }
                Separator separatorMenuItem1 = new Separator();
                txtInput.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem1);
                cmdIndex++;
                MenuItem ignoreAllMi = new MenuItem();
                ignoreAllMi.Header = "Ignore All";
                ignoreAllMi.Command = EditingCommands.IgnoreSpellingError;
                ignoreAllMi.CommandTarget = txtInput;
                txtInput.ContextMenu.Items.Insert(cmdIndex, ignoreAllMi);
                cmdIndex++;
                Separator separatorMenuItem2 = new Separator();
                txtInput.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem2);
                cmdIndex++;
            }
            MenuItem cutMi = new MenuItem();
            cutMi.Header = "Cut";
            cutMi.Command = ApplicationCommands.Cut;
            cutMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, cutMi);
            cmdIndex++;
            MenuItem copyMi = new MenuItem();
            copyMi.Header = "Copy";
            copyMi.Command = ApplicationCommands.Copy;
            copyMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, copyMi);
            cmdIndex++;
            MenuItem pasteMi = new MenuItem();
            pasteMi.Header = "Paste";
            pasteMi.Command = ApplicationCommands.Paste;
            pasteMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, pasteMi);
            cmdIndex++;
            Separator separatorMenuItem3 = new Separator();
            txtInput.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem3);
            cmdIndex++;

            MenuItem boldMi = new MenuItem();
            boldMi.Header = "Bold";
            boldMi.Command = ChatControl.InsertCommand;
            boldMi.CommandParameter = new string((char)0x2502, 1);
            boldMi.InputGestureText = "Ctrl+B";
            boldMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, boldMi);
            cmdIndex++;
            MenuItem underlineMi = new MenuItem();
            underlineMi.Header = "Underline";
            underlineMi.Command = ChatControl.InsertCommand;
            underlineMi.CommandParameter = new string((char)0x251F, 1);
            underlineMi.InputGestureText = "Ctrl+U";
            underlineMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, underlineMi);
            cmdIndex++;
            MenuItem reverseMi = new MenuItem();
            reverseMi.Header = "Reverse";
            reverseMi.Command = ChatControl.InsertCommand;
            reverseMi.CommandParameter = new string((char)0x2516, 1);
            reverseMi.InputGestureText = "Ctrl+R";
            reverseMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, reverseMi);
            cmdIndex++;
            MenuItem clearMi = new MenuItem();
            clearMi.Header = "Clear Formatting";
            clearMi.Command = ChatControl.InsertCommand;
            clearMi.CommandParameter = new string((char)0x250F, 1);
            clearMi.InputGestureText = "Ctrl+O";
            clearMi.CommandTarget = txtInput;
            txtInput.ContextMenu.Items.Insert(cmdIndex, clearMi);
            cmdIndex++;
        }

        private void lstNicknames_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var listItem = e.Source as ListBoxItem;
            if(listItem != null)
            {
                var nickItem = listItem.Content as NicknameItem;
                if(nickItem != null)
                {
                    ChatWindow.ChatCommand.Execute(nickItem.Nickname, this);
                }
            }
        }

        private void _window_Deactivated(object sender, EventArgs e)
        {
            _hasDeactivated = true;
            this.SelectedLink = null;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            _hasDeactivated = false;
            if (e.NewFocus == txtInput)
            {
                lstNicknames.SelectedItem = null;
            }
        }

        private void boxOutput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var link = boxOutput.SelectedLink;
            if (!string.IsNullOrEmpty(link))
            {
                if (Constants.UrlRegex.IsMatch(link))
                {
                    App.BrowseTo(link);
                }
                else
                {
                    ChatWindow.ChatCommand.Execute(this.GetNickWithoutLevel(link), this);
                }
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            this.SelectedLink = boxOutput.SelectedLink;
            if (!string.IsNullOrEmpty(this.SelectedLink))
            {
                if (Constants.UrlRegex.IsMatch(this.SelectedLink))
                {
                    boxOutput.ContextMenu = this.Resources["cmHyperlink"] as ContextMenu;
                }
                else
                {
                    if (this.Type == ChatPageType.DccChat)
                    {
                        return;
                    }
                    this.SelectedLink = this.GetNickWithoutLevel(this.SelectedLink);
                    boxOutput.ContextMenu = this.Resources["cmNickname"] as ContextMenu;
                }
                boxOutput.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
            else
            {

                boxOutput.ContextMenu = this.GetDefaultContextMenu();
                if (this.IsServer && boxOutput.ContextMenu != null)
                {
                    boxOutput.ContextMenu.Items.Refresh();
                }
            }

            base.OnContextMenuOpening(e);
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            var item = ((MenuItem)boxOutput.ContextMenu.Items[0]).ItemContainerGenerator.ItemFromContainer((DependencyObject)e.OriginalSource)
                as Floe.Configuration.ServerElement;
            if (item != null)
            {
                if (this.IsConnected)
                {
                    this.Session.Quit("Changing servers");
                }
                this.Connect(item);
            }
        }

        private void ChatControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtInput.ContextMenu = new ContextMenu();
            Keyboard.Focus(txtInput);
            this.SetTitle();

            if (_window == null)
            {
                _window = Window.GetWindow(this);
                if (_window != null)
                {
                    _window.Deactivated += new EventHandler(_window_Deactivated);
                }
            }
            else
            {
                _window = Window.GetWindow(this);
                this.NotifyState = NotifyState.None;
            }
        }

        private void ChatControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _hasDeactivated = true;
            this.SelectedLink = null;
            if (_window != null)
            {
                _window.Deactivated -= new EventHandler(_window_Deactivated);
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.SelectedLink = null;
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);
            if (focused is TextBox && focused != txtInput)
            {
                return;
            }

            if((Keyboard.Modifiers & ModifierKeys.Alt) == 0 &&
                (Keyboard.Modifiers & ModifierKeys.Control) == 0 &&
                !(FocusManager.GetFocusedElement(this) is ListBoxItem))
            {
                e.Handled = true;
                bool tabHit = false;

                switch (e.Key)
                {
                    case Key.PageUp:
                        boxOutput.PageUp();
                        break;
                    case Key.PageDown:
                        boxOutput.PageDown();
                        break;
                    case Key.Up:
                        if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) > 0)
                        {
                            e.Handled = false;
                            return;
                        }
                        else
                        {
                            if (_historyNode != null)
                            {
                                if (_historyNode.Next != null)
                                {
                                    _historyNode = _historyNode.Next;
                                    this.SetInputText(_historyNode.Value);
                                }
                            }
                            else if (_history.First != null)
                            {
                                _historyNode = _history.First;
                                this.SetInputText(_historyNode.Value);
                            }
                        }
                        break;
                    case Key.Down:
                        if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) < txtInput.LineCount - 1)
                        {
                            e.Handled = false;
                            return;
                        }
                        if (_historyNode != null)
                        {
                            _historyNode = _historyNode.Previous;
                            if (_historyNode != null)
                            {
                                this.SetInputText(_historyNode.Value);
                            }
                            else
                            {
                                txtInput.Clear();
                            }
                        }
                        else
                        {
                            txtInput.Clear();
                        }
                        break;
                    case Key.Tab:
                        if (this.IsChannel || this.IsNickname)
                        {
                            tabHit = true;
                            DoNickCompletion();
							DoChannelCompletion();
						}
                        break;
                    default:
                        Keyboard.Focus(txtInput);
                        e.Handled = false;
                        break;
                }

                if (tabHit != true)
                {
                    _nicknameComplete = null;
					_channameComplete = null;
				}
            }
            else if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                Keyboard.Focus(txtInput);
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                boxOutput.MouseWheelUp();
            }
            else
            {
                boxOutput.MouseWheelDown();
            }
            e.Handled = true;

            base.OnPreviewMouseWheel(e);
        }

        private void SubscribeEvents()
        {
            this.Session.StateChanged += new EventHandler<EventArgs>(Session_StateChanged);
            this.Session.ConnectionError += new EventHandler<ErrorEventArgs>(Session_ConnectionError);
            this.Session.Noticed += new EventHandler<IrcMessageEventArgs>(Session_Noticed);
            this.Session.PrivateMessaged += new EventHandler<IrcMessageEventArgs>(Session_PrivateMessaged);
            this.Session.Kicked += new EventHandler<IrcKickEventArgs>(Session_Kicked);
            this.Session.SelfKicked += new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
            this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
            this.Session.CtcpCommandReceived += new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
            this.Session.Joined += new EventHandler<IrcJoinEventArgs>(Session_Joined);
            this.Session.Parted += new EventHandler<IrcPartEventArgs>(Session_Parted);
            this.Session.NickChanged += new EventHandler<IrcNickEventArgs>(Session_NickChanged);
            this.Session.SelfNickChanged += new EventHandler<IrcNickEventArgs>(Session_SelfNickChanged);
            this.Session.TopicChanged += new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
            this.Session.UserModeChanged += new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
            this.Session.ChannelModeChanged += new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
            this.Session.UserQuit += new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
            this.Session.Invited += new EventHandler<IrcInviteEventArgs>(Session_Invited);
            DataObject.AddPastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

            this.IsConnected = !(this.Session.State == IrcSessionState.Disconnected);
            if (_nickList != null)
                _nickList.NickListChanged += OnNickListChanged;
        }

        private void UnsubscribeEvents()
        {
            this.Session.StateChanged -= new EventHandler<EventArgs>(Session_StateChanged);
            this.Session.ConnectionError -= new EventHandler<ErrorEventArgs>(Session_ConnectionError);
            this.Session.Noticed -= new EventHandler<IrcMessageEventArgs>(Session_Noticed);
            this.Session.PrivateMessaged -= new EventHandler<IrcMessageEventArgs>(Session_PrivateMessaged);
            this.Session.Kicked -= new EventHandler<IrcKickEventArgs>(Session_Kicked);
            this.Session.SelfKicked -= new EventHandler<IrcKickEventArgs>(Session_SelfKicked);
            this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
            this.Session.CtcpCommandReceived -= new EventHandler<CtcpEventArgs>(Session_CtcpCommandReceived);
            this.Session.Joined -= new EventHandler<IrcJoinEventArgs>(Session_Joined);
            this.Session.Parted -= new EventHandler<IrcPartEventArgs>(Session_Parted);
            this.Session.NickChanged -= new EventHandler<IrcNickEventArgs>(Session_NickChanged);
            this.Session.SelfNickChanged -= new EventHandler<IrcNickEventArgs>(Session_SelfNickChanged);
            this.Session.TopicChanged -= new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
            this.Session.UserModeChanged -= new EventHandler<IrcUserModeEventArgs>(Session_UserModeChanged);
            this.Session.ChannelModeChanged -= new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
            this.Session.UserQuit -= new EventHandler<IrcQuitEventArgs>(Session_UserQuit);
            this.Session.Invited -= new EventHandler<IrcInviteEventArgs>(Session_Invited);
            DataObject.RemovePastingHandler(txtInput, new DataObjectPastingEventHandler(txtInput_Pasting));

            if (_window != null)
            {
                _window.Deactivated -= new EventHandler(_window_Deactivated);
            }

            if (_nickList != null)
                _nickList.NickListChanged -= OnNickListChanged;
        }

        private void PrepareContextMenus()
        {
            var menu = this.Resources["cmServer"] as ContextMenu;
            if (menu != null)
            {
                NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
            }
            menu = this.Resources["cmNickList"] as ContextMenu;
            if (menu != null)
            {
                NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
            }
            menu = this.Resources["cmNickname"] as ContextMenu;
            if (menu != null)
            {
                NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
            }
            menu = this.Resources["cmHyperlink"] as ContextMenu;
            if (menu != null)
            {
                NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
            }
            menu = this.Resources["cmChannel"] as ContextMenu;
            if (menu != null)
            {
                NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
            }
			menu = this.Resources["cmUser"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
		}

		public static string FormatTime(string text)
        {
            int seconds = 0;
            if (!int.TryParse(text, out seconds))
            {
                return "";
            }
            var ts = new TimeSpan(0, 0, seconds);
            return new DateTime(1970, 1, 1).Add(ts).ToLocalTime().ToString();
        }

        public static string FormatTimeSpan(string text)
        {
            int seconds = 0;
            if (!int.TryParse(text, out seconds))
            {
                return "";
            }
            return new TimeSpan(0, 0, seconds).ToString();
        }

        public void OnNickListChanged(object source, EventArgs e)
        {
            this.SetTitle();
        }

        public string OrderChannelModes(string chanModes)
        {
            string resultModes = String.Empty;
            if (chanModes.Contains('s'))
                resultModes += 's';
            if (chanModes.Contains('p'))
                resultModes += 'p';
            if (chanModes.Contains('m'))
                resultModes += 'm';
            if (chanModes.Contains('t'))
                resultModes += 't';
            if (chanModes.Contains('i'))
                resultModes += 'i';
            if (chanModes.Contains('n'))
                resultModes += 'n';
            if (chanModes.Contains('r'))
                resultModes += 'r';
            if (chanModes.Contains('D'))
                resultModes += 'D';
            if (chanModes.Contains('d'))
                resultModes += 'd';
            if (chanModes.Contains('R'))
                resultModes += 'R';
            if (chanModes.Contains('l'))
                resultModes += 'l';
            if (chanModes.Contains('k'))
                resultModes += 'k';
            if (string.IsNullOrWhiteSpace(resultModes) || string.IsNullOrEmpty(resultModes))
                resultModes = chanModes;
            return resultModes;
        }

        public string AssembleChannelModes(IrcChannelModeEventArgs chModes)
        {
            List<string> StrChanModes = new List<string>(new string(_channelModes).Split(' ').ToList());
            string _chanModes = StrChanModes[0];
            List<string> modeParam = new List<string>();
            if (StrChanModes.Count > 1)
                modeParam = StrChanModes.Skip(1).ToList();
            StringBuilder sb = new StringBuilder();
            sb.Append(_chanModes);
            foreach (IrcChannelMode mode in chModes.Modes)
            {
                switch (mode.Mode)
                {
                    case 'O':
                    case 'o':
                    case 'v':
                    case 'h':
                    //case 'k':
                    //case 'l':
                    case 'b':
                    case 'e':
                    case 'I':
                    case 'f':
                    case 'j':
                    case 'q':
                        continue;
                }
                if (!_chanModes.Contains(mode.Mode) && mode.Set)
                {
                    sb.Append(mode.Mode);
                    if (!string.IsNullOrWhiteSpace(mode.Parameter))
                    {
                        if (mode.Mode == 'l')
                            modeParam.Insert(0, mode.Parameter);
                        else
                            modeParam.Add(mode.Parameter);
                    }
                }
                else if (_chanModes.Contains(mode.Mode))
                {
					if (!mode.Set)
					{
						sb.Remove(sb.ToString().IndexOf(mode.Mode), 1);
						if (modeParam.Count > 0)
						{
							if (mode.Mode == 'l')
								modeParam.RemoveAt(0);
							if (mode.Mode == 'k')
							{
								if (modeParam.Count > 1)
									modeParam.RemoveAt(1);
								else
									modeParam.RemoveAt(0);
							}
						}
					}
					else
					{
						if (mode.Mode == 'l')
							modeParam[0] = mode.Parameter;
						if (mode.Mode == 'k')
						{
							if (modeParam.Count > 1)
								modeParam[1] = mode.Parameter;
							else
								modeParam[0] = mode.Parameter;
						}
					}
				}
            }
            sb = new StringBuilder(OrderChannelModes(sb.ToString()));
            if (modeParam.Count > 0)
                sb.Append(" ").Append(string.Join<string>(" ", modeParam));
            _chanModes = sb.ToString();
            return _chanModes;
        }
    }
}
