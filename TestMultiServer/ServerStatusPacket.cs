using Common;

namespace GenericTcpServer
{

	public sealed class ServerStatusPacket : JsonMessage, IPacket
	{
		public ServerStatusPacket(ServerSatusData statusData)
		{
			this.Command = "serverStatus";
			Data = statusData;
		}

		public string SessionId { get; set; }
		public string Command { get; set; }
		IPacketData IPacket.Data
		{
			get { return this.Data;}
			set { this.Data = (ServerSatusData)value; }
		}
		public ServerSatusData Data { get; set; }
	}
}
