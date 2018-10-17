using System;

namespace Common.Packets
{
	public sealed class MessagePacket : Packet<MessageData>
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

	}
}