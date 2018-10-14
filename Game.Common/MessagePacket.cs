using System;
using Newtonsoft.Json;

namespace Game.Common
{
	public sealed class MessagePacket : JsonMessage, IPacket
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePacket"/> class.
		/// </summary>
		public MessagePacket()
		{
			Command = String.Empty;
			Data = null;
		}

		public MessagePacket(string msg)
		{
			Command = "message";
			Data = new MessageData() { Message = msg};
		}

		public string SessionId { get; set; }
		public string Command { get; set; }
		IPacketData IPacket.Data
		{
			get { return this.Data;}
			set { this.Data = (MessageData) value; }
		}

		public MessageData Data { get; set; }
	}
}