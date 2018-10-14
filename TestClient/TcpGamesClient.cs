using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.Common;
using Newtonsoft.Json;

public class TcpGamesClient
{
	// Conneciton objects
	public readonly string ServerAddress;
	public readonly int Port;
	public bool Running { get; private set; }
	private TcpClient _client;
	private bool _clientRequestedDisconnect = false;
	private string _sessionId;

	// Messaging
	private NetworkStream _msgStream = null;
	private Dictionary<string, Func<IPacket, Task>> _commandHandlers = new Dictionary<string, Func<IPacket, Task>>();

	public TcpGamesClient(string serverAddress, int port)
	{
		// Create a non-connectec TcpClient
		_client = new TcpClient();
		Running = false;

		// Set other data
		ServerAddress = serverAddress;
		Port = port;
	}

	// Cleans up any leftover network resources
	private void _cleanupNetworkResources()
	{
		_msgStream?.Close();
		_msgStream = null;
		_client.Close();
	}

	// Connects to the games server
	public void Connect()
	{
		// Connect to the server
		try
		{
			_client.Connect(ServerAddress, Port);   // Resolves DNS for us
		}
		catch (SocketException se)
		{
			Console.WriteLine("[ERROR] {0}", se.Message);
		}

		// check that we've connected
		if (_client.Connected)
		{
			// Connected!
			Console.WriteLine("Connected to the server at {0}.", _client.Client.RemoteEndPoint);
			Running = true;

			// Get the message stream
			_msgStream = _client.GetStream();
			//_sendPacket(new MessagePacket("I'm here!")).GetAwaiter().GetResult();
			// Hook up some packet command handlers
			_commandHandlers["bye"] = _handleBye;
			_commandHandlers["message"] = _handleMessage;
			_commandHandlers["input"] = _handleInput;
			_commandHandlers["ping"] = _handlePing;
			_commandHandlers["connect"] = _handleConnect;
			_commandHandlers["unknown"] = _unknownCommand;
		}
		else
		{
			// Nope...
			_cleanupNetworkResources();
			Console.WriteLine("Wasn't able to connect to the server at {0}:{1}.", ServerAddress, Port);
		}
	}

	// Requests a disconnect, will send a "bye," message to the server
	// This should only be called by the user
	public void Disconnect()
	{
		Console.WriteLine("Disconnecting from the server...");
		Running = false;
		_clientRequestedDisconnect = true;
		_msgStream = _client.GetStream();
		_sendPacket(new MessagePacket("bye")).GetAwaiter().GetResult();
	}

	// Main loop for the Games Client
	public void Run()
	{
		bool wasRunning = Running;

		// Listen for messages
		List<Task> tasks = new List<Task>();
		while (Running)
		{
			// Check for new packets
			_handleIncomingPackets().GetAwaiter().GetResult();

			// Use less CPU
			Thread.Sleep(10);

			// Make sure that we didn't have a graceless disconnect
			if (_isDisconnected(_client) && !_clientRequestedDisconnect)
			{
				Running = false;
				Console.WriteLine("The server has disconnected from us ungracefully.\n:[");
			}
		}

		// Just incase we have anymore packets, give them one second to be processed
		Task.WaitAll(tasks.ToArray(), 1000);

		// Cleanup
		_cleanupNetworkResources();
		if (wasRunning)
			Console.WriteLine("Disconnected.");
	}

	// Sends packets to the server asynchronously
	private async Task _sendPacket(IPacket packet)
	{
		try
		{
			packet.SessionId = _sessionId;
			// convert JSON to buffer and its length to a 16 bit unsigned integer buffer
			string jsonPacket = packet.ToJson();
			Console.WriteLine($"Sending:{jsonPacket}");
			byte[] jsonBuffer = Encoding.UTF8.GetBytes(jsonPacket);
			byte[] lengthBuffer = BitConverter.GetBytes(Convert.ToUInt16(jsonBuffer.Length));

			// Join the buffers
			byte[] packetBuffer = new byte[lengthBuffer.Length + jsonBuffer.Length];
			lengthBuffer.CopyTo(packetBuffer, 0);
			jsonBuffer.CopyTo(packetBuffer, lengthBuffer.Length);

			// Send the packet
			await _msgStream.WriteAsync(packetBuffer, 0, packetBuffer.Length);

			//Console.WriteLine("[SENT]\n{0}", packet);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	// Checks for new incoming messages and handles them
	// This method will handle one Packet at a time, even if more than one is in the memory stream
	private async Task _handleIncomingPackets()
	{
		try
		{
			// Check for new incomding messages
			if (_client.Available > 0)
			{
				// There must be some incoming data, the first two bytes are the size of the Packet
				byte[] lengthBuffer = new byte[2];
				await _msgStream.ReadAsync(lengthBuffer, 0, 2);
				ushort packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);

				// Now read that many bytes from what's left in the stream, it must be the Packet
				byte[] jsonBuffer = new byte[packetByteSize];
				await _msgStream.ReadAsync(jsonBuffer, 0, jsonBuffer.Length);

				// Convert it into a packet datatype
				string jsonString = Encoding.UTF8.GetString(jsonBuffer);
				Console.WriteLine("Received:" + jsonString);
				var packet = JsonConvert.DeserializeObject<Packet>(jsonString);
				_sessionId = packet.SessionId;
				// Dispatch it
				if(_commandHandlers.ContainsKey(packet.Command))
					await _commandHandlers[packet.Command](packet);
				else
				{
					await _commandHandlers["unknown"](packet);
				}

			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	#region Command Handlers
	private Task _handleConnect(IPacket arg)
	{
		_sessionId = arg.SessionId;
		return Task.FromResult(0);
	}
	private Task _unknownCommand(IPacket arg)
	{
		Console.WriteLine(arg.ToJson());
		return Task.FromResult(0);
	}
	private async Task _handlePing(IPacket arg)
	{
		await _sendPacket(new Packet() {Command = "pong"});
	}
	private Task _handleBye(IPacket message)
	{
		// Print the message
		Console.WriteLine("The server is disconnecting us with this message:");
		Console.WriteLine(message.Data.ToJson());

		// Will start the disconnection process in Run()
		Running = false;
		return Task.FromResult(0);  // Task.CompletedTask exists in .NET v4.6
	}

	// Just prints out a message sent from the server
	private Task _handleMessage(IPacket message)
	{

		Console.Write(message.ToJson());
		return Task.FromResult(0);  // Task.CompletedTask exists in .NET v4.6
	}

	// Gets input from the user and sends it to the server
	private async Task _handleInput(IPacket message)
	{
		// Print the prompt and get a response to send
		Console.Write(message);
		string responseMsg = Console.ReadLine();

		// Send the response
		IPacket resp = new MessagePacket(responseMsg);
		await _sendPacket(resp);
	}
	#endregion // Command Handlers

	#region TcpClient Helper Methods
	// Checks if a client has disconnected ungracefully
	// Adapted from: http://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket
	private static bool _isDisconnected(TcpClient client)
	{
		try
		{
			Socket s = client.Client;
			return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
		}
		catch (SocketException)
		{
			// We got a socket error, assume it's disconnected
			return true;
		}
	}
	#endregion // TcpClient Helper Methods

}
