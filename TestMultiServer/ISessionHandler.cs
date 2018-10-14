using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Game.Common;

namespace TestMultiServer
{
	public interface ISessionHandler
	{
		string SessionNumber { get; }
		event PacketEventHandler PacketReceivedEvent;
		event SessionEndedEventHandler SessionEndedEvent;
		event SessionStartedEventHandler SessionStartedEvent;
		void Start();
		void Stop();
		Task Send(IPacket message);
		Task<IPacket> ReceivePacket(TcpClient client);
		Task Ping();
	}
}
