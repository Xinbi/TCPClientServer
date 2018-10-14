using Newtonsoft.Json;
using System;

namespace Game.Common
{
	public class PacketData : IPacketData
	{
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.None,
				new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}
	}
}
