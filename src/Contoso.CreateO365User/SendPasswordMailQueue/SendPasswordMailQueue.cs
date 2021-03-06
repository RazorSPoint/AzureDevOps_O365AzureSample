using System;
using System.IO;
using System.Threading.Tasks;
using Contoso.Common.Services;
using Contoso.Common.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Contoso.CreateO365User.SendPasswordMailQueue
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

	        var decodedQueue = Security.ToInsecureString(Security.DecryptString(myQueueItem)).Split('|');
	        var userPrincipalName = decodedQueue[0];
	        var userPassword = decodedQueue[1];

			log.LogInformation($"Processing the password mail for user: {userPrincipalName}");

			var graphService = new GraphService(graphToken, azureFunctionsLogger);
	        var emailService = new EmailService(graphService, Path.Combine(context.FunctionDirectory, "..", "Templates"));
	        Task.Run(async () =>
	        {
		        var user = await graphService.GetUserAsync(userPrincipalName);
		        var admin = await graphService.GetUserAsync(Configs.UserEmailSender);
		        var userCopyMail = await graphService.GetUserAsync(Configs.UserEmailPasswordCopy);

				var isExchangeDeployed = await graphService.IsServicePlanFromUserActiveAndDeployed(userPrincipalName, Configs.DefaultExchangeOnlineLicense);

				if (!isExchangeDeployed)
				{
					throw new Exception($"The Exchange is not yet assigned or deployed for user '{userPrincipalName}'");
				}

		        await emailService.SendPasswordMailAsync(user, userCopyMail, admin, userPassword);

	        }).GetAwaiter().GetResult();

        }
    }
}
