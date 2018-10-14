using System;
using System.Collections.Generic;

namespace TestMultiServer
{
	public class Client
	{
		public string ID { get; set; }
		public Client()
		{
			ID = Guid.NewGuid().ToString("N");
		}
	}
}
