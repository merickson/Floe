﻿using System;
using System.ComponentModel;
using System.Configuration;

namespace Floe.Configuration
{
	public class FormattingElement : ConfigurationElement, INotifyPropertyChanged
	{
		[ConfigurationProperty("fontFamily", DefaultValue = "Consolas")]
		public string FontFamily
		{
			get { return (string)this["fontFamily"]; }
			set { this["fontFamily"] = value; this.OnPropertyChanged("FontFamily"); }
		}

		[ConfigurationProperty("fontSize", DefaultValue = 14.0)]
		public double FontSize
		{
			get { return (double)this["fontSize"]; }
			set
			{
				if (value < 0.1 || value > 200.0)
				{
					throw new ArgumentException("Font size is not valid.");
				}
				this["fontSize"] = value;
				this.OnPropertyChanged("FontSize");
			}
		}

		[ConfigurationProperty("fontStyle", DefaultValue="Normal")]
		public string FontStyle
		{
			get { return (string)this["fontStyle"]; }
			set { this["fontStyle"] = value; this.OnPropertyChanged("FontStyle"); }
		}

		[ConfigurationProperty("fontWeight", DefaultValue="Black")]
		public string FontWeight
		{
			get { return (string)this["fontWeight"]; }
			set { this["fontWeight"] = value; this.OnPropertyChanged("FontWeight"); }
		}

		[ConfigurationProperty("showTimestamp", DefaultValue=true)]
		public bool ShowTimestamp
		{
			get { return (bool)this["showTimestamp"]; }
			set { this["showTimestamp"] = value; this.OnPropertyChanged("ShowTimestamp"); }
		}

		[ConfigurationProperty("timestampFormat", DefaultValue = "[HH:mm]")]
		public string TimestampFormat
		{
			get { return (string)this["timestampFormat"]; }
			set
			{
				DateTime.Now.ToString(value);
				if (value.Trim().Length < 1)
				{
					throw new ArgumentException("String cannot be empty.");
				}
				this["timestampFormat"] = value;
				this.OnPropertyChanged("TimestampFormat");
			}
		}

		[ConfigurationProperty("useTabularView", DefaultValue=false)]
		public bool UseTabularView
		{
			get { return (bool)this["useTabularView"]; }
			set { this["useTabularView"] = value; this.OnPropertyChanged("UseTabularView"); }
		}

		[ConfigurationProperty("colorizeNicknames", DefaultValue = false)]
		public bool ColorizeNicknames
		{
			get { return (bool)this["colorizeNicknames"]; }
			set { this["colorizeNicknames"] = value; this.OnPropertyChanged("ColorizeNicknames"); }
		}

		[ConfigurationProperty("nicknameColorSeed", DefaultValue = 0)]
		public int NicknameColorSeed
		{
			get { return (int)this["nicknameColorSeed"]; }
			set { this["nicknameColorSeed"] = value; this.OnPropertyChanged("NicknameColorSeed"); }
		}

		[ConfigurationProperty("attentionOnOwnNickname", DefaultValue = true)]
		public bool AttentionOnOwnNickname
		{
			get { return (bool)this["attentionOnOwnNickname"]; }
			set { this["attentionOnOwnNickname"] = value; this.OnPropertyChanged("AttentionOnOwnNickname"); }
		}

		[ConfigurationProperty("overlayIconOnChatActivity", DefaultValue = false)]
		public bool OverlayIconOnChatActivity
		{
			get { return (bool)this["overlayIconOnChatActivity"]; }
			set { this["overlayIconOnChatActivity"] = value; this.OnPropertyChanged("OverlayIconOnChatActivity"); }
		}

		[ConfigurationProperty("overlayIconChangeColor", DefaultValue = false)]
		public bool OverlayIconChangeColor
		{
			get { return (bool)this["overlayIconChangeColor"]; }
			set { this["overlayIconChangeColor"] = value; this.OnPropertyChanged("OverlayIconChangeColor"); }
		}

		[ConfigurationProperty("attentionPatterns", DefaultValue = "")]
		public string AttentionPatterns
		{
			get { return (string)this["attentionPatterns"]; }
			set { this["attentionPatterns"] = value; this.OnPropertyChanged("AttentionPatterns"); }
		}

		[ConfigurationProperty("autoSizeColumn", DefaultValue = true)]
		public bool AutoSizeColumn
		{
			get { return (bool)this["autoSizeColumn"]; }
			set { this["autoSizeColumn"] = value; this.OnPropertyChanged("AutoSizeColumn"); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
