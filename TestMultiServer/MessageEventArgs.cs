using System;
using Common;

namespace GenericTcpServer
{
	public class MessageEventArgs : EventArgs
	{
		public string SessionID { get; set; }
		public IPacket Message { get; set; }
	}
}
