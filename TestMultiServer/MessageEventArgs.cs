using System;
using Common.Packets;

namespace GenericTcpServer
{
	public class MessageEventArgs : EventArgs
	{
		public string SessionId { get; set; }
		public IPacket Message { get; set; }
	}
}
