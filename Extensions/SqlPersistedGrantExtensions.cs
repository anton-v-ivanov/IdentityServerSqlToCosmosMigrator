// // Copyright (c) BandLab Technologies. All rights reserved.

using System;

using IS2CosmosMigrator.Entities;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json.Linq;

namespace IS2CosmosMigrator.Extensions
{
    public static class SqlPersistedGrantExtensions
    {
        public static CosmosPersistedGrant ToDestination(this SqlPersistedGrant source)
        {
            return new CosmosPersistedGrant
            {
                Key = Base64UrlEncoder.Encode(source.Key),
                SubjectId = source.SubjectId,
                ClientId = source.ClientId,
                CreationTime = source.CreationTime,
                Expiration = source.Expiration,
                Type = source.Type,
                Data = JObject.Parse(source.Data),
                TimeStamp = ((DateTimeOffset)source.CreationTime).ToUnixTimeSeconds(),
                Ttl = source.Expiration == null
                      ? (int?)null
                      : (int)(source.Expiration.Value - DateTime.UtcNow).TotalSeconds
            };
        }
    }
}
