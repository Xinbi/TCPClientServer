using System;
using System.Threading;

namespace TestClient
{
	public class Program
	{
		public static TcpGamesClient gamesClient;

		public static void InterruptHandler(object sender, ConsoleCancelEventArgs args)
		{
			// Perform a graceful disconnect
			args.Cancel = true;
			gamesClient?.Disconnect();
		}

		public static void Main(string[] args)
		{
			// Setup the Games Client
			string host = "192.168.1.29"; //args[0].Trim();
			int port = 8888; //int.Parse(args[1].Trim());
			gamesClient = new TcpGamesClient(host, port);

			// Add a handler for a Ctrl-C press
			Console.CancelKeyPress += InterruptHandler;

			// Try to connecct & interact with the server
			gamesClient.Connect();
			gamesClient.Run();
			Thread.Sleep(2000);
		}
	}
}
