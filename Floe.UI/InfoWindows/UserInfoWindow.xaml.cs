using Floe.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Floe.UI.InfoWindows
{
	/// <summary>
	/// Interaction logic for UserInfoWindow.xaml
	/// </summary>
	public partial class UserInfoWindow : Window
	{
		private readonly IrcSession Session;
		private IrcCodeHandler whoisUserHandler;
		private IrcCodeHandler whoisChannelsHandler;
		private IrcCodeHandler whoisServerHandler;
		private IrcCodeHandler whoisIdleHandler;
		private IrcCodeHandler whoisInvitingHandler;
		private IrcCodeHandler whoisEndHandler;

		public UserInfoWindow(IrcSession session, IrcTarget target)
		{
			InitializeComponent();
			this.Session = session;
			this.Title = "User information about " + target.Name;
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			CaptureWhoisUser();
			CaptureWhoisChannels();
			CaptureWhoisServer();
			CaptureWhoisIdle();
			CaptureWhoisInvitings();
			CaptureWhoisEnd();
			this.Session.WhoIs(target.Name);
		}

		private void CaptureWhoisUser()
		{
			whoisUserHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISUSER);

			this.Session.AddHandler(whoisUserHandler);
		}

		private void CaptureWhoisChannels()
		{
			whoisChannelsHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISCHANNELS);

			this.Session.AddHandler(whoisChannelsHandler);
		}

		private void CaptureWhoisServer()
		{
			whoisServerHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISSERVER);

			this.Session.AddHandler(whoisServerHandler);
		}

		private void CaptureWhoisIdle()
		{
			whoisIdleHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISIDLE);
			this.Session.AddHandler(whoisIdleHandler);
		}

		private void CaptureWhoisInvitings()
		{
			whoisInvitingHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_INVITING);

			this.Session.AddHandler(whoisInvitingHandler);
		}

		private void CaptureWhoisEnd()
		{
			whoisEndHandler = new IrcCodeHandler((e) =>
			{
				this.Session.RemoveHandler(whoisUserHandler);
				this.Session.RemoveHandler(whoisChannelsHandler);
				this.Session.RemoveHandler(whoisServerHandler);
				this.Session.RemoveHandler(whoisIdleHandler);
				this.Session.RemoveHandler(whoisInvitingHandler);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_ENDOFWHOIS);

			this.Session.AddHandler(whoisEndHandler);
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.RPL_WHOISUSER:
				case IrcCode.RPL_WHOWASUSER:
					if (e.Message.Parameters.Count == 6)
					{
						//this.Write("ServerInfo", e.Message.Time,
						//	string.Format("{1} " + (e.Code == IrcCode.RPL_WHOWASUSER ? "was" : "is") + " {2}@{3} {4} {5}",
						//	(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISCHANNELS:
					if (e.Message.Parameters.Count == 3)
					{
						//this.Write("ServerInfo", e.Message.Time, string.Format("{1} is on {2}",
						//	(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISSERVER:
					if (e.Message.Parameters.Count == 4)
					{
						//this.Write("ServerInfo", e.Message.Time, string.Format("{1} using {2} {3}",
						//	(object[])e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RPL_WHOISIDLE:
					if (e.Message.Parameters.Count == 5)
					{
						//this.Write("ServerInfo", e.Message.Time, string.Format("{0} has been idle {1}, signed on {2}",
						//	e.Message.Parameters[1], this.FormatTimeSpan(e.Message.Parameters[2]),
						//	this.FormatTime(e.Message.Parameters[3])));
						return;
					}
					break;
				case IrcCode.RPL_INVITING:
					if (e.Message.Parameters.Count == 3)
					{
						//this.Write("ServerInfo", e.Message.Time, string.Format("Invited {0} to channel {1}",
						//	e.Message.Parameters[1], e.Message.Parameters[2]));
						return;
					}
					break;
				default:
					{
						e.Handled = true;
					}
					break;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			base.OnClosing(e);
		}
	}
}
