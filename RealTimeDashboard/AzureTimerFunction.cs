using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using RealTimeDashboard.Shared;

namespace RealTimeDashboard
{
    public static class AzureTimerFunction
    {
        [FunctionName("AzureTimerFunction")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
		//	await new CosmosDbOperation().Add().ConfigureAwait(false);

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
