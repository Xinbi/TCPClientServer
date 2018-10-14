using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Game.Common;

namespace TestMultiServer
{
	public interface IServer
	{
		IPAddress IpAddress { get; }
		int Port { get; }
		int ActiveSessions { get; }
		void Start();
		void Stop();
		IPacket GetStatus();
	}
}
