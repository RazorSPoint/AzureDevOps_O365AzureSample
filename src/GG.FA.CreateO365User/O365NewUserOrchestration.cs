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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="https://github.com/microsoftgraph/uwp-csharp-snippets-sample/blob/master/Microsoft-Graph-Snippets-SDK/Groups/GroupSnippets.cs"/>
    /// <seealso cref="https://msdn.microsoft.com/en-us/library/azure/ad/graph/api/users-operations#CreateUser"/>
    /// <seealso cref="https://github.com/Azure/azure-functions-microsoftgraph-extension/blob/master/samples/GraphExtensionSamples/TokenExamples.cs"/>
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
            TraceWriter log)
        {

            var azureFunctionsLogger = new AzureFunctionLogger(log);
            var graphService = new GraphService(graphToken, azureFunctionsLogger);

            // site: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration
            var siteId = "***REMOVED***,***REMOVED***,***REMOVED***";
            // list: ***REMOVED***/sites/gut-goedelitz/UserAdminiatration/Lists/UserInventory
            var listId = "***REMOVED***";

            var spUser = await graphService.GetUserFromSpUserListAsync(siteId, listId, true);

            foreach (var currUserItem in spUser)
            {
                var user = GraphService.GetAdUserObjectFromUserListItem(currUserItem);

                return "stopped here";

                var userId = await graphService.CreateUserAsync(user);

                var skus = await graphService.GraphClient.SubscribedSkus.Request().GetAsync();
                var e2Sku = skus.FirstOrDefault(sku => sku.SkuPartNumber.Equals("STANDARDWOFFPACK"));

                var assignedLicense = new List<AssignedLicense>() { new AssignedLicense { SkuId = e2Sku.SkuId } };

                user = await graphService.GraphClient.Users[userId].AssignLicense(assignedLicense, new Guid[0]).Request()
                    .PostAsync();

            }
            
           

            return "true!";
        }
    }
}
