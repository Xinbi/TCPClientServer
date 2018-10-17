using System.Net;
using Common;
using Common.Packets;

namespace GenericTcpServer
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
