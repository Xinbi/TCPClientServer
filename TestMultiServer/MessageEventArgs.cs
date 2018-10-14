using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Common;

namespace TestMultiServer
{
	public class MessageEventArgs : EventArgs
	{
		public string SessionID { get; set; }
		public IPacket Message { get; set; }
	}
}
