using System;
using System.Threading;

namespace TestClient
{
	public class Program
	{
		public static GenericTcpClient Client;

		public static void InterruptHandler(object sender, ConsoleCancelEventArgs args)
		{
			// Perform a graceful disconnect
			args.Cancel = true;
			Client?.Disconnect();
		}

		public static void Main(string[] args)
		{
			// Setup the Games Client
			string host = "53.220.84.72"; //args[0].Trim();
			int port = 8888; //int.Parse(args[1].Trim());
			Client = new GenericTcpClient(host, port);

			// Add a handler for a Ctrl-C press
			Console.CancelKeyPress += InterruptHandler;

			// Try to connecct & interact with the server
			Client.Connect();
			Client.Run();
			Thread.Sleep(2000);
		}
	}
}
