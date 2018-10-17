using System;
using Newtonsoft.Json;

namespace Common.Packets
{
    public class Packet<T> : Packet
    {
        public virtual T Data { get; set; }
    }

    public class Packet : JsonMessage, IPacket
	{
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }
        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }


	}
}

