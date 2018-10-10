using System.Threading.Tasks;
using Contoso.Common.Services;
using Contoso.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Contoso.CreateO365User.UserOrchestration
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A 365 new user orchestration. </summary>
    ///
    /// <remarks>   Sebastian Schütze, 07/04/2018. </remarks>
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
		        Configs.UserAdministrationSharePointListId, 
		        true
		    );

	        var sendPasswordQueue = new QueueService(Configs.QueueConnectionString, Configs.SendPasswordQueueName);
			var addUserToGroupQueue = new QueueService(Configs.QueueConnectionString, Configs.AddToGroupUsersQueueName);

	        var exchangeOnlineService = new ExchangeOnlineService(
		        addUserToGroupQueue
	        );

			foreach (var currUserItem in currUserItems)
			{
				var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);

				await graphService.DeleteUserByPrincipalNameAsync(user.UserPrincipalName, false);

				var userId = await graphService.CreateUserAsync(user);
				
				var createdUser = await graphService.AssignE2LicenseToUserById(userId,Configs.DefaultO365UserLicense);
				
				await exchangeOnlineService.AddUserToGroupAsync(user.UserPrincipalName,Configs.DefaultExchangeGroupId);
				
				await sendPasswordQueue.CreateEncryptedMessageAsync($"{user.UserPrincipalName}|{user.PasswordProfile.Password}");
			}

			return "Email sent!";
        }

       
    }
}
