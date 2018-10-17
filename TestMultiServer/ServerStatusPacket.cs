using Common;
using Common.Packets;

namespace GenericTcpServer
{

	public sealed class ServerStatusPacket : JsonMessage, IPacket<ServerStatusData>
	{
		public ServerStatusPacket(ServerStatusData statusData)
		{
			this.Command = "serverStatus";
			Data = statusData;
		}

		public string SessionId { get; set; }
		public string Command { get; set; }

		public ServerStatusData Data { get; set; }
	}
}
