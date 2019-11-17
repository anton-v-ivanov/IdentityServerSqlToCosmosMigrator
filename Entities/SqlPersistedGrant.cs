// // Copyright (c) BandLab Technologies. All rights reserved.

using System;

namespace IS2CosmosMigrator.Entities
{
    public class SqlPersistedGrant
    {
        public string Key { get; set; }

        public string Type { get; set; }

        public string SubjectId { get; set; }

        public string ClientId { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? Expiration { get; set; }

        public string Data { get; set; }
    }
}
