using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Packets;

namespace GenericTcpServer
{
	public class Server : IServer
	{
		private Dictionary<string, ISessionHandler> _sessionHandlers { get; }
		public IPAddress IpAddress { get; }
		
		public int Port => 8888;
		public int ActiveSessions => _sessionHandlers.Count;
		private static readonly object _lockObj = new object();
		private bool _running = false;
		private readonly TcpListener _serverSocket;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Server"/> class.
		/// </summary>
		public Server()
		{
			IpAddress = GetLocalIpAddress();
			_sessionHandlers = new Dictionary<string, ISessionHandler>();
			_serverSocket = new TcpListener(IPAddress.Any, Port);
		}
		
		public void Start()
		{
			_serverSocket.Start();
			_running = true;
			Console.WriteLine(" >> " + "Server Started");
			new Thread(this.PingAllSessions).Start();
			while (_running)
			{
				try
				{
					if (!_serverSocket.Pending())
					{
						Thread.Sleep(100);
					}
					else
					{
                        _handleNewConnection().GetAwaiter().GetResult();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Server Stopped." + ex.Message);
					_running = false;
				}
			}
		}

		public void Stop()
		{
			_running = false;
			var sessionList = _sessionHandlers.Keys.ToArray();
			Parallel.ForEach(sessionList, sessionId =>
			{
				try
				{
					_sessionHandlers[sessionId].Stop();
				}
				catch
				{
					// ignored
				}
			});
			_serverSocket.Stop();
		}

		public IPacket GetStatus()
		{
			return new ServerStatusPacket(new ServerStatusData()
			{
                IpAddress =  this.IpAddress.ToString(),
				ActiveConnections = this.ActiveSessions,
				Port = this.Port,
				Status = "Running."
			});
		}

		private void PingAllSessions()
		{
			while (_running)
			{
				lock (_lockObj)
				{
					if (_sessionHandlers.Count <= 0)
						continue;
					List<ISessionHandler> handlers = _sessionHandlers.Values.ToList();
					foreach (var handler in handlers)
					{
					    var result = handler.Ping().GetAwaiter().GetResult();
					    if (result.Data.Equals(Result.Disconnected))
					    {
					        handler.Stop();
					    }
					}
				}
				Thread.Sleep(1000);
			}
		}

		private void OnSessionEnd(object sender, SessionEndedEventArgs e)
		{
			Console.WriteLine($" >> Session ended ({e.SessionId}) - {e.Reason}");
			_sessionHandlers[e.SessionId].Stop();
			_sessionHandlers.Remove(e.SessionId);
			Console.WriteLine($" >> {_sessionHandlers.Count} active sessions.");
		}

		private void OnSessionStart(object sender, SessionStartedEventArgs e)
		{
			Console.WriteLine($" >> Session started ({e.SessionId}) : {_sessionHandlers.Count} active sessions.");
		}

		// Awaits for a new connection and then adds them to the waiting lobby
		private async Task _handleNewConnection()
		{
			try
			{
				string sessionId = Guid.NewGuid().ToString("N");
				TcpClient newClient = await _serverSocket.AcceptTcpClientAsync();
				var handler = new SessionHandler(newClient, sessionId);
				handler.PacketReceivedEvent += (sender, e) => Console.WriteLine($"Received:{e.Message.ToJson()}");
				handler.PacketSentEvent += (sender, e) => Console.WriteLine($"Sent:{e.Message.ToJson()}");
				handler.SessionEndedEvent += OnSessionEnd;
				handler.SessionStartedEvent += OnSessionStart;
				_sessionHandlers.Add(handler.SessionNumber, handler);
				Console.WriteLine("New connection from {0}.", newClient.Client.RemoteEndPoint);
				handler.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}
		
		private static IPAddress GetLocalIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip;
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
		
	}
}

