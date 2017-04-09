using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
    class NicknameComplete
    {
        private string[] _nickCandidates;
        private uint _tabCount;
        private string _incompleteNick;
        private int _incompleteNickStart;
        private TextBox _txtInput;
        private bool _nickAtBeginning;

        public NicknameComplete(TextBox txtInput, NicknameList nickList)
        {
            _tabCount = 0;
            _txtInput = txtInput;
            _incompleteNick = initialInputParse();
			if (!IrcTarget.IsChannelName(_incompleteNick))
			{
				_nickCandidates = (from n in nickList
								   where n.Nickname.StartsWith(_incompleteNick, StringComparison.InvariantCultureIgnoreCase)
								   orderby n.Nickname.ToLowerInvariant()
								   select n.Nickname).ToArray();
			}
        }

        public void getNextNick()
        {
            _tabCount++;
            if (_nickCandidates == null || _nickCandidates.Length <= 0)
            {
                return;
            }

            if (_tabCount > _nickCandidates.Length)
            {
                _tabCount = 1;
            }

            string completeNick = _nickCandidates[_tabCount - 1];

            if(_nickAtBeginning)
            {
                completeNick += ": ";
            }
            else
            {
                completeNick += " ";
            }

            _txtInput.Text = _txtInput.Text.Substring(0, _incompleteNickStart) + completeNick;
            _txtInput.CaretIndex = _txtInput.Text.Length;
            return;
        }

        private string initialInputParse()
        {
            int i = _txtInput.CaretIndex - 1;
            char c = _txtInput.Text[i];
            while ( c != ' ' && i > 0)
            {
                c = _txtInput.Text[--i];
                if ((c == '=') && (_txtInput.Text[i - 1] == ' '))
                {
                    break;
                }
            }
            if (i == 0)
            {
                _nickAtBeginning = true;
                _incompleteNickStart = 0;
            }
            else if (_txtInput.Text[i-1] == ':')
            {
                _incompleteNickStart = i + 1;
                _nickAtBeginning = true;
            }
            else
            {
                _incompleteNickStart = i+1;
                _nickAtBeginning = false;
            }
            return _txtInput.Text.Substring(_incompleteNickStart, _txtInput.CaretIndex - _incompleteNickStart);
        }
    }
}
