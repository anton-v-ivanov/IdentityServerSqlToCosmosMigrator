// // Copyright (c) BandLab Technologies. All rights reserved.

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IS2CosmosMigrator.Entities
{
    public class CosmosPersistedGrant
    {
        [JsonProperty("id")]
        public string Key { get; set; }

        public string Type { get; set; }

        public string SubjectId { get; set; }

        public string ClientId { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? Expiration { get; set; }

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? Ttl { get; set; }

        [JsonProperty("_ts")]
        public long TimeStamp { get; set; }

        public JObject Data { get; set; }
    }
}
