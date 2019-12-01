// // Copyright (c) BandLab Technologies. All rights reserved.

using System;
using System.Security.Cryptography;
using System.Text;

using IS2CosmosMigrator.Entities;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json.Linq;

namespace IS2CosmosMigrator.Extensions
{
    public static class SqlPersistedGrantExtensions
    {
        public static CosmosPersistedGrant ToDestination(this SqlPersistedGrant source, int partitionCount) =>
            new CosmosPersistedGrant
            {
                Key = ToId(source.Key),
                PartitionKey = ToPartitionKey(source.Key, partitionCount),
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

        private static string ToId(string key) => Base64UrlEncoder.Encode(key);

        private static string ToPartitionKey(string key, int partitionCount)
        {
            var partition = BitConverter.ToInt32(Encoding.UTF8.GetBytes(key), 0);
            var bucket = partition % partitionCount;
            var hash = SHA256.Create().ComputeHash(BitConverter.GetBytes(bucket));

            var result = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }

            return result.ToString();
        }
    }
}
