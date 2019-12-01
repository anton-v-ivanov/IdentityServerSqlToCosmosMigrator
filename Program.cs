﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;

using Dapper;

using IS2CosmosMigrator.Entities;
using IS2CosmosMigrator.Extensions;
using IS2CosmosMigrator.Settings;

using Microsoft.Azure.Cosmos;

namespace IS2CosmosMigrator
{
    class Program
    {
        private static Container _container;

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(opt =>
                {
                    try
                    {
                        SetupCosmosDbContainerAsync(opt).GetAwaiter().GetResult();

                        var totalCount = GetGrantsToMigrateCountAsync(opt).GetAwaiter().GetResult();
                        Console.WriteLine($"Total grants: {totalCount}");

                        MigrateDataAsync(opt, totalCount, DateTime.UtcNow).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.ReadKey();
                    }
                });

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task SetupCosmosDbContainerAsync(CommandLineOptions opt)
        {
            var client = new CosmosClient(opt.CosmosDbConnectionString,
                new CosmosClientOptions
                {
                    AllowBulkExecution = true,
                    ConnectionMode = ConnectionMode.Direct
                });

            Database database = await client.CreateDatabaseIfNotExistsAsync(opt.CosmosDbDatabaseName);
            _container = await database.CreateContainerIfNotExistsAsync(
                opt.CosmosDbContainerName,
                "/PartitionKey",
                400);
        }

        private static async Task<int> GetGrantsToMigrateCountAsync(CommandLineOptions opt)
        {
            var connectionString = opt.SqlConnectionString;
            await using var db = new SqlConnection(connectionString);
            return await db.ExecuteScalarAsync<int>(new CommandDefinition(
                @"select count(1) from PersistedGrants
where (@time is null or CreationTime > @time)",
                new { time = opt.StartTime },
                commandTimeout: 300));
        }

        private static async Task MigrateDataAsync(CommandLineOptions opt, int totalCount, DateTime now)
        {
            var count = 0;
            var added = 0;
            while (true)
            {
                var sourceGrants = (await GetGrantsFromSourceAsync(opt, count)).ToArray();
                if (!sourceGrants.Any())
                {
                    break;
                }

                var destinationGrants = sourceGrants
                    .Where(s => s.Expiration == null || s.Expiration > now)
                    .Select(s => s.ToDestination(opt.PartitionCount))
                    .ToArray();

                await SaveToDestinationAsync(destinationGrants);

                added += destinationGrants.Length;
                count += sourceGrants.Length;
                var percentage = (double)count / totalCount;
                Console.WriteLine($"Processed {count} of {totalCount} grants ({percentage:P}), added: {destinationGrants.Length}");
            }

            Console.WriteLine($"Total added grants: {added}");
        }

        private static async Task<IEnumerable<SqlPersistedGrant>> GetGrantsFromSourceAsync(CommandLineOptions opt,
            int offset)
        {
            var connectionString = opt.SqlConnectionString;
            await using var db = new SqlConnection(connectionString);
            return await db.QueryAsync<SqlPersistedGrant>(new CommandDefinition(
                @"select * from PersistedGrants
where (@time is null or CreationTime > @time)
order by CreationTime asc
offset @offset ROWS
fetch next @batch rows only",
                new
                {
                    batch = opt.BatchSize,
                    offset,
                    time = opt.StartTime
                },
                commandTimeout:300));
        }

        private static async Task SaveToDestinationAsync(IEnumerable<CosmosPersistedGrant> grants)
        {
            var tasks = grants.Select(grant => _container.UpsertItemAsync(grant));
            await Task.WhenAll(tasks);
        }
    }
}
