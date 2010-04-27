﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Floe.Net;
using System.Windows;

namespace Floe.UI
{
	public sealed class ChatContext : DependencyObject
	{
		public IrcSession Session { get; private set; }

		public IrcTarget Target { get; private set; }

		public bool IsConnected { get { return this.Session.State == IrcSessionState.Connecting ||
			this.Session.State == IrcSessionState.Connected; } }

		public event EventHandler ChangingServer;

		public ChatContext(IrcSession ircSession, IrcTarget target)
		{
			this.Session = ircSession;
			this.Target = target;
		}

		public void OnChangeServer()
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				var handler = this.ChangingServer;
				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}));
		}

		public override string ToString()
		{
			return this.Target == null ? "Server" : this.Target.ToString();
		}
	}
}
