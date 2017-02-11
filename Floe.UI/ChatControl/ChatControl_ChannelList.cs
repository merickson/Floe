using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private string[] _chanList;
		private ChanNameComplete _channameComplete;

		private void DoChannelCompletion()
		{
			if (txtInput.CaretIndex == 0)
			{
				return;
			}
			if (_channameComplete == null)
			{
				_chanList = ((ChatWindow)_window).Items.Where((item) => item.IsVisible && item.Page.Session == this.Session && item.Page.Target != null && item.Page.Target.IsChannel).Select(p => p.Page.Target.Name).ToArray();
				_channameComplete = new ChanNameComplete(txtInput, _chanList);
			}

			_channameComplete.getNextChan();
		}
	}
}
