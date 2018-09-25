using System;
using System.IO;
using System.Threading.Tasks;
using GG.FA.Common.Services;
using GG.FA.Common.Utilities;
using GG.FA.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GG.FA.CreateO365User
{
    public static class SendPasswordMailQueue
    {
        [FunctionName("SendPasswordMailQueue")]
        public static void Run(
	        [QueueTrigger(
		        "sendpasswordqueue-items", 
		        Connection = "QueueConnectionString"
			)]
	        string myQueueItem, 
	        [Token(
		        Identity = TokenIdentityMode.ClientCredentials,
		        IdentityProvider = "AAD",
		        Resource = "https://graph.microsoft.com"
	        )]
	        string graphToken,
	        ILogger log,
	        ExecutionContext context)
        {
			var azureFunctionsLogger = new AzureFunctionLogger(log);

	        log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
	        log.LogInformation($"C# Queue trigger function processed: {graphToken}");

	        var decodedQueue = Security.ToInsecureString(Security.DecryptString(myQueueItem)).Split('|');
	        var userPrincipalName = decodedQueue[0];
	        var userPassword = decodedQueue[1];

			log.LogInformation($"C# Queue trigger function processed: {userPrincipalName}");
			log.LogInformation($"C# Queue trigger function processed: {userPassword}");

			var graphService = new GraphService(graphToken, azureFunctionsLogger);
	        var emailService = new EmailService(graphService, Path.Combine(context.FunctionDirectory, "..", "Templates"));
	        Task.Run(async () =>
	        {
		        var user = await graphService.GetUserAsync(userPrincipalName);
		        var admin = await graphService.GetUserAsync("sebastian.schuetze@***REMOVED***");

				var isExchangeDeployed = await graphService.IsServicePlanFromUserActiveAndDeployed(userPrincipalName, "EXCHANGE_S_STANDARD");

				if (!isExchangeDeployed)
				{
					throw new Exception($"The Exchange is not yet assigned or deployed for user '{userPrincipalName}'");
				}
				else
				{
					emailService.SendPasswordMailAsync(user, admin, userPassword);
				}		

	        }).GetAwaiter().GetResult();

        }
    }
}
