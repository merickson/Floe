using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	class ChanNameComplete
	{
		private string[] _chanCandidates;
		private uint _tabCount;
		private string _incompleteChan;
		private int _incompleteChanStart;
		private TextBox _txtInput;
		private bool _chanAtBeginning;

		public ChanNameComplete(TextBox txtInput, string[] chanCandidates)
		{
			_tabCount = 0;
			_txtInput = txtInput;
			_incompleteChan = initialInputParse();
			if (IrcTarget.IsChannelName(_incompleteChan))
			{
				_chanCandidates = (from n in chanCandidates
								   where n.StartsWith(_incompleteChan, StringComparison.InvariantCultureIgnoreCase)
								   orderby n.ToLowerInvariant()
								   select n).ToArray();
			}
		}
		public void getNextChan()
		{
			_tabCount++;
			if (_chanCandidates == null || _chanCandidates.Length <= 0)
			{
				return;
			}

			if (_tabCount > _chanCandidates.Length)
			{
				_tabCount = 1;
			}

			string completeChan = _chanCandidates[_tabCount - 1];

			if (_chanAtBeginning)
			{
				completeChan += ": ";
			}
			else
			{
				completeChan += " ";
			}

			_txtInput.Text = _txtInput.Text.Substring(0, _incompleteChanStart) + completeChan;
			_txtInput.CaretIndex = _txtInput.Text.Length;
			return;
		}

		private string initialInputParse()
		{
			int i = _txtInput.CaretIndex - 1;
			char c = _txtInput.Text[i];
			while (c != ' ' && i > 0)
			{
				c = _txtInput.Text[--i];
			}
			if (i == 0)
			{
				_chanAtBeginning = true;
				_incompleteChanStart = 0;
			}
			else if (_txtInput.Text[i - 1] == ':')
			{
				_incompleteChanStart = i + 1;
				_chanAtBeginning = true;
			}
			else
			{
				_incompleteChanStart = i + 1;
				_chanAtBeginning = false;
			}
			return _txtInput.Text.Substring(_incompleteChanStart, _txtInput.CaretIndex - _incompleteChanStart);
		}

	}
}
