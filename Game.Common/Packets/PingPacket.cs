using System;

namespace Common.Packets
{
    public class PingPacket : Packet
    {
        public PingPacket()
        {
            Command = "ping";
        }
    }
}

