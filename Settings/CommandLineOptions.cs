// // Copyright (c) BandLab Technologies. All rights reserved.

using CommandLine;

namespace IS2CosmosMigrator.Settings
{
    public class CommandLineOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source MSSQL database connection string")]
        public string SqlConnectionString { get; set; }

        [Option('t', "table", Required = false, Default = "PersistedGrants", HelpText = "Persisted grants table name")]
        public string SourceTableName { get; set; }

        [Option('b', "batch", Required = false, Default = 100, HelpText = "Batch size for source table")]
        public int BatchSize { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination CosmosDB connection string")]
        public string CosmosDbConnectionString { get; set; }

        [Option("db", Required = false, Default = "identitydb", HelpText = "CosmosDB destination database name")]
        public string CosmosDbDatabaseName { get; set; }

        [Option('c', "container", Required = false, Default = "grants", HelpText = "CosmosDB destination container name")]
        public string CosmosDbContainerName { get; set; }
    }
}
