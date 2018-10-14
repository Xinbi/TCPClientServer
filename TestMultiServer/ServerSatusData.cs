using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Newtonsoft.Json;

namespace TestMultiServer
{
	public class ServerSatusData : IPacketData
	{
		public int Port { get; set; }
		public int ActiveConnections { get; set; }
		public string Status { get; set; }
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
