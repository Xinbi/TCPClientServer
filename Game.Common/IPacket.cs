namespace Common
{
	public interface IPacket : IJsonSerializable
	{
		string SessionId { get; set; }
		string Command { get; set; }
		IPacketData Data { get; set; }
	}
}
