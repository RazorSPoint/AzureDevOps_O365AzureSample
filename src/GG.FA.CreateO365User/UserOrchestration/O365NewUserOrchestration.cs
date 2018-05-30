using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GG.FA.Common;
using Microsoft.Graph;
using System.Text;
using GG.FA.Common.Services;
using GG.FA.Common.Utilities;
using GG.FA.Model;

namespace GG.FA.CreateO365User
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
            TraceWriter log,
	        ExecutionContext context)
        {

            var azureFunctionsLogger = new AzureFunctionLogger(log);
            var graphService = new GraphService(graphToken, azureFunctionsLogger);
	        var exchangeOnlineService = new ExchangeOnlineService(
		        "https://gg-fa-mytestapp-powershell.azurewebsites.net/api/GG-FA-AddUserToGroup",
		        "zB1tzh78v3HL446pjmaxAsPs5SUtlesKLXm3Fl01XQfIqT0ePh2MMg==",
		        "sebastian.schuetze@***REMOVED***",
		        Security.ToSecureString("***REMOVED***")
	        );

            // site: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration
            var siteId = "***REMOVED***,***REMOVED***,***REMOVED***";
            // list: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration/Lists/UserInventory
            var listId = "***REMOVED***";

            var currUserItems = await graphService.GetUserFromSpUserListAsync(siteId, listId, true);

			var connectionString = System.Environment.GetEnvironmentVariable("QueueConnectionString", EnvironmentVariableTarget.Process);
				
			var sendPasswordQueue = new QueueService(connectionString, "sendpasswordqueue-items");
			
			foreach (var currUserItem in currUserItems)
			{
				var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);
				
				var userId = await graphService.CreateUserAsync(user);
				
				var createdUser = await graphService.AssignE2LicenseToUserById(userId);
				
				await exchangeOnlineService.AddUserToWerteAkademieGroupAsync(userId);

				await sendPasswordQueue.CreateEncryptedMessageAsync($"{user.UserPrincipalName}|{user.PasswordProfile.Password}");
			}
			

			return "Email sent!";
        }

       
    }
}
