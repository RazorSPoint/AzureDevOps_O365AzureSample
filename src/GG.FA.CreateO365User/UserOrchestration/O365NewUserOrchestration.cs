using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using GG.FA.Common.Services;
using GG.FA.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace GG.FA.CreateO365User
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A 365 new user orchestration. </summary>
    ///
    /// <remarks>   Sebastian Sch�tze, 07/04/2018. </remarks>
    ///
    /// <seealso cref="https://github.com/microsoftgraph/uwp-csharp-snippets-sample/blob/master/Microsoft-Graph-Snippets-SDK/Groups/GroupSnippets.cs"/>
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/azure/ad/graph/api/users-operations#CreateUser"/>
    /// <seealso cref="https://github.com/Azure/azure-functions-microsoftgraph-extension/blob/master/samples/GraphExtensionSamples/TokenExamples.cs"/>
    /// <seealso cref="https://login.microsoftonline.com/common/adminconsent?client_id=***REMOVED***&state=12345&redirect_uri=https://***REMOVED***.azurewebsites.net/.auth/login/aad/callback"/>
    /// <seealso cref="https://resources.azure.com"/>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class O365NewUserOrchestration
    {
        [FunctionName("O365NewUserOrchestration")]
        public static async Task<string> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = null
            )]
            HttpRequest req,
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
	        GraphService graphService = new GraphService(graphToken, azureFunctionsLogger);
			
	        var currUserItems = await graphService.GetUserFromSpUserListAsync(
		        Configs.UserAdministrationGraphSiteId, 
		        Configs.UserAdminstrationSharePointListId, 
		        true
		    );

	        var sendPasswordQueue = new QueueService(Configs.QueueConnectionString, Configs.SendPasswordQueueName);
			var addUserToGroupQueue = new QueueService(Configs.QueueConnectionString, Configs.AddToGroupUsersQueueName);

			var exchangeOnlineService = new ExchangeOnlineService(
		        addUserToGroupQueue,
		        Configs.O365AdminUser,
		        Security.ToSecureString(Configs.O365AdminPassword)
	        );

			foreach (var currUserItem in currUserItems)
			{
				var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);

				await graphService.DeleteUserByPrincipalNameAsync(user.UserPrincipalName, false);

				var userId = await graphService.CreateUserAsync(user);
				
				var createdUser = await graphService.AssignE2LicenseToUserById(userId);
				
				//await exchangeOnlineService.AddUserToWerteAkademieGroupAsync(user.UserPrincipalName);
				
				//await sendPasswordQueue.CreateEncryptedMessageAsync($"{user.UserPrincipalName}|{user.PasswordProfile.Password}");
			}

			return "Email sent!";
        }

       
    }
}
