using Newtonsoft.Json;

namespace Common
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
