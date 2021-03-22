using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeDashboard
{
	public static class RealTimeDashboard
	{
		// This will manage connections to SignalR
		[FunctionName("negotiate")]
		public static SignalRConnectionInfo GetConnectionInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post")]
			HttpRequest req,
			[SignalRConnectionInfo(HubName = "Dashboard")]
			SignalRConnectionInfo connectionInfo)
		{
			return connectionInfo;
		}

		[FunctionName("Dashboard")]
		public static Task AcceptAndBroadcastDataToDashBoard
		(
			[CosmosDBTrigger(databaseName: "SampleDb",
				collectionName: "weather",
				ConnectionStringSetting = "CosmosDbConnectionstring",
				LeaseCollectionName = "leases")]
			IReadOnlyList<Document> documents,
			[SignalR(HubName = "Dashboard")] IAsyncCollector<SignalRMessage> signalrMessageForDashboard,
			ILogger log
		 )
		{
			var message = documents?.Select(doc => new DashboardMessage
			{ Id = doc.GetPropertyValue<string>("CityId"), Details = doc.GetPropertyValue<string>("temprature") })
				.ToList();

			log.LogInformation($"Message arrived from Cosmos Db");

			return signalrMessageForDashboard.AddAsync(new SignalRMessage
			{ Target = "dashboardMessage", Arguments = new[] { message } });
		}
	}
}