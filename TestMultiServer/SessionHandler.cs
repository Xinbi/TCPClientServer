using System;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Packets;
using Newtonsoft.Json;

namespace GenericTcpServer
{
	public class SessionHandler : ISessionHandler
	{
		private readonly TcpClient _client;
		private bool _running;

		/// <summary>
		/// Initializes a new instance of the <see cref="SessionHandler"/> class.
		/// </summary>
		/// <param name="inClientSocket"></param>
		/// <param name="sessionNumber"></param>
		public SessionHandler(TcpClient inClientSocket, string sessionNumber)
		{
			SessionNumber = sessionNumber;
			this._client = inClientSocket;
		}

		public string SessionNumber { get; }
		public event PacketEventHandler PacketSentEvent;
		public event PacketEventHandler PacketReceivedEvent;
		public event SessionEndedEventHandler SessionEndedEvent;
		public event SessionStartedEventHandler SessionStartedEvent;
		public void Start()
		{
			Run().GetAwaiter().GetResult();
		}

		public void Stop()
		{
			_running = false;
			_client.Client.Close(5000);
			_client.Close();
		}

		public async Task Send(IPacket message)
		{
			try
			{
				message.SessionId = SessionNumber;
				// convert JSON to buffer and its length to a 16 bit unsigned integer buffer
				byte[] jsonBuffer = Encoding.UTF8.GetBytes(message.ToJson());
				byte[] lengthBuffer = BitConverter.GetBytes(Convert.ToUInt16(jsonBuffer.Length));

				// Join the buffers
				byte[] msgBuffer = new byte[lengthBuffer.Length + jsonBuffer.Length];
				lengthBuffer.CopyTo(msgBuffer, 0);
				jsonBuffer.CopyTo(msgBuffer, lengthBuffer.Length);

				// Send the packet
				await _client.GetStream().WriteAsync(msgBuffer, 0, msgBuffer.Length);
				OnMessageSentEvent(message);
			}
			catch 
			{
				// ignored
			}
		}
		public async Task<IPacket> ReceivePacket(TcpClient client)
		{
			IPacket packet = null;
			try
			{
				// First check there is data available
				if (client.Available == 0)
					return null;

				NetworkStream msgStream = client.GetStream();

				// There must be some incoming data, the first two bytes are the size of the Packet
				byte[] lengthBuffer = new byte[2];
				await msgStream.ReadAsync(lengthBuffer, 0, 2);
				ushort packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);

				// Now read that many bytes from what's left in the stream, it must be the Packet
				byte[] jsonBuffer = new byte[packetByteSize];
				await msgStream.ReadAsync(jsonBuffer, 0, jsonBuffer.Length);

				// Convert it into a packet datatype
				string jsonString = Encoding.UTF8.GetString(jsonBuffer);
				packet = JsonConvert.DeserializeObject<Packet>(jsonString);

			}
			catch (Exception e)
			{
				// There was an issue in receiving
				Console.WriteLine("There was an issue sending a packet to {0}.", client.Client.RemoteEndPoint);
				Console.WriteLine("Reason: {0}", e.Message);
			}

			return packet;
		}

		private async Task Run()
		{
			OnSessionStartedEvent();
			_running = true;
			try
			{
				while (_running)
				{
					if (!_running )
					{
						break;
					}

					Thread.Sleep(10);
					var packet = await ReceivePacket(_client);
					if (packet == null)
						continue;
					if (packet.SessionId != SessionNumber)
					{
						_running = false;
						continue;
					}
					OnMessageReceivedEvent(packet);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Run error:" + ex.Message);
			}
			finally
			{
				OnSessionEndedEvent("Connection Closed.");
			}
		}
		
		protected virtual void OnSessionStartedEvent()
		{
			var handler = SessionStartedEvent;
			handler?.Invoke(this, new SessionStartedEventArgs() { SessionId = this.SessionNumber });
		}

		protected virtual void OnSessionEndedEvent(string reason)
		{
			var handler = SessionEndedEvent;
			handler?.Invoke(this, new SessionEndedEventArgs() { Reason = reason, SessionId = this.SessionNumber });
		}

		protected virtual void OnMessageSentEvent(IPacket msg)
		{
			var handler = PacketSentEvent;
			handler?.Invoke(this, new MessageEventArgs() { Message = msg, SessionId = this.SessionNumber });
		}

		protected virtual void OnMessageReceivedEvent(IPacket msg)
		{
			var handler = PacketReceivedEvent;
			handler?.Invoke(this, new MessageEventArgs() { Message = msg, SessionId = this.SessionNumber });
		}

	    public async Task<ResponsePacket> Ping()
	    {
	        return await Ping(5000);
	    }

	    public async Task<ResponsePacket> Ping(long timeout)
	    {
	        var connected = false;
	        if (!_isConnected(_client))
	        {
	            return new ResponsePacket() {Data =  Result.Disconnected};
	        }

	        PacketEventHandler handler = (sender, e) =>
	        {
	            if (e.Message.Command == "pong")
	            {
	                connected = true;
                }
            };

	        this.PacketReceivedEvent += handler;
	        await Send(new PingPacket());
            var timeoutWatch = Stopwatch.StartNew();

	        SpinWait.SpinUntil(()=> connected || timeoutWatch.ElapsedMilliseconds >= timeout);
	        this.PacketReceivedEvent -= handler;
	        return !connected ? new ResponsePacket() { Data = Result.Disconnected } : new ResponsePacket() { Data = Result.Success };
	    }


	    // Checks if a client has disconnected ungracefully
		// Adapted from: http://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket
		private static bool _isConnected(TcpClient client)
		{
			try
			{
				Socket s = client.Client;
				bool newVariable = s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
				return !newVariable;
			}
			catch (SocketException)
			{
				// We got a socket error, assume it's disconnected
				return false;
			}
		}

	}

	public delegate void SessionEndedEventHandler(object sender, SessionEndedEventArgs e);
	public delegate void SessionStartedEventHandler(object sender, SessionStartedEventArgs e);

	public class SessionStartedEventArgs : EventArgs
	{
		public string SessionId { get; set; }
	}

	public class SessionEndedEventArgs : EventArgs
	{
		public string SessionId { get; set; }
		public string Reason { get; set; }
	}
}

