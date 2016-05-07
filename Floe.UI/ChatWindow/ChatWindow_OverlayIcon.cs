using System;
using System.Windows;
using System.Windows.Media;

namespace Floe.UI
{
    public enum OverlayIconState
    {
        None,
        ChatActivity,
        OwnNickname,
        PrivateMessage
    }

    public partial class ChatWindow : Window
    {

        private OverlayIconState state = OverlayIconState.None;

        public void clearOverlayIcon()
        {
            if (state != OverlayIconState.None)
            {
                state = OverlayIconState.None;
                TaskbarItemInfo.Overlay = null;
            }
        }

        public void setOverlayIcon(OverlayIconState newState)
        {
            if (newState == OverlayIconState.None && this.state != OverlayIconState.None)
            {
                state = OverlayIconState.None;
                TaskbarItemInfo.Overlay = null;
                return;
            }

            if (App.Settings.Current.Formatting.OverlayIconOnChatActivity && (newState > state))
            {
                if (!App.Settings.Current.Formatting.OverlayIconChangeColor || (newState == OverlayIconState.ChatActivity))
                {
                    state = newState;
                    TaskbarItemInfo.Overlay = (ImageSource)Resources["GreenBubble"];
                }
                else if (App.Settings.Current.Formatting.OverlayIconChangeColor)
                {
                    if (newState == OverlayIconState.OwnNickname)
                    {
                        state = OverlayIconState.OwnNickname;
                        TaskbarItemInfo.Overlay = (ImageSource)Resources["TealBubble"];
                    }
                    else if (newState == OverlayIconState.PrivateMessage)
                    {
                        state = OverlayIconState.PrivateMessage;
                        TaskbarItemInfo.Overlay = (ImageSource)Resources["RedBubble"];
                    }
                }
            }
        }
    }
}
