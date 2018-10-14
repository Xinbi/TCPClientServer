using Newtonsoft.Json;
using System;
using System.Net.Sockets;

namespace Game.Common
{

	public class Packet : JsonMessage, IPacket
	{
		public string SessionId { get; set; }
		public string Command { get; set; }
		public PacketData Data { get; set; }

		IPacketData IPacket.Data
		{
			get { return this.Data; }

			set { this.Data = (PacketData) value; }
		}
	}

	public class JsonMessage : IJsonSerializable
	{
		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}

	}
	
}

