using System;
using System.Collections.Generic;
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

            // site: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration
            var siteId = "***REMOVED***,***REMOVED***,***REMOVED***";
            // list: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration/Lists/UserInventory
            var listId = "***REMOVED***";

            var spUser = await graphService.GetUserFromSpUserListAsync(siteId, listId, true);

	        var emailService = new EmailService(graphService);

	        var emailString = "sebastian.schuetze@***REMOVED***";
	        var displayName = "Sebastian Schütze";
	        var toMail = new[] { emailString + ";Sebastian Schütze" };
	        var body = emailService.GetMailTemplateByFile($@"{context.FunctionDirectory}\..\Templates\WK_EmailTemplate.html")
		        .Replace("#GGEmail#", emailString)
		        .Replace("#GGPassword#", "MyPassword")
		        .Replace("#GGDisplayName#", displayName);

	        var subject = "Test Subject";

	        emailService.SendMailAsync(emailString, toMail, toMail, emailString, subject, body);


			//foreach (var currUserItem in spUser)
   //         {
   //             var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);

   //             var userId = await graphService.CreateUserAsync(user);

   //             user = await graphService.AssignE2LicenseToUserById(userId);

   //             //security group for 'Beiräte'
   //             var securityGroup = "d5a50ddc-739e-46ac-97ee-9569872ea644";
   //             //security group for 'Werteakademie member'
   //             //var securityGroup = "03f2689e-3879-413b-ab9e-002d16b72641"
   //             await graphService.AddUserToGroupAsync(userId, securityGroup);
				
			//}
            

           

            return "true!";
        }

       
    }
}
