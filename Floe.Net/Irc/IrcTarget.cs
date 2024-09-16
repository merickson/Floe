﻿using System;

namespace Floe.Net
{
	/// <summary>
	/// Represents a target to which a message may be sent.
	/// </summary>
	public sealed class IrcTarget
	{
		/// <summary>
		/// Gets the IrcPrefix of this IrcTarget was constructed from.
		/// </summary>
		public IrcPrefix Prefix { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the target is a channel. If this is false, the target is a user.
		/// </summary>
		public bool IsChannel { get; private set; }

		/// <summary>
		/// Gets the name of the target. If the target is a channel, this is the channel name including leading symbols.
		/// If the target is a user, this is their nickname.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Construct an IrcTarget and automatically determine the type of the target.
		/// </summary>
		/// <param name="name">A channel name or a nickname.</param>
		public IrcTarget(string name)
		{
			this.IsChannel = IsChannelName(name);
			this.Name = name;
		}

		/// <summary>
		/// Construct an IrcTarget.
		/// </summary>
		/// <param name="isChannel">Indicates whether the target is a channel (true) or a user (false).</param>
		/// <param name="name">The name of the channel or user.</param>
		public IrcTarget(bool isChannel, string name)
		{
			this.IsChannel = isChannel;
			this.Name = name;
		}

		/// <summary>
		/// Construct an IrcTarget from an IrcPeer. This is useful when replying to a received message.
		/// </summary>
		/// <param name="peer"></param>
		public IrcTarget(IrcPeer peer)
		{
			this.Prefix = peer;
			this.IsChannel = false;
			this.Name = peer.Nickname;
		}

		/// <summary>
		/// Construct an IrcTarget from an IrcServer. This is useful when replying to a received message.
		/// </summary>
		/// <param name="peer"></param>
		public IrcTarget(IrcServer peer)
		{
			this.Prefix = peer;
			this.IsChannel = false;
			this.Name = peer.ServerName;
		}

		/// <summary>
		/// Construct an IrcTarget from an IrcPrefix. This is useful when replying to a received message.
		/// </summary>
		/// <param name="peer"></param>
		public IrcTarget(IrcPrefix peer)
		{
			this.Prefix = peer;
			this.IsChannel = false;
			if (peer is IrcPeer)
			{
				this.Name = new IrcPeer(peer.Prefix.ToString()).Nickname;
			}
			if (peer is IrcServer)
			{
				this.Name = new IrcServer(peer.Prefix.ToString()).ServerName;
			}
		}

		/// <summary>
		/// Gets the name of the target.
		/// </summary>
		/// <returns>Returns the channel name or user name of the target.</returns>
		public override string ToString()
		{
			return this.Name;
		}

		/// <summary>
		/// Determine whether the specified string refers to a channel or a user.
		/// </summary>
		/// <param name="name">The channel or user name to inspect.</param>
		/// <returns>Returns true if the name refers to a channel, false if it refers to a user.</returns>
		public static bool IsChannelName(string name)
		{
			if (name.Length < 1)
				return false;
			if (name.StartsWith("@"))
				name = name.Substring(1);
			if (name.Length < 1)
				return false;
			return name[0] == '#' || name[0] == '+' || name[0] == '&' || name[0] == '!';
		}

		/// <summary>
		/// Determine whether another object is an IrcTarget referring to the same entity.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>Returns true if the other object is an IrcTarget referring to the same entity..</returns>
		public override bool Equals(object obj)
		{
			var other = obj as IrcTarget;
			string otherName = String.Empty;
			if (other != null)
			{
				otherName = other.Name;
				if (otherName.StartsWith("@"))
					otherName = otherName.Substring(1);
			}
			string thisName = this.Name;
			if (thisName.StartsWith("@"))
				thisName = thisName.Substring(1);

			return other != null && other.IsChannel == this.IsChannel &&
				string.Compare(otherName, thisName, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Compute a suitable hashcode.
		/// </summary>
		/// <returns>Returns a hashcode for the object.</returns>
		public override int GetHashCode()
		{
			return this.Name.GetHashCode() + (this.IsChannel ? 1 : 0);
		}
	}
}
