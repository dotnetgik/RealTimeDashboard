using System;
using System.Threading.Tasks;
using RealTimeDashboard.Shared;

namespace CosmosDbCrudTrigger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                await new CosmosDbOperation().Add().ConfigureAwait(false);
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
}