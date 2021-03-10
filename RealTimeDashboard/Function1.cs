using System;
using System.Collections.Generic;
using System.Linq;
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


		[FunctionName("Dashboard")]
		public static Task SendDataToDashBoard([CosmosDBTrigger(databaseName: "SampleDb",
		collectionName: "weather",
		ConnectionStringSetting = "CosmosDbConnectionstring",
		LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
			[SignalR(HubName = "Dashboard")] IAsyncCollector<SignalRMessage> signalrMessageForDashboard,
			ILogger log)
		{

			var message = input.Select(doc => new DashboardMessage {Id = doc.GetPropertyValue<string>("CityId"), Details = doc.GetPropertyValue<string>("temprature")}).ToList();

			return signalrMessageForDashboard.AddAsync(new SignalRMessage {Target = "dashboardMessage",Arguments = new[] { message } });
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



