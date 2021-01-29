using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace RealTimeDashboard
{
    public static class Function1
    {

        // This will manage connections to SignalR
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "Dashboard")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        //Send the messages!
        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] ClientMessage clientMessage, [SignalR(HubName = "chatHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "clientMessage",
                    Arguments = new[] { clientMessage }
                });
        }

        [FunctionName("Dashboard")]
        public static Task SendDataToDashBoard([CosmosDBTrigger(
        databaseName: "SampleDb",
        collectionName: "SampleCollection",
        ConnectionStringSetting = "CosmosDbConnectionstring",
        LeaseCollectionName = "leases")]IReadOnlyList<Document> input, [SignalR(HubName = "Dashboard")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }

            var dashboardMessage= new DashboardMessage()
            {
                Id = input?[0].GetPropertyValue<string>("id"),
                Details = input?[0].GetPropertyValue<string>("details")
            };

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "dashboardMessage",
                    Arguments = new[] { dashboardMessage }
                });
        }
    }

    public class ClientMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class DashboardMessage
    {
        public string Id { get; set; }
        public string Details { get; set; }
    }



}



