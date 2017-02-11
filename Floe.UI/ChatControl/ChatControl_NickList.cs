using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
    public partial class ChatControl : ChatPage
    {
        private NicknameList _nickList;
        private NicknameComplete _nicknameComplete;

        public NicknameList Nicknames
        {
            get { return _nickList; }
        }
        
        private string GetNickWithLevel(string nick)
        {
            return this.IsChannel && _nickList.Contains(nick) ? _nickList[nick].ToString() : nick;
        }

        private string GetNickWithoutLevel(string nick)
        {
            return (nick.Length > 1 && (nick[0] == '@' || nick[0] == '+' || nick[0] == '%')) ? nick.Substring(1) : nick;
        }

        private void DoNickCompletion()
        {
            if (txtInput.CaretIndex == 0)
            {
                return;
            }

			if (_nicknameComplete == null)
			{
				if (this.IsNickname)
				{
					NicknameList pnickList = new NicknameList();
					pnickList.Add(this.Session.Nickname);
					pnickList.Add(this.Target.Name);
					_nicknameComplete = new NicknameComplete(txtInput, pnickList);
				}
				else
					_nicknameComplete = new NicknameComplete(txtInput, _nickList);
			}

            _nicknameComplete.getNextNick();
        }
    }
}
