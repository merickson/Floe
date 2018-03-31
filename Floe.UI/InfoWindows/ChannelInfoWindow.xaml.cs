using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Windows.Data;
using Floe.Net;

namespace Floe.UI
{
	public class InviteItem
	{
		public string InvitedTo { get; }
		public string InvitedBy { get; }
		public DateTime? Timestamp { get; }

		public InviteItem(string invitedTo, string invitedBy, DateTime? timestamp)
		{
			this.InvitedTo = invitedTo;
			this.InvitedBy = invitedBy;
			this.Timestamp = timestamp;
		}

		public InviteItem(string invitedTo) : this(invitedTo, null, null)
		{
		}
	}
}

namespace Floe.UI.InfoWindows
{
	public class MaskItem
	{
		public string Mask { get; set; }
		public string SetBy { get; private set; }
		public DateTime Timestamp { get; private set; }

		public MaskItem(string mask, string setby, DateTime timestamp)
		{
			Mask = mask;
			SetBy = setby;
			Timestamp = timestamp;
		}
	}

	public class BanItem : MaskItem
	{
		public BanItem(string mask, string setby, DateTime timestamp)
			:base(mask, setby, timestamp)
		{

		}
	}

	public class ExceptItem : MaskItem
	{
		public ExceptItem(string mask, string setby, DateTime timestamp)
			: base(mask, setby, timestamp)
		{

		}
	}

	public class MaskListType<T> : ObservableCollection<T>
	{
		private int _selectedMaskIndex = -1;
		public int SelectedMaskIndex
		{
			get
			{
				return _selectedMaskIndex;
			}
			set
			{
				_selectedMaskIndex = value;
				ChannelInfoWindow.IsBanChanged = false;
				ChannelInfoWindow.IsExceptChanged = false;
				if (typeof(T) == typeof(BanItem))
					ChannelInfoWindow.IsBanChanged = true;
				if (typeof(T) == typeof(ExceptItem))
					ChannelInfoWindow.IsExceptChanged = true;
			}
		}

		public T SelectedMaskItem
		{
			get
			{
				if (SelectedMaskIndex > -1)
					return this[SelectedMaskIndex];
				else
					return default(T);
			}
		}
	}

	public class BanListType : MaskListType<BanItem>
	{ }

	public class ExceptListType : MaskListType<ExceptItem>
	{ }

	/// <summary>
	/// Interaction logic for ChannelInfoWindow.xaml
	/// </summary>
	public partial class ChannelInfoWindow : Window, INotifyPropertyChanged
	{
		public static bool IsBanChanged = false;
		public static bool IsExceptChanged = false;
		private bool IsNewRowAdding = false;
		private bool bSkipKeySet = false;
		private ChatControl ChatControl; 
		private readonly IrcSession Session;
		private readonly IrcTarget Target;

