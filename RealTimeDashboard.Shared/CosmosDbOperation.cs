using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace RealTimeDashboard.Shared
{
    public class CosmosDbOperation
    {
        public async Task Add()
        {
            using (var client = new DocumentClient(new Uri("https://localhost:8081"),
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="))
            {
                var databaseDefinition = new Database {Id = "SampleDb"};
                var database = await client.CreateDatabaseIfNotExistsAsync(databaseDefinition);
                Console.WriteLine("Database testDb created successfully");

                Console.WriteLine("\r\n>>>>>>>>>>>>>>>> Creating Collection <<<<<<<<<<<<<<<<<<<");
                var collectionDefinition = new DocumentCollection
                {
                    Id = "weather",
                    PartitionKey = new PartitionKeyDefinition() {Paths = new Collection<string>() {"/weather/id"}}
                };
                var collection = await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri("SampleDb"),
                    collectionDefinition);
                Console.WriteLine("Collection testDocumentCollection created successfully");
                dynamic doc1Definition = new
                {
                    id = Guid.NewGuid().ToString("N"),
                    CityId = "1",
                    temprature = new Random().Next(0, 50).ToString()
                };

                var document1 = await client.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri("SampleDb", "weather"),
                    doc1Definition);

                dynamic doc2Definition = new
                {
                    id = Guid.NewGuid().ToString("N"),
                    CityId = "2",
                    temprature = new Random().Next(0, 50).ToString()
                };


                var document2 = await client.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri("SampleDb", "weather"),
                    doc2Definition);
            }
        }
    }
}


public interface ICosmosDbOperation
{
}