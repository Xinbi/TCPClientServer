using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Newtonsoft.Json;

namespace GenericTcpServer
{
	public class ServerStatusData 
	{
        public string IpAddress { get; set; }
		public int Port { get; set; }
		public int ActiveConnections { get; set; }
		public string Status { get; set; }

	}
}