		public readonly static RoutedUICommand AddBanMaskCommand = new RoutedUICommand("Add", "AddBanMask", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand EditBanMaskCommand = new RoutedUICommand("Edit", "EditBanMask", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand RemoveBanMaskCommand = new RoutedUICommand("Remove", "RemoveBanMask", typeof(ChannelInfoWindow));

		public readonly static RoutedUICommand Mode_s_Command = new RoutedUICommand("Mode s toggle", "Mode_s_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_p_Command = new RoutedUICommand("Mode p toggle", "Mode_p_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_m_Command = new RoutedUICommand("Mode m toggle", "Mode_m_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_t_Command = new RoutedUICommand("Mode t toggle", "Mode_t_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_i_Command = new RoutedUICommand("Mode i toggle", "Mode_i_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_n_Command = new RoutedUICommand("Mode n toggle", "Mode_n_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_r_Command = new RoutedUICommand("Mode r toggle", "Mode_r_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_D_Command = new RoutedUICommand("Mode D toggle", "Mode_D_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_d_Command = new RoutedUICommand("Mode d toggle", "Mode_d_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_R_Command = new RoutedUICommand("Mode R toggle", "Mode_R_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_l_Command = new RoutedUICommand("Mode l toggle", "Mode_l_toggle", typeof(ChannelInfoWindow));
		public readonly static RoutedUICommand Mode_k_Command = new RoutedUICommand("Mode k toggle", "Mode_k_toggle", typeof(ChannelInfoWindow));

		private string _topicStr;
		public string TopicStr
		{
			get { return _topicStr; }
			set
			{
				if ((_topicStr != value) && ((!this.Session.ContainsHandler(topicHandler)) && (!this.Session.ContainsHandler(noTopicHandler))))
				{
					if (String.IsNullOrEmpty(value))
						this.Session.Topic(this.Target.Name, ":");
					else
						this.Session.Topic(this.Target.Name, value);
				}
				_topicStr = value;
				OnPropertyChanged("TopicStr");
			}
		}

		private string _limitStrNr;
		public string LimitStrNr
		{
			get { return _limitStrNr; }
			set
			{
				if (!String.IsNullOrEmpty(value) && !int.TryParse(value, out int intval))
					return;

				if ((_limitStrNr != value) && (!this.Session.ContainsHandler(modesHandler)))
				{
					if (!String.IsNullOrEmpty(value))
						this.Session.Mode(this.Target.Name, "+l " + value);
					else
						this.Session.Mode(this.Target.Name, "-l");
				}
				_limitStrNr = value;
				OnPropertyChanged("LimitStrNr");
			}
		}

		private string _keyStr;
		public string KeyStr
		{
			get { return _keyStr; }
			set
			{
				if ((_keyStr != value) && (!this.Session.ContainsHandler(modesHandler)))
				{
					if (!String.IsNullOrEmpty(value))
					{
						if (!String.IsNullOrEmpty(_keyStr))
						{
							bSkipKeySet = true;
							this.Session.Mode(this.Target.Name, "-k " + _keyStr);
						}
						this.Session.Mode(this.Target.Name, "+k " + value);
					}
					else if (!String.IsNullOrEmpty(_keyStr))
						this.Session.Mode(this.Target.Name, "-k " + _keyStr);
				}
				_keyStr = value;
				OnPropertyChanged("KeyStr");
			}
		}

		#region Mode_x_Toggleds
		private bool _mode_s_Toggled;
		public bool Mode_s_Toggled
		{
			get { return _mode_s_Toggled; }
			set { _mode_s_Toggled = value; OnPropertyChanged("Mode_s_Toggled"); }
		}

		private bool _mode_p_Toggled;
		public bool Mode_p_Toggled
		{
			get { return _mode_p_Toggled; }
			set { _mode_p_Toggled = value; OnPropertyChanged("Mode_p_Toggled"); }
		}

		private bool _mode_m_Toggled;
		public bool Mode_m_Toggled
		{
			get { return _mode_m_Toggled; }
			set { _mode_m_Toggled = value; OnPropertyChanged("Mode_m_Toggled"); }
		}

		private bool _mode_t_Toggled;
		public bool Mode_t_Toggled
		{
			get { return _mode_t_Toggled; }
			set { _mode_t_Toggled = value; OnPropertyChanged("Mode_t_Toggled"); }
		}

		private bool _mode_i_Toggled;
		public bool Mode_i_Toggled
		{
			get { return _mode_i_Toggled; }
			set { _mode_i_Toggled = value; OnPropertyChanged("Mode_i_Toggled"); }
		}

		private bool _mode_n_Toggled;
		public bool Mode_n_Toggled
		{
			get { return _mode_n_Toggled; }
			set { _mode_n_Toggled = value; OnPropertyChanged("Mode_n_Toggled"); }
		}

		private bool _mode_r_Toggled;
		public bool Mode_r_Toggled
		{
			get { return _mode_r_Toggled; }
			set { _mode_r_Toggled = value; OnPropertyChanged("Mode_r_Toggled"); }
		}

		private bool _mode_D_Toggled;
		public bool Mode_D_Toggled
		{
			get { return _mode_D_Toggled; }
			set { _mode_D_Toggled = value; OnPropertyChanged("Mode_D_Toggled"); }
		}

		private bool _mode_d_Toggled;
		public bool Mode_d_Toggled
		{
			get { return _mode_d_Toggled; }
			set { _mode_d_Toggled = value; OnPropertyChanged("Mode_d_Toggled"); }
		}

		private bool _mode_R_Toggled;
		public bool Mode_R_Toggled
		{
			get { return _mode_R_Toggled; }
			set { _mode_R_Toggled = value; OnPropertyChanged("Mode_R_Toggled"); }
		}

		private bool _mode_l_Toggled;
		public bool Mode_l_Toggled
		{
			get { return _mode_l_Toggled; }
			set { _mode_l_Toggled = value; OnPropertyChanged("Mode_l_Toggled"); }
		}

		private bool _mode_k_Toggled;
		public bool Mode_k_Toggled
		{
			get { return _mode_k_Toggled; }
			set { _mode_k_Toggled = value; OnPropertyChanged("Mode_k_Toggled"); }
		}
		#endregion

		public BanListType BanList { get; set; }
		private List<BanItem> burstBanList;
		private List<ExceptItem> burstExceptList;
		public ExceptListType ExceptsList { get; set; }
		public ObservableCollection<InviteItem> InvitesList { get; private set; }

		private IrcCodeHandler noTopicHandler, topicHandler, topicSetByHandler;
		private IrcCodeHandler modesHandler, channelCreatedOnHandler;
		private IrcCodeHandler bansHandler, bansEndHandler;
		private IrcCodeHandler exceptionsHandler, exceptionsEndHandler;
		private IrcCodeHandler invitesHandler, invitesEndHandler;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public ChannelInfoWindow(ChatControl chatControl)
		{
			this.ChatControl = chatControl;
			this.Session = this.ChatControl.Session;
			this.Target = this.ChatControl.Target;
			InitializeComponent();
			this.Title = "Channel info " + Target.Name;
			BanList = new BanListType();
			burstBanList = new List<BanItem>();
			burstExceptList = new List<ExceptItem>();
			ExceptsList = new ExceptListType();
			InvitesList = ChatControl.InvitesList;
			SubscribeEvents();
			CaptureTopic();
			CaptureModes();
			CaptureBans();
			CaptureExceptions();
			if (InvitesList.Count == 0)
				CaptureInvites();
			this.DataContext = this;
		}

		private void CaptureTopic()
		{
			noTopicHandler = new IrcCodeHandler((e) =>
			{
				TopicStr = null;
				e.Handled = true;
				return true;
			}, IrcCode.RPL_NOTOPIC);

			topicHandler = new IrcCodeHandler((e) =>
			{
				TopicStr = e.Message.Parameters[2];
				e.Handled = true;
				return true;
			}, IrcCode.RPL_TOPIC);

			topicSetByHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_TOPICSETBY);

			this.Session.AddHandler(topicHandler);
			this.Session.AddHandler(noTopicHandler);
			this.Session.AddHandler(topicSetByHandler);
			this.Session.Topic(this.Target.Name);
		}

		private void CaptureModes()
		{
			modesHandler = new IrcCodeHandler((e) =>
			{
				ToggleModeButtons(IrcChannelMode.ParseModes(String.Join(" ", e.Message.Parameters.Skip(2))));
				e.Handled = true;
				return true;
			}, IrcCode.RPL_CHANNELMODEIS);

			channelCreatedOnHandler = new IrcCodeHandler((e) =>
			{
				e.Handled = true;
				return true;
			}, IrcCode.RPL_CHANNELCREATEDON);

			this.Session.AddHandler(modesHandler);
			this.Session.AddHandler(channelCreatedOnHandler);
			this.Session.Mode(this.Target);
		}

		private void CaptureBans()
		{
			bansHandler = new IrcCodeHandler((e) =>
			{
				if (e.Message.Parameters.Count == 5 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
				{
					burstBanList.Add(new BanItem(e.Message.Parameters[2], e.Message.Parameters[3], UnixTimestamp.DateTimeFromTimestamp(int.Parse(e.Message.Parameters[4]))));
				}
				this.Session.AddHandler(bansHandler);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_BANLIST);

			bansEndHandler = new IrcCodeHandler((e) =>
			{
				this.Session.RemoveHandler(bansHandler);
				burstBanList = burstBanList.OrderBy(b => b.Timestamp).ToList();
				foreach (BanItem item in burstBanList)
					BanList.Add(item);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_ENDOFBANLIST);

			this.Session.AddHandler(bansHandler);
			this.Session.AddHandler(bansEndHandler);
			this.Session.Mode(this.Target.Name, "+b");
		}

		private void CaptureExceptions()
		{
			exceptionsHandler = new IrcCodeHandler((e) =>
			{
				if (e.Message.Parameters.Count == 5 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
				{
					burstExceptList.Add(new ExceptItem(e.Message.Parameters[2], e.Message.Parameters[3], UnixTimestamp.DateTimeFromTimestamp(int.Parse(e.Message.Parameters[4]))));
				}
				this.Session.AddHandler(exceptionsHandler);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_EXCEPTLIST);

			exceptionsEndHandler = new IrcCodeHandler((e) =>
			{
				this.Session.RemoveHandler(exceptionsHandler);
				burstExceptList = burstExceptList.OrderBy(b => b.Timestamp).ToList();
				foreach (ExceptItem item in burstExceptList)
					ExceptsList.Add(item);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_ENDOFEXCEPTLIST);

			this.Session.AddHandler(exceptionsHandler);
			this.Session.AddHandler(exceptionsEndHandler);
			this.Session.Mode(this.Target.Name, "+e");
		}

		private void CaptureInvites()
		{
			invitesHandler = new IrcCodeHandler((e) =>
			{
				InvitesList.Add(new InviteItem(e.Text));
				this.Session.AddHandler(invitesHandler);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_INVITELIST);

			invitesEndHandler = new IrcCodeHandler((e) =>
			{
				this.Session.RemoveHandler(noTopicHandler);
				this.Session.RemoveHandler(topicHandler);
				this.Session.RemoveHandler(topicSetByHandler);
				this.Session.RemoveHandler(modesHandler);
				this.Session.RemoveHandler(channelCreatedOnHandler);
				this.Session.RemoveHandler(bansHandler);
				this.Session.RemoveHandler(bansEndHandler);
				this.Session.RemoveHandler(exceptionsHandler);
				this.Session.RemoveHandler(exceptionsEndHandler);
				this.Session.RemoveHandler(invitesHandler);
				this.Session.RemoveHandler(invitesEndHandler);
				e.Handled = true;
				return true;
			}, IrcCode.RPL_ENDOFINVITELIST);

			this.Session.AddHandler(invitesHandler);
			this.Session.AddHandler(invitesEndHandler);
			this.Session.Invite();
		}

		private void SubscribeEvents()
		{
			this.Session.InfoReceived += new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.TopicChanged += new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
			this.Session.ChannelModeChanged += new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.Invited += new EventHandler<IrcInviteEventArgs>(Session_Invited);
		}

		private void UnsubscribeEvents()
		{
			this.Session.InfoReceived -= new EventHandler<IrcInfoEventArgs>(Session_InfoReceived);
			this.Session.TopicChanged -= new EventHandler<IrcTopicEventArgs>(Session_TopicChanged);
			this.Session.ChannelModeChanged -= new EventHandler<IrcChannelModeEventArgs>(Session_ChannelModeChanged);
			this.Session.Invited -= new EventHandler<IrcInviteEventArgs>(Session_Invited);
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.RPL_TOPIC:
					if (e.Message.Parameters.Count == 3 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						TopicStr = e.Message.Parameters[2];
					}
					return;
				case IrcCode.RPL_TOPICSETBY:
					if (e.Message.Parameters.Count == 4 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						//string TopicSetBy = e.Message.Parameters[2];
						//string TopicSetOn = ChatControl.FormatTime(e.Message.Parameters[3]);
					}
					return;
				case IrcCode.RPL_CHANNELCREATEDON:
					if (e.Message.Parameters.Count == 3 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						//string ChannelCreatedOn = ChatControl.FormatTime(e.Message.Parameters[2]);
					}
					return;
				case IrcCode.RPL_BANLIST:
					if (e.Message.Parameters.Count == 5 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						BanList.Add(new BanItem(e.Message.Parameters[2], e.Message.Parameters[3], UnixTimestamp.DateTimeFromTimestamp(int.Parse(e.Message.Parameters[4]))));
					}
					return;
				case IrcCode.RPL_ENDOFBANLIST:
					{
					}
					return;
				case IrcCode.RPL_EXCEPTLIST:
					if (e.Message.Parameters.Count == 5 && this.Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						ExceptsList.Add(new ExceptItem(e.Message.Parameters[2], e.Message.Parameters[3], UnixTimestamp.DateTimeFromTimestamp(int.Parse(e.Message.Parameters[4]))));
					}
					return;
				case IrcCode.RPL_ENDOFEXCEPTLIST:
					{
					}
					return;
				default:
					{
						e.Handled = true;
					}
					break;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			UnsubscribeEvents();
		}

		private void Session_TopicChanged(object sender, IrcTopicEventArgs e)
		{
			TopicStr = e.Text;
		}

		private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
		{
			ToggleModeButtons(e.Modes);

			IrcChannelMode modeBan = e.Modes.Where(m => m.Mode == 'b').FirstOrDefault();
			if (modeBan.Mode != Char.MinValue)
			{
				if (modeBan.Set == true)
				{
					BanItem NewBanItem = new BanItem(modeBan.Parameter, e.Who.Name, DateTime.Now);
					IsNewRowAdding = true;
					BanList.Add(NewBanItem);
				}
				if (modeBan.Set == false)
				{
					BanList.Remove(BanList.Where(b => b.Mask == modeBan.Parameter).FirstOrDefault());
				}
			}
			IrcChannelMode modeExcept = e.Modes.Where(m => m.Mode == 'e').FirstOrDefault();
			if (modeExcept.Mode != Char.MinValue)
			{
				if (modeExcept.Set == true)
				{
					ExceptItem NewExceptItem = new ExceptItem(modeExcept.Parameter, e.Who.Name, DateTime.Now);
					IsNewRowAdding = true;
					ExceptsList.Add(NewExceptItem);
				}
				if (modeExcept.Set == false)
				{
					ExceptsList.Remove(ExceptsList.Where(b => b.Mask == modeExcept.Parameter).FirstOrDefault());
				}
			}
		}

		private void Session_Invited(object sender, IrcInviteEventArgs e)
		{
			InvitesList.Add(new InviteItem(e.Channel, e.From.Name, DateTime.Now));
		}

		private void ExecuteAddBanMask(object sender, ExecutedRoutedEventArgs e)
		{
			DataGrid BanGrid = (DataGrid)sender;
			MaskItem NewItem = null;
			if (IsBanChanged == false && IsExceptChanged == false)
			{
				if (BanGrid.ItemsSource.GetType() == typeof(BanListType))
					IsBanChanged = true;
				if (BanGrid.ItemsSource.GetType() == typeof(ExceptListType))
					IsExceptChanged = true;
			}
			if (IsBanChanged)
			{
				BanItem NewBan = new BanItem("", "You", DateTime.Now);
				NewItem = NewBan;
				BanList.Add(NewBan);				
			}
			if (IsExceptChanged)
			{
				ExceptItem NewExcept = new ExceptItem("", "You", DateTime.Now);
				NewItem = NewExcept;
				ExceptsList.Add(NewExcept);
			}
			if (BanGrid != null && NewItem != null)
			{
				BanGrid.SelectedItem = NewItem;
				BanGrid.ScrollIntoView(NewItem);
				EnterEditMaskCell(BanGrid);
			}
			return;
		}

		private void ExecuteEditBanMask(object sender, ExecutedRoutedEventArgs e)
		{
			DataGrid BanGrid = (DataGrid)sender;
			EnterEditMaskCell(BanGrid);
		}

		private void ExecuteRemoveBanMask(object sender, ExecutedRoutedEventArgs e)
		{
			MessageBoxResult res = MessageBox.Show("Are you sure you want to remove?", "Confirmation!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (res == MessageBoxResult.Yes)
			{
				if (IsBanChanged)
					this.Session.Mode(this.Target.Name, "-b " + BanList[BanList.SelectedMaskIndex].Mask);
				if (IsExceptChanged)
					this.Session.Mode(this.Target.Name, "-e " + ExceptsList[ExceptsList.SelectedMaskIndex].Mask);
			}
			e.Handled = (res == MessageBoxResult.No);
			return;
		}

		public bool IsOp()
		{
			if (!this.ChatControl.IsChannel || !this.ChatControl.Nicknames.Contains(this.Session.Nickname))
			{
				return false;
			}
			var nick = this.ChatControl.Nicknames[this.Session.Nickname];
			return nick != null && (nick.Level & ChannelLevel.Op) > 0;
		}

		private void CanExecuteIsOp(object sender, CanExecuteRoutedEventArgs e)
		{
			bool enabled = IsOp();
			e.CanExecute = enabled;
			//ToggleButton TgBttn = e.Source as ToggleButton;
			//if (TgBttn != null && TgBttn.IsChecked == true && enabled == false)
			//	TgBttn.Background = Brushes.Bisque;
			if (LimTxtBx != null)
				LimTxtBx.IsEnabled = enabled;
			if (KeyTxtBx != null)
				KeyTxtBx.IsEnabled = enabled;
		}

		private void Mode_s_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_s_Toggled = false;
				this.Session.Mode(this.Target.Name, "+s");
			}
			else
			{
				Mode_s_Toggled = true;
				this.Session.Mode(this.Target.Name, "-s");
			}
			return;
		}

		private void Mode_p_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_p_Toggled = false;
				this.Session.Mode(this.Target.Name, "+p");
			}
			else
			{
				Mode_p_Toggled = true;
				this.Session.Mode(this.Target.Name, "-p");
			}
			return;
		}

		private void Mode_m_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_m_Toggled = false;
				this.Session.Mode(this.Target.Name, "+m");
			}
			else
			{
				Mode_m_Toggled = true;
				this.Session.Mode(this.Target.Name, "-m");
			}
			return;
		}

		private void Mode_t_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_t_Toggled = false;
				this.Session.Mode(this.Target.Name, "+t");
			}
			else
			{
				Mode_t_Toggled = true;
				this.Session.Mode(this.Target.Name, "-t");
			}
			return;
		}

		private void Mode_i_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_i_Toggled = false;
				this.Session.Mode(this.Target.Name, "+i");
			}
			else
			{
				Mode_i_Toggled = true;
				this.Session.Mode(this.Target.Name, "-i");
			}
			return;
		}

		private void Mode_n_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_n_Toggled = false;
				this.Session.Mode(this.Target.Name, "+n");
			}
			else
			{
				Mode_n_Toggled = true;
				this.Session.Mode(this.Target.Name, "-n");
			}
			return;
		}

		private void Mode_r_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_r_Toggled = false;
				this.Session.Mode(this.Target.Name, "+r");
			}
			else
			{
				Mode_r_Toggled = true;
				this.Session.Mode(this.Target.Name, "-r");
			}
			return;
		}

