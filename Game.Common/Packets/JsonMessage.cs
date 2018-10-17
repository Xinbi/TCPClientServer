using Newtonsoft.Json;
using System;

namespace Common.Packets
{
    public class JsonMessage : IJsonSerializable
    {

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

    }
}

