namespace Common.Packets
{
	public interface IPacket<T> : IPacket
	{
		T Data { get; set; }
	}

    public interface IPacket : IJsonSerializable
    {
        string SessionId { get; set; }
        string Command { get; set; }
    }
}