		private void Mode_D_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_D_Toggled = false;
				this.Session.Mode(this.Target.Name, "+D");
			}
			else
			{
				Mode_D_Toggled = true;
				this.Session.Mode(this.Target.Name, "-D");
			}
			return;
		}

		private void Mode_d_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			TgBttn.IsChecked = Mode_d_Toggled;
		}

		private void Mode_R_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				Mode_R_Toggled = false;
				this.Session.Mode(this.Target.Name, "+R");
			}
			else
			{
				Mode_R_Toggled = true;
				this.Session.Mode(this.Target.Name, "-R");
			}
			TgBttn.IsChecked = Mode_R_Toggled;
		}

		private void Mode_R_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = IsOp();
		}

		private void Mode_l_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				// Never happen case, LimitStr property changing sends the new limit
			}
			else
			{
				LimitStrNr = null;
			}
			return;
		}

		private void Mode_l_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (String.IsNullOrEmpty(LimitStrNr))
			{
				e.CanExecute = false;
				return;
			}
			e.CanExecute = IsOp();
		}

		private void Mode_k_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (TgBttn.IsChecked == true)
			{
				// Never happen case, KeyStr property changing sends the new key
			}
			else
			{
				KeyStr = null;
			}
			return;
		}

		private void Mode_k_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			ToggleButton TgBttn = e.Source as ToggleButton;
			if (String.IsNullOrEmpty(KeyStr))
			{
				e.CanExecute = false;
				return;
			}
			e.CanExecute = IsOp();
		}

		private void UpdateTextBoxSourceProperty(TextBox tBox)
		{
			DependencyProperty prop = TextBox.TextProperty;

			BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
			if (binding != null) { binding.UpdateSource(); }
		}

		private void TxtBx_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				UpdateTextBoxSourceProperty((TextBox)sender);
		}

		private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			if (!IsOp())
				e.Cancel = true;
		}

		private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			DataGrid BanGrid = (DataGrid)sender;
			TextBox t = e.EditingElement as TextBox;
			DataGridColumn dgc = e.Column;
			//if (dgc.Header.ToString() != "Mask")
			//	return;
			if (IsBanChanged)
			{
				BanItem newBan = e.Row.Item as BanItem;
				if (t.Text != newBan.Mask)
				{
					if (String.IsNullOrEmpty(newBan.Mask))
						BanList.Remove(newBan);
					else
					{
						BanList.Remove(newBan);
						this.Session.Mode(this.Target.Name, "-b " + newBan.Mask);
					}
					this.Session.Mode(this.Target.Name, "+b " + t.Text);
				}
			}
			if (IsExceptChanged)
			{
				ExceptItem newExcept = e.Row.Item as ExceptItem;
				if (t.Text != newExcept.Mask)
				{
					if (String.IsNullOrEmpty(newExcept.Mask))
						ExceptsList.Remove(newExcept);
					else
					{
						ExceptsList.Remove(newExcept);
						this.Session.Mode(this.Target.Name, "-e " + newExcept.Mask);
					}
					this.Session.Mode(this.Target.Name, "+e " + t.Text);
				}
			}
		}

		private void ExecuteJoin(object sender, ExecutedRoutedEventArgs e)
		{
			string channel = e.Parameter as string;
			if (!string.IsNullOrEmpty(channel))
			{
				this.Session.Join(channel);
				InvitesList.Remove(InvitesList.Where(i => i.InvitedTo == channel).FirstOrDefault());
			}
		}

		private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			if (IsNewRowAdding)
			{
				e.Row.Focus();
				DataGrid BanGrid = (DataGrid)sender;
				BanGrid.SelectedIndex = BanGrid.Items.Count - 1;
				BanGrid.ScrollIntoView(BanGrid.SelectedItem);
				IsNewRowAdding = false;
			}
		}

		private void CanModifyBans(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = IsOp();
		}

		public void ToggleModeButtons(ICollection<IrcChannelMode> Modes)
		{
			foreach (IrcChannelMode m in Modes)
			{
				if (m.Mode == 's') Mode_s_Toggled = m.Set;
				if (m.Mode == 'p') Mode_p_Toggled = m.Set;
				if (m.Mode == 'm') Mode_m_Toggled = m.Set;
				if (m.Mode == 't') Mode_t_Toggled = m.Set;
				if (m.Mode == 'i') Mode_i_Toggled = m.Set;
				if (m.Mode == 'n') Mode_n_Toggled = m.Set;
				if (m.Mode == 'r') Mode_r_Toggled = m.Set;
				if (m.Mode == 'D') Mode_D_Toggled = m.Set;
				if (m.Mode == 'd') Mode_d_Toggled = m.Set;
				if (m.Mode == 'R') Mode_R_Toggled = m.Set;
				if (m.Mode == 'l')
				{
					Mode_l_Toggled = m.Set;
					if (m.Set == true)
						LimitStrNr = m.Parameter;
					if (m.Set == false)
						LimitStrNr = null;
				}
				if (m.Mode == 'k')
				{
					if (!bSkipKeySet)
					{
						Mode_k_Toggled = m.Set;
						if (m.Set == true)
							KeyStr = m.Parameter;
						if (m.Set == false)
							KeyStr = null;
					}
					else
						bSkipKeySet = false;
				}
			}
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DataGrid BanGrid = (DataGrid)sender;
			if (BanGrid != null)
			{
				// It works
			}
		}

		private void EnterEditMaskCell(DataGrid dg)
		{
			if (dg != null)
			{
				foreach (DataGridCellInfo selCell in dg.SelectedCells)
				{
					if (selCell.Column.Header.ToString() == "Mask" && IsOp())
					{
						dg.CurrentCell = selCell;
						dg.BeginEdit();
						break;
					}
				}
			}
		}
	}
}
