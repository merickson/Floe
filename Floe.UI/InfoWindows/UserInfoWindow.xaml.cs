using System;
using System.Windows;
using System.ComponentModel;
using Floe.Net;

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
		private IrcCodeHandler whoisAwayHandler;
		private IrcCodeHandler whoisOperatorHandler;
		private IrcCodeHandler whoisAccountHandler;
		private IrcCodeHandler whoisUserHostHandler;
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
			CaptureWhoisAway();
			CaptureWhoisOperator();
			CaptureWhoisAccount();
			CaptureWhoisUserHost();
			CaptureWhoisIdle();
			CaptureWhoisInvitings();
			CaptureWhoisEnd();
			this.Session.WhoIs(target.Name, target.Name);
		}

		private void CaptureWhoisUser()
		{
			whoisUserHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockRealName.Text = "Real Name:";
				TxtBlockRealNameData.Text = e.Message.Parameters[5];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISUSER);

			this.Session.AddHandler(whoisUserHandler);
		}

		private void CaptureWhoisChannels()
		{
			whoisChannelsHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockChannels.Visibility = Visibility.Visible;
				TxtBlockChannels.Text = "Is on:";
				TxtBlockChannelsData.Visibility = Visibility.Visible;
				TxtBlockChannelsData.Text = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISCHANNELS);

			this.Session.AddHandler(whoisChannelsHandler);
		}

		private void CaptureWhoisServer()
		{
			whoisServerHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockServer.Text = "Is using:";
				TxtBlockServerData.Text = e.Message.Parameters[2] + " (" + e.Message.Parameters[3] + ")";
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISSERVER);

			this.Session.AddHandler(whoisServerHandler);
		}

		private void CaptureWhoisAway()
		{
			whoisAwayHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockAway.Visibility = Visibility.Visible;
				TxtBlockAway.Text = "Is away:";
				TxtBlockAwayData.Visibility = Visibility.Visible;
				TxtBlockAwayData.Text = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_AWAY);

			this.Session.AddHandler(whoisAwayHandler);
		}

		private void CaptureWhoisOperator()
		{
			whoisOperatorHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockOper.Visibility = Visibility.Visible;
				TxtBlockOper.Text = "Status:";
				TxtBlockOperData.Visibility = Visibility.Visible;
				TxtBlockOperData.Text = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISOPERATOR);

			this.Session.AddHandler(whoisOperatorHandler);
		}

		private void CaptureWhoisAccount()
		{
			whoisAccountHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockAccount.Visibility = Visibility.Visible;
				TxtBlockAccount.Text = "Is logged in as:";
				TxtBlockAccountData.Visibility = Visibility.Visible;
				TxtBlockAccountData.Text = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISACCOUNT);

			this.Session.AddHandler(whoisAccountHandler);
		}

		private void CaptureWhoisUserHost()
		{
			whoisUserHostHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockUserHost.Visibility = Visibility.Visible;
				TxtBlockUserHost.Text = "Actual userhost:";
				TxtBlockUserHostData.Visibility = Visibility.Visible;
				TxtBlockUserHostData.Text = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISUSERHOST);

			this.Session.AddHandler(whoisUserHostHandler);
		}

		private void CaptureWhoisIdle()
		{
			whoisIdleHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockIdle.Visibility = Visibility.Visible;
				TxtBlockIdle.Text = "Is idle:";
				TxtBlockIdleData.Visibility = Visibility.Visible;
				TxtBlockIdleData.Text = ChatControl.FormatTimeSpan(e.Message.Parameters[2]) + ", Signed on: " + ChatControl.FormatTime(e.Message.Parameters[3]);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_WHOISIDLE);
			this.Session.AddHandler(whoisIdleHandler);
		}

		private void CaptureWhoisInvitings()
		{
			whoisInvitingHandler = new IrcCodeHandler((e) =>
			{
				TxtBlockInvite.Visibility = Visibility.Visible;
				TxtBlockInvite.Text = "Is invited to:";
				TxtBlockInviteData.Visibility = Visibility.Visible;
				TxtBlockInviteData.Text = e.Message.Parameters[2];
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
				this.Session.RemoveHandler(whoisAwayHandler);
				this.Session.RemoveHandler(whoisOperatorHandler);
				this.Session.RemoveHandler(whoisAccountHandler);
				this.Session.RemoveHandler(whoisUserHostHandler);
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
						TxtBlockChannelsData.Text = e.Message.Parameters[2];
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
				case IrcCode.RPL_AWAY:
					if (e.Message.Parameters.Count == 4)
					{
						TxtBlockAwayData.Text = e.Message.Parameters[2];
					}
					break;
				case IrcCode.RPL_UNAWAY:
					{
						TxtBlockAway.Visibility = Visibility.Collapsed;
						TxtBlockAwayData.Visibility = Visibility.Collapsed;
						TxtBlockAwayData.Text = null;
					}
					break;
				case IrcCode.RPL_WHOISOPERATOR:
					if (e.Message.Parameters.Count == 5)
					{
						TxtBlockOper.Visibility = Visibility.Visible;
						TxtBlockOper.Text = "Status:";
						TxtBlockOperData.Visibility = Visibility.Visible;
						TxtBlockOperData.Text = e.Message.Parameters[2];
					}
					break;
				case IrcCode.RPL_WHOISACCOUNT:
					{
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
				case IrcCode.RPL_WHOISIDLE:
					if (e.Message.Parameters.Count == 5)
					{
						TxtBlockIdle.Visibility = Visibility.Visible;
						TxtBlockIdle.Text = "Is idle:";
						TxtBlockIdleData.Visibility = Visibility.Visible;
						TxtBlockIdleData.Text = ChatControl.FormatTimeSpan(e.Message.Parameters[2]) + ", Signed on: " + ChatControl.FormatTime(e.Message.Parameters[3]);
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
