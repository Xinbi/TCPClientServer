using System;
using System.Collections.Generic;
using System.Threading;

namespace TestMultiServer
{
	class Program
	{
		static void Main(string[] args)
		{
			Server server = new Server();
			Thread serverThread = new Thread(server.Start);
			serverThread.Start();

			string command = Console.ReadLine();
			while (command != "stop")
			{
				switch (command)
				{
					case "status":
						Console.WriteLine($" >> {server.GetStatus().ToJson()}");
						break;
				}

				command = Console.ReadLine();
			}
			server.Stop();
			
			Thread.Sleep(1000);
		}
	}
}
